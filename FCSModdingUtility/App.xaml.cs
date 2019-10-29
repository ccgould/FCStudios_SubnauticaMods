using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dna;
using static FCSModdingUtility.DI;

namespace FCSModdingUtility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup the main application 
            await ApplicationSetupAsync();

            //Logger.LogDebugSource("Application starting...");
            //var mess = new MessageBoxDialogViewModel { Message = "Dd", Title = "ffd", OkText = "OK" };
            //await UI.ShowMessage(mess);

            ViewModelApplication.GoToPage(ApplicationPage.StartPage);
        }
        
        /// <summary>
        /// Configures our application ready for use
        /// </summary>
        private async Task ApplicationSetupAsync()
        {
            //TODO load settings
            // Setup the DNA Framework
            Framework.Construct<DefaultFrameworkConstruction>()
                .AddFileLogger()
                .AddViewModels()
                .AddClientServices()
                .Build();
        }
    }
}
