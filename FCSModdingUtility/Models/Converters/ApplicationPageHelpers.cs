using System.Diagnostics;

namespace FCSModdingUtility
{
    /// <summary>
    /// Converts the <see cref="ApplicationPage"/> to an actual view/page
    /// </summary>
    public static class ApplicationPageHelpers
    {
        /// <summary>
        /// Takes a <see cref="ApplicationPage"/> and a view model, if any, and creates the desired page
        /// </summary>
        /// <param name="page"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static BasePage ToBasePage(this ApplicationPage page, object viewModel = null)
        {
            // Find the appropriate page
            switch (page)
            {
                case ApplicationPage.StartPage:
                    return new Home(viewModel as HomeViewModel);
                //case ApplicationPage.NewProject:
                //    return new NewProjectPage(viewModel as NewProjectViewModel);
                case ApplicationPage.EditorPage:
                    return new EditorPage(viewModel as EditorPageViewModel);
                default:
                    Debugger.Break();
                    return null;
            }
        }

        /// <summary>
        /// Converts a <see cref="BasePage"/> to the specific <see cref="ApplicationPage"/> that is for that type of page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static ApplicationPage ToApplicationPage(this BasePage page)
        {
            // Find application page that matches the base page
            if (page is Home)
                return ApplicationPage.StartPage;
            //if (page is NewProjectPage)
            //    return ApplicationPage.NewProject;
            if (page is EditorPage)
                return ApplicationPage.EditorPage;
            // Alert developer of issue
            Debugger.Break();
            return default(ApplicationPage);
        }
    }
}
