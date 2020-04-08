using System;


namespace FCSCommon.Exceptions
{
    /// <summary>
    /// A custom exception for errors in patching
    /// </summary>
    internal class PatchTerminatedException : Exception
    {
        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public PatchTerminatedException()
        {

        }
        #endregion

        internal PatchTerminatedException(string message)
            : base(message)
        {
        }

        internal PatchTerminatedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
