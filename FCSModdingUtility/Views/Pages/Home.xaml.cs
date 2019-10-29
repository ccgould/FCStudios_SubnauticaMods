using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FCSModdingUtility
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : BasePage<HomeViewModel>
    {
        public Home()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with specific view model
        /// </summary>
        public Home(HomeViewModel specificViewModel) : base(specificViewModel)
        {
            InitializeComponent();
        }
    }
}
