using System;

namespace LibLinearDotNet
{

    /// <summary>
    /// The exception is general exception for LIBLINEAR. This class cannot be inherited.
    /// </summary>
    public sealed class LibLinearException : Exception
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LibLinearException"/> class.
        /// </summary>
        public LibLinearException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibLinearException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public LibLinearException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibLinearException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The name of the parameter that caused the current exception.</param>
        public LibLinearException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion

    }

}
