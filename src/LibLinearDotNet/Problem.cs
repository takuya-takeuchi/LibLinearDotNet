using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LibLinearDotNet
{

    /// <summary>
    /// Represents an problem.
    /// </summary>
    public sealed class Problem : DisposableObject
    {

        #region Properties

        private readonly IntPtr _XSpacePtr;

        #endregion

        #region Constructors

        internal unsafe Problem(FeatureNode[][] x, double[] y, double bias)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));

            // Get array of each length of featureNode array
            var lengthArray = x.Select(nodes => nodes.Length).ToArray();

            // Create feature_node**
            var count = x.Length;
            var totalElementNum = lengthArray.Sum();

            // reserve bias area
            if (bias >= 0)
                totalElementNum += count;

            // reserve delimiter area
            totalElementNum += count;

            var maxIndex = 0;
            var pIndex = 0;

            var failAlloc = false;
            var tempPx = (NativeMethods.feature_node**)0;
            var tempXSpace = (NativeMethods.feature_node*)0;
            var tmpProb = (NativeMethods.problem*)0;

            try
            {
                // Alloc memery area continuously
                tempPx = (NativeMethods.feature_node**)NativeMethods.malloc(sizeof(NativeMethods.feature_node*), count);
                tempXSpace = (NativeMethods.feature_node*)NativeMethods.malloc(sizeof(NativeMethods.feature_node), totalElementNum);
                this._XSpacePtr = (IntPtr)tempXSpace;

                for (var index = 0; index < x.Length; index++)
                {
                    var array = x[index];
                    var max = 0;

                    tempPx[index] = &tempXSpace[pIndex];

                    var len = array.Length;
                    fixed (FeatureNode* pX = &array[0])
                    {
                        for (var j = 0; j < len; j++, pIndex++)
                        {
                            tempXSpace[pIndex].index = pX[j].Index;
                            tempXSpace[pIndex].value = pX[j].Value;

                            // Basically, last index should be max index
                            max = Math.Max(pX[j].Index, max);
                        }

                        if (max > maxIndex)
                            maxIndex = max;

                        if (bias >= 0)
                            tempXSpace[pIndex++].value = bias;

                        // delimiter
                        tempXSpace[pIndex++].index = -1;
                    }

                    maxIndex = Math.Max(maxIndex, max);
                }

                // Create problem instance
                tmpProb = (NativeMethods.problem*)NativeMethods.malloc(sizeof(NativeMethods.problem), 1);
                tmpProb->l = y.Length;
                tmpProb->x = tempPx;
                tmpProb->bias = bias;

                if (bias >= 0)
                {
                    tmpProb->n = maxIndex + 1;
                    for (int i = 1, l = tmpProb->l; i < l; i++)
                        (tmpProb->x[i] - 2)->index = tmpProb->n;

                    // delimiter + bias area
                    var last = x[count - 1].Length + 2;
                    tmpProb->x[count - 1][last - 2].index = tmpProb->n;
                }
                else
                {
                    tmpProb->n = maxIndex;
                }

                tmpProb->y = (double*)NativeMethods.malloc(sizeof(double), tmpProb->l);
                Marshal.Copy(y, 0, (IntPtr)tmpProb->y, tmpProb->l);

                this.NativePtr = (IntPtr)tmpProb;
                this.Number = tmpProb->n;
                this.Bias = bias;
                this._Y = y;
                this._X = new FeatureNodeArrayCollecion(tempPx, lengthArray);

                failAlloc = true;
            }
            finally
            {
                if (!failAlloc)
                {
                    NativeMethods.free((IntPtr)tmpProb->y);
                    NativeMethods.free((IntPtr)tmpProb);
                    NativeMethods.free((IntPtr)tempXSpace);
                    NativeMethods.free((IntPtr)tempPx);
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the bias.
        /// </summary>
        public double Bias
        {
            get;
        }

        /// <summary>
        /// Gets the number of data contained in the <see cref="Problem"/>.
        /// </summary>
        /// <returns>The number of data contained in the <see cref="Problem"/>.</returns>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        public int Length
        {
            get
            {
                this.ThrowIfDisposed();
                return this._Y.Length;
            }
        }

        /// <summary>
        /// Gets the number of feature (including the bias feature if <code>Bias >= 0</code>).
        /// </summary>
        /// <returns>The number of feature (including the bias feature if bias >= 0) in the <see cref="Problem"/>.</returns>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        public int Number
        {
            get;
        }

        private readonly FeatureNodeArrayCollecion _X;

        /// <summary>
        /// Gets the <see cref="FeatureNodeArrayCollecion"/>.
        /// </summary>
        /// <returns>An <see cref="FeatureNodeArrayCollecion"/> instance that problem owns.</returns>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        public FeatureNodeArrayCollecion X
        {
            get
            {
                this.ThrowIfDisposed();
                return this._X;
            }
        }

        private readonly double[] _Y;

        /// <summary>
        /// Gets an array containing the target values.
        /// </summary>
        /// <returns>An array containing the target values. Integers in classification. Real numbers in regression.</returns>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        public double[] Y
        {
            get
            {
                this.ThrowIfDisposed();
                return this._Y;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new <see cref="Problem"/> from the specified file.
        /// </summary>
        /// <param name="path">The LIBLINEAR format file name and path.</param>
        /// <param name="bias">The bias.</param>
        /// <returns>This method returns a new <see cref="Problem"/> for the specified file.</returns>
        /// <exception cref="ArgumentException">The specified path is null or whitespace.</exception>
        /// <exception cref="FileNotFoundException">The specified file is not found.</exception>
        /// <exception cref="FormatException">The specified file is invalid format.</exception>
        public static Problem FromFile(string path, double bias)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("The specified path is null or whitespace.");

            if (!File.Exists(path))
                throw new FileNotFoundException("The specified file is not found.");

            var x = new List<FeatureNode[]>();
            var y = new List<double>();
            var lines = File.ReadAllLines(path);

            try
            {
                foreach (var tokens in lines.Select(line => line.Split(" \t\n\r\f".ToCharArray())
                                            .Where(c => c != string.Empty).ToArray()))
                {
                    y.Add(double.Parse(tokens[0]));

                    var nodes = new List<FeatureNode>();
                    for (var i = 1; i <= tokens.Length - 1; i++)
                    {
                        var token = tokens[i].Trim().Split(':');
                        nodes.Add(new FeatureNode
                        {
                            Index = int.Parse(token[0]),
                            Value = double.Parse(token[1])
                        });
                    }

                    x.Add(nodes.ToArray());
                }

                return new Problem(x.ToArray(), y.ToArray(), bias);

            }
            catch (FormatException fe)
            {
                throw new FormatException("The specified file is invalid format.", fe);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Problem"/> from the specified nodes and labels.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="labels">The labels.</param>
        /// <param name="bias">The bias.</param>
        /// <returns>This method returns a new <see cref="Problem"/> for the specified nodes and labels.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="nodes"/> is null or <paramref name="labels"/> is null.</exception>
        /// <exception cref="ArgumentException">Number of nodes and numbr of labels are not the same.</exception>
        public static Problem FromSequence(IList<FeatureNode[]> nodes, IEnumerable<double> labels, double bias)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            if (nodes.Count != labels.Count())
                throw new ArgumentException("Number of nodes and numbr of labels are not the same.");

            return new Problem(nodes.ToArray(), labels.ToArray(), bias);
        }

        #region Overrides

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            unsafe
            {
                var problem = (NativeMethods.problem*)this.NativePtr;
                NativeMethods.free((IntPtr)problem->x);
                NativeMethods.free((IntPtr)problem->y);
                NativeMethods.free(this._XSpacePtr);
            }
        }

        #endregion

        #endregion

    }

}