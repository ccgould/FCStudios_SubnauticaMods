using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FCSModdingUtility
{
    public static class DataStorage
    {
        public static ObservableCollection<ModItemViewModel> NewItemModels { get; set; } = new ObservableCollection<ModItemViewModel>();

        private static int _modConfigsCount;

        public static void Load()
        {
            DataStorage.NewItemModels.Clear();
            Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
            progress.ProgressChanged += OnModLoadProgressChanged;
            GetAllModsAsync(progress);
        }

        private static void OnModLoadProgressChanged(object sender, ProgressReportModel e)
        {
            var progress = e.PercentageComplete;
            //ViewModelApplication.UpdateMessage($"Progress: {progress}%");
        }

        private static void GetAllModsAsync(IProgress<ProgressReportModel> progress)
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

                if (!modConfigs.Any()) return;

                _modConfigsCount = modConfigs.Count;

                foreach (string modConfig in modConfigs)
                {
                    var config = File.ReadAllText(modConfig);
                    var item = JsonConvert.DeserializeObject<ModItemViewModel>(config);
                    item.Location = modConfig;
                    item.CheckForIssues();
                    DataStorage.NewItemModels.Add(item);
                    report.ModsLoaded = DataStorage.NewItemModels;
                    report.PercentageComplete = (DataStorage.NewItemModels.Count * 100) / modConfig.Length;
                    progress.Report(report);
                }
            }
            catch (Exception e)
            {
                //ViewModelApplication.DefaultError(e.Message);
            }
        }

    }
}
