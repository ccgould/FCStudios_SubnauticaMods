using System;

namespace FCSSubnauticaCore.Exceptions
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
