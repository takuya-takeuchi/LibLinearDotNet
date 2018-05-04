using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LibLinearDotNet;
using Microsoft.Extensions.CommandLineUtils;

namespace FindParameterC
{

    internal class Program
    {

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(FindParameterC);
            app.Description = "The exsample program for FindParameterC";
            app.HelpOption("-h|--help");

            var quietArgument = app.Argument("quiet", "Suppress output of LIBLINEAR");
            var solverOption = app.Option("-s|--solver", "solver", CommandOptionType.SingleValue);
            var foldOption = app.Option("-f|--fold", "K-fold. (An integer of not less than 0)", CommandOptionType.SingleValue);
            var biasOption = app.Option("-b|--bias", "bias", CommandOptionType.SingleValue);
            var startCOption = app.Option("-sc|--startc", "start of C parameter", CommandOptionType.SingleValue);
            var maxCOption = app.Option("-mc|--maxc", "max of C parameter", CommandOptionType.SingleValue);

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

                if (!int.TryParse(foldOption.Value(), out var fold))
                {
                    app.ShowHelp();
                    return -1;
                }

                if (!int.TryParse(startCOption.Value(), out var startC))
                {
                    app.ShowHelp();
                    return -1;
                }

                if (!int.TryParse(maxCOption.Value(), out var maxC))
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

                var temp = Path.GetTempPath();
                var trainDic = new StringBuilder();

                for (var l = 0; l < 10; l++)
                {
                    for (var i = 0; i < trainCount; i++)
                    {
                        // Create decimal value 0 or greater but less than 10
                        var v = r.NextDouble() + l;
                        trainDic.AppendLine($"{l} 1:{v}");
                    }
                }

                var tempTrainPath = Path.Combine(temp, "train");

                using (var fs = new FileStream(tempTrainPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var sw = new StreamWriter(fs, Encoding.ASCII))
                    sw.Write(trainDic.ToString());

                // Load training data and test data set
                using (var train = Problem.FromFile(tempTrainPath, bias))
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

                    // Train training data
                    var sw = new Stopwatch();
                    sw.Start();
                    LibLinear.FindParameterC(train, param, fold, startC, maxC, out var bestC, out var bestRate);
                    sw.Stop();

                    Console.Write($"BestC: {bestC}, BestRate: {bestRate}, Elapsed: {sw.ElapsedMilliseconds}ms");
                }

                return 0;
            });

            app.Execute(args);
        }

    }

}
