namespace FCSAlterraIndustrialSolutions.Models.Abstract
{
    public abstract class ModStringsOmit
    {
        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public ModStringsOmit()
        {
            LoadDefault();
        }
        #endregion
        /// <summary>
        /// The description of the mod.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Region of the object
        /// </summary>
        public string Region { get; set; }

        public abstract void LoadDefault();
    }
}
