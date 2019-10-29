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
        public ObservableCollection<ModItemViewModel> NewItemModels { get; set; } = new ObservableCollection<ModItemViewModel>();

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
            //Random rnd = new Random();

            //for (int i = 0; i < 5; i++)
            //{
            //    int patch = rnd.Next(0, 100);
            //    NewItemModels.Add(new ModItemViewModel{DisplayName = $"Mod Name {i+1}", Version = $"1.0.{patch}"});
            //}

            RefreshBTNCommand = new RelayCommand(RefreshBTNCommandMethod);

            Load();
        }

        private void RefreshBTNCommandMethod()
        {
            Load();
        }

        private async void Load()
        {
            NewItemModels.Clear();
            Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
            progress.ProgressChanged += OnModLoadProgressChanged;
            await GetAllModsAsync(progress);
        }

        private void OnModLoadProgressChanged(object sender, ProgressReportModel e)
        {
            var progress = e.PercentageComplete;
            ViewModelApplication.UpdateMessage($"Progress: {progress}%");
        }

        private async Task GetAllModsAsync(IProgress<ProgressReportModel> progress)
        {
            try
            {
                var path = @"F:\Program Files\Epic Games\Subnautica\QMods";

                string[] filePaths = Directory.GetFiles(path, "*.json",
                    SearchOption.AllDirectories);

                if (filePaths.Length <= 0) return;

                ProgressReportModel report = new ProgressReportModel();

                var modConfigs =
                    filePaths.Where(x => Path.GetFileName(x).StartsWith("mod", StringComparison.OrdinalIgnoreCase)).ToList();

                if(!modConfigs.Any()) return;

                foreach (string modConfig in modConfigs)
                {
                    var item = await GetDataAsync(modConfig);
                    item.Location = modConfig;
                    NewItemModels.Add(item);
                    report.ModsLoaded = NewItemModels;
                    report.PercentageComplete = (NewItemModels.Count * 100) / modConfig.Length;
                    progress.Report(report);
                }
            }
            catch (Exception e)
            {

            }
        }

        private async Task<ModItemViewModel> GetDataAsync(string modConfig)
        {
            var config = File.ReadAllText(modConfig);
            return await Task.Run(() => JsonConvert.DeserializeObject<ModItemViewModel>(config));
        }
    }
}
