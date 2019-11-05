using System;
using System.Windows;
using System.Windows.Threading;
using FCSModdingUtility;

namespace FCSModdingUtility
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            DataContext = new SplashScreenViewModel(this);
        }
    }
}


