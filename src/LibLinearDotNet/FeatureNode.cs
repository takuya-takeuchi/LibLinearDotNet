using System;

namespace LibLinearDotNet
{

    /// <summary>
    /// Represents an element for vector data.
    /// </summary>
    public struct FeatureNode
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureNode"/> struct with the specified featureNode data and index of featureNode data.
        /// </summary>
        /// <param name="ptr">Pointer to an array of <see cref="NativeMethods.feature_node"/>.</param>
        /// <param name="index">The zero-based index at which featureNode is located.</param>
        internal unsafe FeatureNode(IntPtr ptr, int index)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException($"{nameof(ptr)} should not be IntPtr.Zero");

            var p = (NativeMethods.feature_node*)ptr;
            var node = p[index];
            this.Index = node.index;
            this.Value = node.value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureNode"/> struct with the specified featureNode data.
        /// </summary>
        /// <param name="ptr">Pointer to an array of <see cref="NativeMethods.feature_node"/>.</param>
        internal unsafe FeatureNode(NativeMethods.feature_node* ptr)
        {
            if ((IntPtr)ptr == IntPtr.Zero)
                throw new ArgumentException($"{nameof(ptr)} should not be IntPtr.Zero");

            this.Index = ptr->index;
            this.Value = ptr->value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating the position of the element in vector.
        /// </summary>
        /// <returns>The one-based index representing the position of the element in vector.</returns>
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value of the element in vector.
        /// </summary>
        /// <returns>The value of the element in vector.</returns>
        public double Value
        {
            get;
            set;
        }

        #endregion

    }

}