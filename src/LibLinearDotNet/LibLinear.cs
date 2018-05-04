using System;
using System.Runtime.InteropServices;

namespace LibLinearDotNet
{

    /// <summary>
    /// Provides functions of LIBLINEAR.
    /// </summary>
    public static class LibLinear
    {

        #region Callback

        /// <summary>
        /// Encapsulates a method that has a string parameter and does not return a value.
        /// </summary>
        /// <param name="message">The message given from LIBLINEAR.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PrintFunc(string message);

        #endregion

        #region Fields

        private static GCHandle PrintFuncHandle;

        #endregion

        #region Methods

        /// <summary>
        /// Checks whether the parameters are within the feasible range of the problem.
        /// </summary>
        /// <param name="problem"><see cref="Problem"/>.</param>
        /// <param name="parameter"><see cref="Parameter"/>.</param>
        /// <returns>It returns null if the parameters are feasible, otherwise an error message is returned.</returns>
        public static string CheckParameter(Problem problem, Parameter parameter)
        {
            if (problem == null)
                throw new ArgumentNullException(nameof(problem));
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            problem.ThrowIfDisposed();

            unsafe
            {
                var param = new NativeMethods.parameter();

                try
                {
                    // This method throw exception when there is some errors.
                    // This error check is not official's.
                    param = parameter.ToNative();
                }
                catch (LibLinearException lle)
                {
                    return lle.Message;
                }

                try
                {
                    var ret = NativeMethods.check_parameter((NativeMethods.problem*)problem.NativePtr, &param);
                    return Marshal.PtrToStringAnsi(ret);
                }
                finally
                {
                    Free(&param);
                }
            }
        }

        /// <summary>
        /// Conducts cross validation.
        /// </summary>
        /// <param name="problem"><see cref="Problem"/>.</param>
        /// <param name="parameter"><see cref="Parameter"/>.</param>
        /// <param name="fold">The number of division for samples.</param>
        /// <param name="target">The predicted labels (of all prob's instances) in the validation process will be stored.</param>
        public static void CrossValidation(Problem problem, Parameter parameter, int fold, out double[] target)
        {
            target = null;

            if (problem == null)
                throw new ArgumentNullException(nameof(problem));
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            problem.ThrowIfDisposed();

            unsafe
            {
                var param = parameter.ToNative();

                try
                {
                    target = new double[problem.Length];
                    var prob = (NativeMethods.problem*)problem.NativePtr;
                    NativeMethods.cross_validation(prob, &param, fold, target);
                }
                finally
                {
                    Free(&param);
                }
            }
        }

        /// <summary>
        /// Instead of conducting cross validation under a specified parameter C, conducts cross validation many times to find the best one with the highest cross validation accuracy.
        /// </summary>
        /// <param name="problem"><see cref="Problem"/>.</param>
        /// <param name="parameter"><see cref="Parameter"/>.</param>
        /// <param name="fold">The number of division for samples.</param>
        /// <param name="startC">The start parameter of C.</param>
        /// <param name="maxC">The max parameter of C.</param>
        /// <param name="bestC">Contains the best parameter of C.</param>
        /// <param name="bestRate">Contains the accuracy corresponding <paramref name="bestC"/>.</param>
        public static void FindParameterC(Problem problem, Parameter parameter, int fold, double startC, double maxC, out double bestC, out double bestRate)
        {
            bestC = 0;
            bestRate = 0;

            if (problem == null)
                throw new ArgumentNullException(nameof(problem));
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            problem.ThrowIfDisposed();

            unsafe
            {
                var param = parameter.ToNative();

                try
                {
                    var prob = (NativeMethods.problem*)problem.NativePtr;
                    NativeMethods.find_parameter_C(prob, &param, fold, startC, maxC, out bestC, out bestRate);
                }
                finally
                {
                    Free(&param);
                }
            }
        }

        /// <summary>
        /// Does classification or regression on a test vector x given a model.
        /// </summary>
        /// <param name="model"><see cref="Model"/>.</param>
        /// <param name="x">The test vector.</param>
        /// <returns>
        /// <para>For a classification model, the predicted class for x is returned.</para>
        /// <para>For a regression model, the function value of x calculated using the model is returned.</para>
        /// </returns>
        public static double Predict(Model model, FeatureNodeArray x)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (x == null)
                throw new ArgumentNullException(nameof(x));

            unsafe
            {
                var m = model.NativePtr;
                return NativeMethods.predict(m, (NativeMethods.feature_node*)x.NativePtr);
            }
        }

        /// <summary>
        /// Does classification or regression on a test vector x given a model.
        /// </summary>
        /// <param name="model"><see cref="Model"/>.</param>
        /// <param name="x">The test vector.</param>
        /// <param name="probability">Contains probability estimates.</param>
        /// <returns>The predicted class for x is returned.</returns>
        /// <remarks>Currently, we support only the probability outputs of logistic regression.</remarks>
        public static double Predict(Model model, FeatureNodeArray x, out double[] probability)
        {
            probability = null;

            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (x == null)
                throw new ArgumentNullException(nameof(x));

            unsafe
            {
                var m = model.NativePtr;
                probability = new double[model.Classes];
                return NativeMethods.predict_probability(m, (NativeMethods.feature_node*)x.NativePtr, probability);
            }
        }

        /// <summary>
        /// Returns the class with the highest decision value.
        /// </summary>
        /// <param name="model"><see cref="Model"/>.</param>
        /// <param name="x">The test vector.</param>
        /// <param name="decisionValues">Contains decision values.</param>
        /// <returns>The class with the highest decision value is returned.</returns>
        public static double PredictValues(Model model, FeatureNodeArray x, out double[] decisionValues)
        {
            decisionValues = null;

            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (x == null)
                throw new ArgumentNullException(nameof(x));

            unsafe
            {
                var m = model.NativePtr;
                decisionValues = new double[model.Classes];
                return NativeMethods.predict_values(m, (NativeMethods.feature_node*)x.NativePtr, decisionValues);
            }
        }

        /// <summary>
        /// Specify output fpr LIBLINEAR.
        /// </summary>
        /// <param name="printFunc">The callback to receive the output and process.</param>
        /// <remarks>If specify null, it suppress output from LIBLINEAR.</remarks>
        public static void SetPrintFunction(PrintFunc printFunc)
        {
            if (printFunc == null)
                printFunc = PrintNull;

            var fp = new PrintFunc(printFunc);
            var handle = GCHandle.Alloc(fp);
            var ip = Marshal.GetFunctionPointerForDelegate(fp);

            NativeMethods.set_print_string_function(ip);

            if (PrintFuncHandle.IsAllocated)
                PrintFuncHandle.Free();

            PrintFuncHandle = handle;
        }

        /// <summary>
        /// Does constructs and returns a linear classification or regression model according to the given training data and parameters.
        /// </summary>
        /// <param name="problem"><see cref="Problem"/>.</param>
        /// <param name="parameter"><see cref="Parameter"/>.</param>
        /// <returns>This method returns a new <see cref="Model"/>.</returns>
        public static Model Train(Problem problem, Parameter parameter)
        {
            if (problem == null)
                throw new ArgumentNullException(nameof(problem));
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            problem.ThrowIfDisposed();

            unsafe
            {
                var param = parameter.ToNative();

                try
                {
                    var ret = NativeMethods.train((NativeMethods.problem*)problem.NativePtr, &param);
                    if ((IntPtr)ret == IntPtr.Zero)
                        throw new LibLinearException("train returns null");

                    return new Model((IntPtr)ret);
                }
                finally
                {
                    Free(&param);
                }
            }
        }

        #region Helpers

        internal static unsafe void Free(NativeMethods.parameter* parameter)
        {
            if ((IntPtr)parameter->weight != IntPtr.Zero)
                NativeMethods.free((IntPtr)parameter->weight);

            if ((IntPtr)parameter->weight_label != IntPtr.Zero)
                NativeMethods.free((IntPtr)parameter->weight_label);

            parameter->weight = (double*)IntPtr.Zero;
            parameter->weight_label = (int*)IntPtr.Zero;
        }

        private static void PrintNull(string message)
        {

        }

        private static unsafe NativeMethods.parameter ToNative(this Parameter parameter)
        {
            var ret = new NativeMethods.parameter
            {
                solver_type = (int)parameter.SolverType,
                nr_weight = parameter.LengthOfWeight,
                C = parameter.C,
                eps = parameter.Epsilon,
                p = parameter.P,
            };

            if (ret.nr_weight > 0)
            {
                if (parameter.WeightLabel == null)
                    throw new LibLinearException("Parameter.WeightLabel is null although Parameter.LengthOfWeight is over 0.");
                if (parameter.Weight == null)
                    throw new LibLinearException("Parameter.Weight is null although Parameter.LengthOfWeight is over 0.");

                var len1 = parameter.WeightLabel.Length;
                var len2 = parameter.Weight.Length;
                if (len1 != ret.nr_weight || len2 != ret.nr_weight)
                    throw new LibLinearException("Parameter.WeightLabel.Length does not match Parameter.Weight.Length");
            }

            var failAlloc = false;

            try
            {
                if (ret.nr_weight > 0)
                {
                    ret.weight_label = (int*)NativeMethods.malloc(sizeof(int), ret.nr_weight);
                    fixed (int* p = &parameter.WeightLabel[0])
                        NativeMethods.memcpy(ret.weight_label, p, ret.nr_weight * sizeof(int));

                    ret.weight = (double*)NativeMethods.malloc(sizeof(double), ret.nr_weight);
                    fixed (double* p = &parameter.Weight[0])
                        NativeMethods.memcpy(ret.weight, p, ret.nr_weight * sizeof(double));
                }

                if (parameter.InitialSolution != null)
                {
                    var length = parameter.InitialSolution.Length;
                    ret.init_sol = (double*)NativeMethods.malloc(sizeof(double), length);
                    Marshal.Copy(parameter.InitialSolution, 0, (IntPtr)ret.init_sol, length);
                }

                failAlloc = true;
            }
            finally
            {
                if (!failAlloc)
                {
                    NativeMethods.free((IntPtr)ret.weight_label);
                    NativeMethods.free((IntPtr)ret.weight);
                    NativeMethods.free((IntPtr)ret.init_sol);
                }
            }

            return ret;
        }

        #endregion

        #endregion

    }

}

