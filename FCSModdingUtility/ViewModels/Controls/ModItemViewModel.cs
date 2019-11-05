using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FCSModdingUtility.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using static FCSModdingUtility.DI;

namespace FCSModdingUtility
{
    public partial class ModItemViewModel : BaseViewModel
    {
        #region Private Members
        private string _id;
        private List<string> _dependancies;
        private Dictionary<string, string> _versionDependencies;
        private string _modFolder;
        private string _fileStructureSave;
        private string _iconPath;

        #endregion

        #region Public JSON Properties

        [DisplayName("ID")]
        [Category("Mod Properties")]
        [Description(DialogStrings.IDDesc)]
        [JsonProperty("Id")]
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                IconImageValue = GetIconLocation();
            }
        }

        [DisplayName("Display Name")]
        [Category("Mod Properties")]
        [Description(DialogStrings.DisplayName)]
        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }


        [DisplayName("Author")]
        [Category("Mod Properties")]
        [Description(DialogStrings.Author)]
        [JsonProperty("Author")]
        public string Author { get; set; }

        [DisplayName("Version")]
        [Category("Mod Properties")]
        [Description(DialogStrings.Version)]
        [JsonProperty("Version")]
        public string Version { get; set; }

        [DisplayName("Game")]
        [Category("Mod Properties")]
        [Description(DialogStrings.Game)]
        [JsonProperty("Game")]
        public string Game { get; set; }

        [DisplayName("Dependencies")]
        [Category("Mod Properties")]
        [Description(DialogStrings.Dependencies)]
        [JsonProperty("Dependencies")]
        public List<string> Dependencies
        {
            get => _dependancies;
            set
            {
                _dependancies = value;
                DependenciesCount = value.Count;
                SetDependentOnTechFab();
            }
        }

        [DisplayName("Version Dependencies")]
        [Category("Mod Properties")]
        [Description(DialogStrings.VersionDependencies)]
        [JsonProperty("VersionDependencies")]
        public Dictionary<string, string> VersionDependencies
        {
            get => _versionDependencies;
            set
            {
                _versionDependencies = value;
                VersionDependenciesCount = value.Count;
                SetDependentOnTechFab();
            }
        }

        [DisplayName("Load Before")]
        [Category("Mod Properties")]
        [Description(DialogStrings.LoadBefore)]
        [JsonProperty("LoadBefore")]
        public List<string> LoadBefore { get; set; }

        [DisplayName("Load After")]
        [Category("Mod Properties")]
        [Description(DialogStrings.LoadAfter)]
        [JsonProperty("LoadAfter")]
        public List<string> LoadAfter { get; set; }

        [DisplayName("Assembly Name")]
        [Category("Mod Properties")]
        [Description(DialogStrings.AssemblyName)]
        [JsonProperty("AssemblyName")]
        public string AssemblyName { get; set; }

        [DisplayName("Entry Method")]
        [Category("Mod Properties")]
        [Description(DialogStrings.EntryMethod)]
        [JsonProperty("EntryMethod")]
        public string EntryMethod { get; set; }

        [DisplayName("Enable")]
        [Category("Mod Properties")]
        [Description(DialogStrings.Enable)]
        [JsonProperty("Enable")]
        public bool Enable { get; set; }
        #endregion

        #region Public JSON Ignored Properties

        [JsonIgnore]
        public string ModSize { get; set; }

        [JsonIgnore]
        public object IconImageValue { get; set; }

        [JsonIgnore]
        public object PhotoImageValue { get; set; }

        [JsonIgnore]
        public object WarningIconValue { get; set; }

        [JsonIgnore]
        public string Location { get; set; }

        [JsonIgnore]
        public List<IssueReport> IssueReports { get; set; } = new List<IssueReport>();

        [JsonIgnore]
        public string Parent { get; set; }

        [JsonIgnore]
        public bool TechFabDependancy { get; set; }

        [JsonIgnore]
        public string DLLVersion { get; set; }

        [JsonIgnore]
        public int VersionDependenciesCount { get; set; }

        [JsonIgnore]
        public int DependenciesCount { get; set; }

        [JsonIgnore]
        public bool HasIssues { get; set; }

        [JsonIgnore]
        public string IssueToolTip => GetToolTipIssues();

        #endregion

        #region Private Methods

        private string GetToolTipIssues()
        {
            var sb = new StringBuilder();

            foreach (IssueReport report in IssueReports)
            {
                sb.Append($"{report.ToStringFull()}");
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private void UpdateData()
        {
            _modFolder = Path.GetDirectoryName(Location);
            _fileStructureSave = Path.Combine(_modFolder, "FCSModdingUtilitySave.fcs");
            ModSize = GetModSize();
            DLLVersion = GetVersionNumber();
            Parent = Path.GetDirectoryName(Location);
            IconImageValue = GetIconLocation();
            WarningIconValue = GetWarningIconLocation();
            PhotoImageValue = GetPhotoIconLocation();
        }

        private BitmapImage doGetImageSourceFromResource(string psAssemblyName, string psResourceName)
        {
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri($"pack://application:,,,/{psAssemblyName};component/Resources/{psResourceName}");
            logo.EndInit();
            return logo;
        }
        private object GetIconLocation()
        {
            var unknown = doGetImageSourceFromResource(Assembly.GetExecutingAssembly().GetName().Name, "unknown.png");

            if (Location == null || Id == null) return unknown;
            
            var image = Path.Combine(_modFolder, "Assets", $"{Id}.png");

            if (!File.Exists(image)) return unknown;

            _iconPath = image;

            return CacheImage(image);
        }

        private BitmapImage CacheImage(string path)
        {
            if (path != null)
            {
                //create new stream and create bitmap frame
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
                //bitmapImage.DecodePixelWidth = (int)_decodePixelWidth;
                //bitmapImage.DecodePixelHeight = (int)_decodePixelHeight;
                //load the image now so we can immediately dispose of the stream
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                //clean up the stream to avoid file access exceptions when attempting to delete images
                bitmapImage.StreamSource.Dispose();

                return bitmapImage;
            }

            return null;
        }


        private object GetWarningIconLocation()
        {
            return new BitmapImage(new Uri($"pack://application:,,,/Images/System/problem.png")); ;
        }

        private object GetPhotoIconLocation()
        {
            return new BitmapImage(new Uri($"pack://application:,,,/Images/System/edit.png")); ;
        }

        public void CheckForIssues()
        {
            UpdateData();

            IssueReports.Clear();

            if (!ApplicationHelpers.VersionCompare(Version, DLLVersion, DisplayName, out string message))
            {
                IssueReports.Add(new IssueReport { IssueReportType = IssueReportType.VersionMissMatch, DLLVersion = DLLVersion});
            }

            if (!Enable)
            {
                IssueReports.Add(new IssueReport { IssueReportType = IssueReportType.ModDisabled });
            }

            UpdateHasIssues();
            //TODO Use HomeViewModel to look for the dependencies
        }

        private void UpdateHasIssues()
        {
            HasIssues = IssueReports.Count > 0;
        }

        #endregion
    }

    public partial class ModItemViewModel
    {
        #region ICommands
        [JsonIgnore] public ICommand FixBTNCommand { get; set; }
        [JsonIgnore] public ICommand PackageBTNCommand { get; set; }
        [JsonIgnore] public ICommand ModBTNCommand { get; set; }
        [JsonIgnore] public ICommand ChangePhotoBTNCommand { get; set; }
        [JsonIgnore] public ICommand OpenModFolderBTNCommand { get; set; }

        #endregion

        #region Public Methods

        public static ModItemViewModel FromJson(string json) => JsonConvert.DeserializeObject<ModItemViewModel>(json);

        [JsonIgnore]
        public ObservableCollection<DirectoryItemViewModel> FileStructure { get; set; } = new ObservableCollection<DirectoryItemViewModel>();
        public ModItemViewModel()
        {
            FixBTNCommand = new RelayCommand(FixBTNCommandMethod);
            PackageBTNCommand = new RelayCommand(PackageBTNCommandMethod);
            ModBTNCommand = new RelayCommand(ModBTNCommandMethod);
            ChangePhotoBTNCommand = new RelayCommand(ChangePhotoBTNCommandMethod);
            OpenModFolderBTNCommand = new RelayCommand(OpenModFolderBTNCommandMethod);
        }
        
        #endregion

        #region Private Methods

        private void ChangePhotoBTNCommandMethod()
        {
            if (string.IsNullOrEmpty(_iconPath))
            {
                using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
                {
                    //ofd.DefaultFileName = !string.IsNullOrEmpty(_iconPath) ? Path.GetFileName(_iconPath) : $"CLASS_NAME.png";
                    ofd.Filters.Add(new CommonFileDialogFilter("PNG Image", "*.png"));
                    if (ofd.ShowDialog() != CommonFileDialogResult.Ok) return;

                    //TODO Add Message dialog to let them know to chose a file that has already been renamed correctly
                    var iconName = Path.GetFileNameWithoutExtension(_iconPath);
                    ApplicationHelpers.ReplaceModIcon(iconName, _modFolder, ofd.FileName);
                    IconImageValue = CacheImage(_iconPath);
                }
            }
            else
            {
                using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
                {
                    //ofd.DefaultFileName = !string.IsNullOrEmpty(_iconPath) ? Path.GetFileName(_iconPath) : $"CLASS_NAME.png";
                    ofd.Filters.Add(new CommonFileDialogFilter("PNG Image", "*.png"));
                    if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        ApplicationHelpers.ReplaceModIcon(Path.GetFileNameWithoutExtension(_iconPath), _modFolder, ofd.FileName);
                        IconImageValue = CacheImage(_iconPath);
                    }
                }
            }
        }

        private void OpenModFolderBTNCommandMethod()
        {
            try
            {
                // opens the folder in explorer
                Process.Start(_modFolder);
            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError(e.Message);
            }
        }

        private void ModBTNCommandMethod()
        {
            UpdateTreeView();

            var vm = new EditorPageViewModel(this);
            //vm.OnSaveAction += OnSave;
            ViewModelApplication.GoToPage(ApplicationPage.EditorPage, vm);
        }

        /// <summary>
        /// Updates the treeview for the directory structure
        /// </summary>
        public void UpdateTreeView()
        {
            var children = DirectoryStructure.GetLogicalDrives();

            // //Create the view models from the data
            var child = Directory.GetDirectories(Parent);
            FileStructure = new ObservableCollection<DirectoryItemViewModel>(
                children.Select(drive => new DirectoryItemViewModel(drive.FullPath, DirectoryItemType.Drive, drive.ChildCount)));

            FileStructure = new ObservableCollection<DirectoryItemViewModel>
            {
                new DirectoryItemViewModel(Parent, DirectoryItemType.Folder, child.Length)
            };

            // Expand the root tree node
            FileStructure[0].IsExpanded = true;

            if (File.Exists(_fileStructureSave))
            {
                var json = File.ReadAllText(_fileStructureSave);
                var save = JsonConvert.DeserializeObject<ObservableCollection<DirectoryItemViewModel>>(json);

                FileStructure = save;
            }

            FileStructure[0].Visibility = Visibility.Collapsed;
            FileStructure[0].IsChecked = true;
            FileStructure[0].IsRoot = true;

            foreach (DirectoryItemViewModel child1 in FileStructure[0].Children)
            {
                if (child1 == null) continue;
                child1.IsInRoot = true;
            }
        }

        internal void SetDependentOnTechFab()
        {
            if (TechFabDependancy) return;

            if (Dependencies == null)
            {
                TechFabDependancy = false;
                return;
            }

            if (VersionDependencies == null)
            {
                TechFabDependancy = false;
                return;
            }

            if (VersionDependencies.ContainsKey("FCSTechFabricator") || Dependencies.Contains("FCSTechFabricator"))
            {
                TechFabDependancy = true;
            }
        }

        private string GetModSize()
        {
            if (string.IsNullOrEmpty(Location)) return "N/A";

            DirectoryInfo di = new DirectoryInfo(_modFolder);
            var dirSize = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            return DirectorySizeHelper.SizeSuffix(dirSize);
        }

        private string GetVersionNumber()
        {

            var dllLocation = GetDLLLocation();
            if (string.IsNullOrEmpty(dllLocation)) return "N/A";
            var versionInfo = FileVersionInfo.GetVersionInfo(dllLocation);
            return ApplicationHelpers.VersionToSematic(versionInfo.ProductVersion); // Will typically return "1.0.0" in your case
        }

        private string GetDLLLocation()
        {
            if (string.IsNullOrEmpty(Location)) return string.Empty;
            string[] files = Directory.GetFiles(_modFolder, "*.dll");

            foreach (string file in files)
            {
                if (Path.GetFileName(file).Equals(AssemblyName))
                {
                    return file;
                }
            }

            return string.Empty;
        }

        private void FixBTNCommandMethod()
        {
            //TODO Go to the Fix Page.
            for (int i = 0; i < IssueReports.Count; i++)
            {
                switch (IssueReports[0].IssueReportType)
                {
                    case IssueReportType.VersionMissMatch:
                        Version = IssueReports[0].DLLVersion;
                        SaveModConfig();
                        IssueReports.Remove(IssueReports[0]);
                        break;
                    case IssueReportType.MissingDependancy:
                        break;
                    case IssueReportType.ModDisabled:
                        Enable = true;
                        IssueReports.Remove(IssueReports[0]);
                        SaveModConfig();
                        break;
                    case IssueReportType.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                UpdateHasIssues();
            }
            
            CheckForIssues();

        }

        internal void SaveModConfig()
        {
            try
            {
                File.WriteAllText(Location, this.ToJson());
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void PackageBTNCommandMethod()
        {
            string result = Path.Combine(Path.GetTempPath(), Path.GetFileName(_modFolder));
            bool completed = true;

            try
            {
                UpdateTreeView();

                using (CommonSaveFileDialog sfd = new CommonSaveFileDialog())
                {
                    sfd.DefaultFileName = $"{Id}_{Version}";
                    sfd.DefaultExtension = "zip";
                    sfd.AlwaysAppendDefaultExtension = true;
                    sfd.Filters.Add(new CommonFileDialogFilter("Zip File", "*.zip"));

                    if (sfd.ShowDialog() == CommonFileDialogResult.Ok)
                    {

                        ViewModelApplication.SetStatus(ApplicationStatusTypes.Running, "Zipping", StatusBarColors.Warning);

                        ApplicationHelpers.SafeDeleteFile(sfd.FileName);
                        ApplicationHelpers.SafeDeleteDirectory(result);

                        Directory.CreateDirectory(result);

                        foreach (DirectoryItemViewModel fileStructure in FileStructure)
                        {
                            completed = ProcessCreatingDirectories(fileStructure, result);
                        }

                        ZipFile.CreateFromDirectory(result, sfd.FileName, CompressionLevel.Optimal, true);

                        if (completed)
                        {
                            ViewModelApplication.SetStatus(ApplicationStatusTypes.Ready, "Zipping Completed.");
                        }

                        ApplicationHelpers.SafeDeleteDirectory(result);
                    }
                    else
                    {
                        ViewModelApplication.DefaultStatus();

                        if (Directory.Exists(result))
                        {
                            Directory.Delete(result, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ApplicationHelpers.SafeDeleteDirectory(result);
                ViewModelApplication.DefaultError(e.Message);
            }
        }

        private bool ProcessCreatingDirectories(DirectoryItemViewModel fileStructure, string result)
        {
            try
            {
                foreach (DirectoryItemViewModel child in fileStructure.Children)
                {
                    if (child.Type == DirectoryItemType.Drive || child.Type == DirectoryItemType.Folder)
                    {
                        Directory.CreateDirectory(Path.Combine(result, child.Name));

                        if (child.HasItems)
                        {
                            ProcessCreatingDirectories(child, result);
                        }
                    }
                    else
                    {
                        if (child.Type != DirectoryItemType.File) continue;

                        var destFile = Path.Combine(result, child.IsInRoot ? string.Empty : child.GetDirectoryName(), child.Name);

                        File.Copy(child.FullPath, destFile);
                    }
                }
            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError(e.Message);
                return false;
            }

            return true;
        }

        internal void SaveCurrentFileStructure()
        {
            var savedStructure = JsonConvert.SerializeObject(FileStructure, Formatting.Indented);
            File.WriteAllText(_fileStructureSave, savedStructure);
        }

        #endregion

        internal void UpdateElement()
        {
            CheckForIssues();
            UpdateDependencyData();
            SetDependentOnTechFab();
            SaveCurrentFileStructure();
            SaveModConfig();
        }

        internal void UpdateDependencyData()
        {
            if (Dependencies != null)
            {
                DependenciesCount = Dependencies.Count;
            }

            if (VersionDependencies != null)
            {
                VersionDependenciesCount = VersionDependencies.Count;
            }
        }
    }

    internal static class Serialize
    {
        public static string ToJson(this ModItemViewModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
