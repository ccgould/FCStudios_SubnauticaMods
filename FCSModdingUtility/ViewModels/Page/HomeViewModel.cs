using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using static FCSModdingUtility.DI;


namespace FCSModdingUtility
{
    public class HomeViewModel : BaseViewModel
    {

        
        public ModItemViewModel SelectedItem { get; set; }

        public ObservableCollection<ModItemViewModel> SelectedResItems { get; set; } = new ObservableCollection<ModItemViewModel>();
        public System.Collections.IList SelectedItems
        {
            get { return SelectedResItems; }
            set
            {
                SelectedResItems.Clear();
                foreach (ModItemViewModel model in value)
                {
                    SelectedResItems.Add(model);
                }
            }
        }

        public ICommand RefreshBTNCommand { get; set; }

        public HomeViewModel()
        {
           RefreshBTNCommand = new RelayCommand(RefreshBTNCommandMethod);
        }

        private void RefreshBTNCommandMethod()
        {
            DataStorage.Load();
        }
    }
}
