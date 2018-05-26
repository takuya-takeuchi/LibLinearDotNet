using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LibLinearDotNet;
using Microsoft.Extensions.CommandLineUtils;

namespace Classification2
{

    internal class Program
    {

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(Classification2);
            app.Description = "The exsample program for classification";
            app.HelpOption("-h|--help");

            var quietArgument = app.Argument("quiet", "Suppress output of LIBLINEAR");
            var solverOption = app.Option("-s|--solver", "solver", CommandOptionType.SingleValue);
            var biasOption = app.Option("-b|--bias", "bias", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (quietArgument.Value != null)
                    LibLinear.SetPrintFunction(null);

                var biasStr = "-1";
                if (biasOption.HasValue())
                    biasStr = biasOption.Value();

                if (!double.TryParse(biasStr, out var bias))
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

                // Create random data
                var r = new Random();
                const int trainCount = 500;
                const int testCount = 100;

                var trainNodes = new List<FeatureNode[]>();
                var trainLabels = new List<double>();
                var testNodes = new List<FeatureNode[]>();
                var testLabels = new List<double>();

                for (var l = 0; l < 2; l++)
                {
                    for (var i = 0; i < trainCount; i++)
                    {
                        // Create decimal value 0 or greater but less than 2
                        var v = r.NextDouble() + l;
                        trainNodes.Add(new[] { new FeatureNode { Index = 0, Value = v } });
                        trainLabels.Add(l);
                    }
                    for (var i = 0; i < testCount; i++)
                    {
                        // Create decimal value 0 or greater but less than 2
                        var v = r.NextDouble() + l;
                        testNodes.Add(new[] { new FeatureNode { Index = 0, Value = v } });
                        testLabels.Add(l);
                    }
                }

                // Load training data and test data set
                using (var train = Problem.FromSequence(trainNodes, trainLabels, bias))
                using (var test = Problem.FromSequence(testNodes, testLabels, bias))
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

                    message = LibLinear.CheckParameter(test, param);
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine($"Error: {message} for test problem");
                        return -1;
                    }

                    // Train training data
                    var sw = new Stopwatch();
                    sw.Start();
                    using (var model = LibLinear.Train(train, param))
                    {
                        sw.Stop();

                        var correct = 0;
                        var total = 0;
                        var x = test.X;

                        double error = 0;
                        double sump = 0;
                        double sumt = 0;
                        double sumpp = 0;
                        double sumtt = 0;
                        double sumpt = 0;

                        for (var i = 0; i < test.Length; i++)
                        {
                            // Get vector from test data
                            var array = x[i];

                            // Get classification result (returns label)
                            var target = (int)testLabels[i];
                            var predict = (int)LibLinear.Predict(model, array);
                            if (predict == target)
                                correct++;

                            error += (predict - target) * (predict - target);
                            sump += predict;
                            sumt += target;
                            sumpp += predict * predict;
                            sumtt += target * target;
                            sumpt += predict * target;

                            total++;
                        }

                        if (model.IsRegressionModel)
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
                    }
                }

                return 0;
            });

            app.Execute(args);
        }
    }

}