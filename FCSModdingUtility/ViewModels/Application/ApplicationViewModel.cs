using System;
using System.Windows.Media;
using FontAwesome.WPF;

namespace FCSModdingUtility
{
    /// <summary>
    /// The application state as a view model
    /// </summary>
    public class ApplicationViewModel : BaseViewModel
    {
        #region Private Members

        /// <summary>
        /// True if the settings menu should be shown
        /// </summary>
        private bool mSettingsMenuVisible;

        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public ApplicationViewModel()
        {
            StatusBarStatus = "Ready";
            StatusBarColor = new SolidColorBrush(Color.FromRgb(0, 122, 204));
            StatusBarIcon = FontAwesomeIcon.CheckSquare;
        }
        #endregion

        #region Public Properties

        /// <summary>
        /// The current page of the application
        /// </summary>

        public ApplicationPage CurrentPage { get; private set; } = ApplicationPage.StartPage;

        /// <summary>
        /// The view model to use for the current page when the CurrentPage changes
        /// NOTE: This is not a live up-to-date view model of the current page
        ///       it is simply used to set the view model of the current page 
        ///       at the time it changes
        /// </summary>
        public BaseViewModel CurrentPageViewModel { get; set; }

        /// <summary>
        /// True if the side menu should be shown
        /// </summary>
        public bool SideMenuVisible { get; set; } = false;

        /// <summary>
        /// True if the settings menu should be shown
        /// </summary>
        public bool SettingsMenuVisible
        {
            get => mSettingsMenuVisible;
            set
            {
                // If property has not changed...
                if (mSettingsMenuVisible == value)
                    // Ignore
                    return;

                // Set the backing field
                mSettingsMenuVisible = value;

                //TODO Fix

                //// If the settings menu is now visible...
                //if (value)
                //    // Reload settings
                //    TaskManager.RunAndForget(ViewModelSettings.LoadAsync);
            }
        }

        /// <summary>
        /// The status bar current status e.g (Ready)
        /// </summary>
        public string StatusBarStatus { get; set; }

        /// <summary>
        /// The status bar current message  
        /// </summary>
        public string StatusBarMessage { get; set; }

        /// <summary>
        /// The status bar color
        /// </summary>
        public Brush StatusBarColor { get; set; }

        /// <summary>
        /// Icon for the status bar
        /// </summary>
        public FontAwesomeIcon StatusBarIcon { get; set; }

        /// <summary>
        /// Activate the spin animation for the icon
        /// </summary>
        public bool SpinAnimation { get; set; }

        /// <summary>
        /// Boolean to state is to save is enabled/disabled
        /// </summary>
        public bool AutoSaveEnabled { get; set; } = true;

        /// <summary>
        /// The amount of time between each auto-save
        /// </summary>
        public int AutoSaveAmount { get; set; } = 5;

        #endregion

        #region Public Helper Methods
        /// <summary>
        /// Navigates to the specified page
        /// </summary>
        /// <param name="page">The page to go to</param>
        /// <param name="viewModel">The view model, if any, to set explicitly to the new page</param>
        public void GoToPage(ApplicationPage page, BaseViewModel viewModel = null)
        {
            // Always hide settings page if we are changing pages
            SettingsMenuVisible = false;

            // Set the view model
            CurrentPageViewModel = viewModel;

            // See if page has changed
            var different = CurrentPage != page;

            // Set the current page
            CurrentPage = page;

            // If the page hasn't changed, fire off notification
            // So pages still update if just the view model has changed
            if (!different)

                OnPropertyChanged(nameof(CurrentPage));
        }

        /// <summary>
        /// Sets the status for the status bar at the bottom of the application
        /// </summary>
        /// <param name="status">The status of the bar using see<see cref="ApplicationStatusTypes"/></param>
        /// <param name="message">The message to show in the status bar</param>
        /// <param name="barColor">The see <see cref="SolidColorBrush"/> color to use as the background.</param>
        /// <param name="spinAnimation">Boolean to make the icon spin indefinitely </param>
        public void SetStatus(ApplicationStatusTypes status, string message, StatusBarColors barColor = StatusBarColors.Default, bool spinAnimation = false)
        {
            StatusBarStatus = status.ToString();
            StatusBarMessage = message;
            SpinAnimation = spinAnimation;


            switch (barColor)
            {
                case StatusBarColors.Default:
                    StatusBarColor = GConv.RGB2SolidColorBrush(0, 122, 204);
                    break;
                case StatusBarColors.Warning:
                    StatusBarColor = GConv.RGB2SolidColorBrush(216, 66, 0);
                    break;
                case StatusBarColors.Error:
                    StatusBarColor = GConv.RGB2SolidColorBrush(178, 34, 34);
                    break;
                case StatusBarColors.Unknown:
                    StatusBarColor = GConv.RGB2SolidColorBrush(186, 85, 211);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(barColor), barColor, null);
            }


            switch (status)
            {
                case ApplicationStatusTypes.Ready:
                    StatusBarIcon = FontAwesomeIcon.CheckSquare;
                    break;
                case ApplicationStatusTypes.Saving:
                    StatusBarIcon = FontAwesomeIcon.Spinner;
                    break;
                case ApplicationStatusTypes.Loading:
                    StatusBarIcon = FontAwesomeIcon.Spinner;
                    SpinAnimation = true;
                    break;
                case ApplicationStatusTypes.Error:
                    StatusBarIcon = FontAwesomeIcon.Close;
                    break;
                case ApplicationStatusTypes.Running:
                    StatusBarIcon = FontAwesomeIcon.Spinner;
                    SpinAnimation = true;
                    break;
                case ApplicationStatusTypes.Warning:
                    StatusBarIcon = FontAwesomeIcon.ExclamationTriangle;
                    break;
                default:
                    StatusBarIcon = FontAwesomeIcon.CheckSquare;
                    break;
            }
        }

        public void UpdateMessage(string message)
        {
            StatusBarMessage = message;
        }

        /// <summary>
        /// Sets status to the default Done
        /// </summary>
        public void DoneStatus()
        {
            SetStatus(ApplicationStatusTypes.Ready, "Done");
        }

        /// <summary>
        /// Sets status to the default Done
        /// </summary>
        public void DefaultStatus()
        {
            SetStatus(ApplicationStatusTypes.Ready, "");
        }

        public void DefaultError(string message)
        {
            SetStatus(ApplicationStatusTypes.Error, message, StatusBarColors.Error);
        }
        #endregion

        public void DefaultWarning(string message)
        {
            SetStatus(ApplicationStatusTypes.Warning, message, StatusBarColors.Warning);
        }
    }
}
