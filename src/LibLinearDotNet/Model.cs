using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LibLinearDotNet
{

    /// <summary>
    /// Represents an trained model.
    /// </summary>
    public sealed class Model : DisposableObject
    {

        #region Constructors

        internal unsafe Model(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException($"{nameof(ptr)} should not be IntPtr.Zero");

            // NOTE
            // Detail of each member size: https://github.com/cjlin1/liblinear/blob/master/README
            this.NativePtr = ptr;
            var model = (NativeMethods.model*)ptr;

            this.Parameter = new Parameter(&model->param);
            this.Classes = model->nr_class;
            this.Features = model->nr_featurel;

            var length = (model->nr_featurel + 1) * model->nr_class;
            if (length > 0)
                this.Weights = Copy(model->w, length);

            if (model->label != (int*)0)
                this.Label = Copy(model->label, this.Classes);

            this.IsEstimableProbability = NativeMethods.check_probability_model(model) == NativeMethods.True;
            this.IsRegressionModel = NativeMethods.check_regression_model(model) == NativeMethods.True;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of classes.
        /// </summary>
        public int Classes
        {
            get;
        }

        /// <summary>
        /// Gets the number of features.
        /// </summary>
        public int Features
        {
            get;
        }

        /// <summary>
        /// Indicates whether this model contains required information to do probability estimates.
        /// </summary>
        /// <returns>true if this model contains required information to do probability estimates; otherwise, false.</returns>
        public bool IsEstimableProbability
        {
            get;
        }

        /// <summary>
        /// Indicates whether this model is a suport vector regression model.
        /// </summary>
        /// <returns>true if this model is a suport vector regression model; otherwise, false.</returns>
        public bool IsRegressionModel
        {
            get;
        }

        /// <summary>
        /// Gets the array for label of each class.
        /// </summary>
        public int[] Label
        {
            get;
        }

        /// <summary>
        /// Gets the parameter of this model.
        /// </summary>
        public Parameter Parameter
        {
            get;
        }

        /// <summary>
        /// Gets the feature weights.
        /// </summary>
        public double[] Weights
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the bias term corresponding to the class with specified label index.
        /// </summary>
        /// <param name="labelIndex">The label index.</param>
        /// <returns>
        /// <para>The bias term corresponding to the class with <paramref name="labelIndex"/>.</para>
        /// <para>For classification models, if <paramref name="labelIndex"/> is not in the valid range(0 to <see cref="Classes"/> - 1), then a zero value will be returned.</para>
        /// <para>For regression models, <paramref name="labelIndex"/> is ignored.</para>
        /// </returns>
        public double GetBiasOfDecisionFunction(int labelIndex)
        {
            this.ThrowIfDisposed();

            return NativeMethods.get_decfun_bias(this.NativePtr, labelIndex);
        }

        /// <summary>
        /// Returns the coefficient for the feature with the specified feature index and the class with the specified label index.
        /// </summary>
        /// <param name="featureIndex">The feature index.</param>
        /// <param name="labelIndex">The label index.</param>
        /// <returns>
        /// <para>The coefficient for the feature with <paramref name="featureIndex"/> and the class with <paramref name="labelIndex"/>.</para>
        /// <para>For classification models, if <paramref name="labelIndex"/> is not in a valid range (0 to <see cref="Classes"/> - 1), then a zero value will be returned.</para>
        /// <para>Ror regression models, <paramref name="labelIndex"/> is ignored.</para>
        /// </returns>
        public double GetCoefficientOfDecisionFunction(int featureIndex, int labelIndex)
        {
            this.ThrowIfDisposed();

            return NativeMethods.get_decfun_coef(this.NativePtr, featureIndex, labelIndex);
        }

        /// <summary>
        /// Loads an <see cref="Model"/> given the specified file.
        /// </summary>
        /// <param name="path">The LIBLINEAR format file name and path.</param>
        /// <returns>This method returns a new <see cref="Model"/> for the specified file.</returns>
        /// <exception cref="ArgumentException">The specified path is null or whitespace.</exception>
        /// <exception cref="FileNotFoundException">The specified file is not found.</exception>
        /// <exception cref="FormatException">The specified file is invalid format.</exception>
        public static Model Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("The specified path is null or whitespace.");

            if (!File.Exists(path))
                throw new FileNotFoundException("The specified file is not found.");

            unsafe
            {
                var ret = NativeMethods.load_model(path);
                return new Model((IntPtr)ret);
            }
        }

        /// <summary>
        /// Saves this <see cref="Model"/> to the specified file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="model">The model to write to the file.</param>
        /// <exception cref="LibLinearException">Failed to save model to the specified file.</exception>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        public static void Save(string path, Model model)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("The specified path is null or whitespace.");

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.ThrowIfDisposed();

            unsafe
            {
                if (NativeMethods.save_model(path, (NativeMethods.model*)model.NativePtr) != NativeMethods.OK)
                    throw new LibLinearException();
            }
        }

        #region Overrides

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            NativeMethods.free_model_content(this.NativePtr);
            NativeMethods.free(this.NativePtr);
        }

        #endregion

        #region Helpers

        private static unsafe double[] Copy(double* src, int len)
        {
            var array = new double[len];
            Marshal.Copy((IntPtr)src, array, 0, len);
            return array;
        }

        private static unsafe int[] Copy(int* src, int len)
        {
            var array = new int[len];
            Marshal.Copy((IntPtr)src, array, 0, len);
            return array;
        }

        #endregion

        #endregion

    }
}
