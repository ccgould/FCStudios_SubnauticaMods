using System.IO;

namespace FCSModdingUtility
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public SettingsViewModel()
        {

        }
        #endregion

        #region public Properties

        /// <summary>
        /// An instance of the settings view model to be available project wide
        /// </summary>
        public static SettingsViewModel Instance { get; set; } = new SettingsViewModel();

        /// <summary>
        /// Location of the res directory
        /// </summary>
        public string ResDirectoryLocation { get; set; }

        public string WindowTitle { get; set; } = "FCS Modding Utility";

        /// <summary>
        /// Location of the project file
        /// </summary>
        public string ProjectSaveLocation { get; set; }

        public string RecentFiles { get; } = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "System",
            "Recent");

        /// <summary>
        /// The name of the ploaded project
        /// </summary>
        public string ProjectName { get; set; }

        #endregion
    }
}
