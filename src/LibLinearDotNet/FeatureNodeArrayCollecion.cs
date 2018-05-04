namespace LibLinearDotNet
{

    /// <summary>
    /// Represents an collection of <see cref="FeatureNodeArray"/>.
    /// </summary>
    public sealed class FeatureNodeArrayCollecion
    {

        #region Fields

        private readonly int[] _LengthArray;

        private readonly unsafe NativeMethods.feature_node** _NativePtr;

        #endregion

        #region Constructors

        internal unsafe FeatureNodeArrayCollecion(NativeMethods.feature_node** ptr, int[] lengthArray)
        {
            this._NativePtr = ptr;
            this._LengthArray = lengthArray;
            this.Count = lengthArray.Length;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="FeatureNodeArrayCollecion"/>.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="FeatureNodeArrayCollecion"/>.</returns>
        public int Count
        {
            get;
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        public FeatureNodeArray this[int index]
        {
            get
            {
                unsafe
                {
                    return new FeatureNodeArray(this._NativePtr[index], this._LengthArray[index]);
                }
            }
        }

        #endregion

    }

}