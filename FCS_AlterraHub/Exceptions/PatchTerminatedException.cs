using System;

namespace FCS_AlterraHub.Exceptions
{
    /// <summary>
    /// A custom exception for errors in patching
    /// </summary>
    public class PatchTerminatedException : Exception
    {
        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public PatchTerminatedException()
        {

        }
        #endregion

        public PatchTerminatedException(string message)
            : base(message)
        {
        }

        public PatchTerminatedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
