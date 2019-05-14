namespace FCSCyclopsDock.Models.Abstract
{
    public abstract class ModStrings
    {
        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public ModStrings()
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
