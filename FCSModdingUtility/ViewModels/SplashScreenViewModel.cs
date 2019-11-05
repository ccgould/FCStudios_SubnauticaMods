using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace FCSModdingUtility
{
    public class SplashScreenViewModel : BaseViewModel
    {
        DispatcherTimer dt = new DispatcherTimer();
        private Window _splash;
        public bool? DialogResult { get; set; }
        public string SplashScreenText { get; set; } = "Initializing...";
        
        public SplashScreenViewModel(Window splashScreen)
        {
            DataStorage.Load();
            dt.Tick += DtOnTick;
            dt.Interval = new TimeSpan(0, 0, 2);
            dt.Start();
            _splash = splashScreen;
        }
        private void DtOnTick(object sender, EventArgs e)
        {
            MainWindow mw = new MainWindow();

            mw.Show();
            dt.Stop();
            _splash.Close();
        }
    }
}
