using System.Windows;
using System.Windows.Controls;

namespace FCSModdingUtility
{
    /// <summary>
    /// The View Model for the custom flat window
    /// </summary>
    public class DialogWindowViewModel : MainWindowViewModel
    {
        #region public Properties

        /// <summary>
        /// The title of this dialog window
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The content to host inside the dialog
        /// </summary>
        public Control Content { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DialogWindowViewModel(Window window) : base(window)
        {
            // Make minimum size smaller
            WindowMinimumWidth = 250;
            WindowMinimumHeight = 100;

            // Make title bar smaller
            TitleHeight = 30;
        }

        #endregion
    }
}
