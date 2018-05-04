using System;
using System.Diagnostics;
using System.Linq;
using LibLinearDotNet;
using Microsoft.Extensions.CommandLineUtils;

namespace CrossValidation
{

    internal class Program
    {

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(CrossValidation);
            app.Description = "The exsample program for cross validation";
            app.HelpOption("-h|--help");

            var quietArgument = app.Argument("quiet", "Suppress output of LIBLINEAR");
            var solverOption = app.Option("-s|--solver", "solver", CommandOptionType.SingleValue);
            var trainingOption = app.Option("-t|--training", "training set file", CommandOptionType.SingleValue);
            var foldOption = app.Option("-f|--fold", "K-fold. (An integer of not less than 0)", CommandOptionType.SingleValue);
            var biasOption = app.Option("-b|--bias", "bias", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (quietArgument.Value != null)
                    LibLinear.SetPrintFunction(null);

                var biasStr = "-1";
                if (biasOption.HasValue())
                    biasStr = biasOption.Value();

                var trainFile = trainingOption.Value();
                if (string.IsNullOrWhiteSpace(trainFile))
                {
                    app.ShowHelp();
                    return -1;
                }

                if (!double.TryParse(biasStr, out var bias))
                {
                    app.ShowHelp();
                    return -1;
                }

                if (!int.TryParse(foldOption.Value(), out var fold))
                {
                    app.ShowHelp();
                    return -1;
                }

                var solverStr = "1";
                if (solverOption.HasValue())
                    solverStr = solverOption.Value();

                if (!int.TryParse(solverStr, out var solver))
                {
                    app.ShowHelp();
                    return -1;
                }

                if (Enum.GetValues(typeof(SolverType)).Cast<int>().All(s => s != solver))
                {
                    app.ShowHelp();
                    return -1;
                }

                var sol = (SolverType)solver;

                double eps = 0;
                switch (sol)
                {
                    case SolverType.L2RegularizedLogisticRegression:
                    case SolverType.L2RegularizedL2LossSupportVectorClassification:
                        eps = 0.01;
                        break;
                    case SolverType.L2RegularizedL2LossSupportVectorRegression:
                        eps = 0.001;
                        break;
                    case SolverType.L2RegularizedL2LossSupportVectorClassificationDual:
                    case SolverType.L2RegularizedL1LossSupportVectorClassificationDual:
                    case SolverType.MulticlassSupportVectorMachineCrammerSinger:
                    case SolverType.L2RegularizedLogisticRegressionDual:
                        eps = 0.1;
                        break;
                    case SolverType.L1RegularizedL2LossSupportVectorClassification:
                    case SolverType.L1RegularizedLogisticRegression:
                        eps = 0.01;
                        break;
                    case SolverType.L2RegularizedL2LossSupportVectorRegressionDual:
                    case SolverType.L2RegularizedL1LossSupportVectorRegressionDual:
                        eps = 0.1;
                        break;
                }

                // Load training data
                using (var train = Problem.FromFile(trainFile, bias))
                {
                    // Configure parameter
                    var param = new Parameter
                    {
                        SolverType = sol,
                        C = 1d,
                        Epsilon = eps,
                        P = 0.1,
                        WeightLabel = new int[0],
                        Weight = new double[0]
                    };

                    var message = LibLinear.CheckParameter(train, param);
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine($"Error: {message} for train problem");
                        return -1;
                    }

                    // Do cross validation
                    var sw = new Stopwatch();
                    sw.Start();
                    LibLinear.CrossValidation(train, param, fold, out var targets);
                    sw.Stop();

                    var correct = 0;
                    var total = 0;
                    var y = train.Y;

                    double error = 0;
                    double sump = 0;
                    double sumt = 0;
                    double sumpp = 0;
                    double sumtt = 0;
                    double sumpt = 0;

                    for (var i = 0; i < train.Length; i++)
                    {
                        // Compare predict result and train data label
                        var target = y[i];
                        var predict = targets[i];
                        if (Math.Abs(target - predict) < double.Epsilon)
                            correct++;

                        error += (predict - target) * (predict - target);
                        sump += predict;
                        sumt += target;
                        sumpp += predict * predict;
                        sumtt += target * target;
                        sumpt += predict * target;

                        total++;
                    }

                    if (param.SolverType == SolverType.L2RegularizedL2LossSupportVectorRegression ||
                        param.SolverType == SolverType.L2RegularizedL1LossSupportVectorRegressionDual ||
                        param.SolverType == SolverType.L2RegularizedL2LossSupportVectorRegressionDual)
                    {
                        var mse = error / total;
                        var scc = (total * sumpt - sump * sumt) * (total * sumpt - sump * sumt) /
                                  ((total * sumpp - sump * sump) * (total * sumtt - sumt * sumt));
                        Console.WriteLine($"Mean squared error: {mse:f5}%, Squared correlation coefficient: {scc:f6}, Elapsed: {sw.ElapsedMilliseconds}ms");
                    }
                    else
                    {
                        var accuracy = correct / (double)total * 100;
                        Console.WriteLine($"Accuracy: {accuracy:f4}% ({correct}/{total}), Elapsed: {sw.ElapsedMilliseconds}ms");
                    }

                    return 0;
                }
            });

            app.Execute(args);
        }

    }

}
