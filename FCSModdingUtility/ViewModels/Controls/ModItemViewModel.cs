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
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
        private string _location;
        private string _modFolder;
        private string _fileStructureSave;

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
                TechFabDependancy = IsDependantOnTechFab();
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
                TechFabDependancy = IsDependantOnTechFab();
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
        public string Location
        {
            get => _location;
            set
            {
                _location = value;
                _modFolder = Path.GetDirectoryName(value);
                _fileStructureSave = Path.Combine(_modFolder, "FCSModdingUtilitySave.fcs");
                ModSize = GetModSize();
                DLLVersion = GetVersionNumber();
                IconImageValue = GetIconLocation();
            }
        }

        [JsonIgnore]
        public bool TechFabDependancy { get; set; }

        [JsonIgnore]
        public string DLLVersion { get; set; }

        [JsonIgnore]
        public int VersionDependenciesCount { get; set; }

        [JsonIgnore]
        public int DependenciesCount { get; set; }
        #endregion

        #region Private Methods
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

            if (_location == null || Id == null) return unknown;

            var image = Path.Combine(_modFolder, "Assets", $"{Id}.png");

            if (!File.Exists(image)) return unknown;

            return image;
        }
        #endregion
    }

    public partial class ModItemViewModel
    {
        #region ICommands
        [JsonIgnore] public ICommand FixBTNCommand { get; set; }
        [JsonIgnore] public ICommand PackageBTNCommand { get; set; }
        [JsonIgnore] public ICommand ModBTNCommand { get; set; }
        #endregion

        #region Public Methods

        public static ModItemViewModel FromJson(string json) => JsonConvert.DeserializeObject<ModItemViewModel>(json);
        
        [JsonIgnore]
        public ObservableCollection<FileStructure> FileStructure { get; set; } = new ObservableCollection<FileStructure>();
        public ModItemViewModel()
        {
            FixBTNCommand = new RelayCommand(FixBTNCommandMethod);
            PackageBTNCommand = new RelayCommand(PackageBTNCommandMethod);
            ModBTNCommand = new RelayCommand(ModBTNCommandMethod);
        } 
        #endregion

        #region Private Methods
        private void ModBTNCommandMethod()
        {
            LoadFileStructure();

            var vm = new EditorPageViewModel(this);
            //vm.OnSaveAction += OnSave;
            ViewModelApplication.GoToPage(ApplicationPage.EditorPage, vm);
        }

        private bool IsDependantOnTechFab()
        {
            if (TechFabDependancy) return true;

            if (Dependencies == null) return false;

            if (Dependencies.Contains("FCSTechFabricator")) return true;
            
            if (VersionDependencies == null) return false;

            if (VersionDependencies.ContainsKey("FCSTechFabricator")) return true;

            return false;
        }

        private string GetModSize()
        {
            if (string.IsNullOrEmpty(_location)) return "N/A";

            DirectoryInfo di = new DirectoryInfo(_modFolder);
            var dirSize = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            return DirectorySizeHelper.SizeSuffix(dirSize);
        }

        private string GetVersionNumber()
        {

            var dllLocation = GetDLLLocation();
            if (string.IsNullOrEmpty(dllLocation)) return "N/A";
            var versionInfo = FileVersionInfo.GetVersionInfo(dllLocation);
            return versionInfo.ProductVersion; // Will typically return "1.0.0" in your case
        }

        private string GetDLLLocation()
        {
            if (string.IsNullOrEmpty(_location)) return string.Empty;
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
            //SaveModConfig(export);
        }

        internal void SaveModConfig()
        {
            try
            {
                File.WriteAllText(_location, this.ToJson());
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void PackageBTNCommandMethod()
        {
            string result = Path.Combine(Path.GetTempPath(), Path.GetFileName(_modFolder));

            try
            {
                

                LoadFileStructure();

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

                        Directory.CreateDirectory(result);

                        foreach (FileStructure fileStructure in FileStructure)
                        {
                            if (fileStructure.IsDirectory && fileStructure.Include)
                            {
                                Directory.CreateDirectory(Path.Combine(result, fileStructure.GetFileName()));
                            }
                        }


                        foreach (FileStructure fileStructure in FileStructure)
                        {
                            if (!fileStructure.Include || fileStructure.IsDirectory) continue;

                            var destFile = Path.Combine(result, fileStructure.IsInRoot(Path.GetFileName(_modFolder)) ? string.Empty : fileStructure.GetDirectoryName(), fileStructure.GetFileName());

                            File.Copy(fileStructure.FileName, destFile);

                        }
                        
                        //Directory.Move();

                        ZipFile.CreateFromDirectory(result, sfd.FileName, CompressionLevel.Optimal, true);
                        ViewModelApplication.SetStatus(ApplicationStatusTypes.Ready,"Zipping Completed.");

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

        private void DeleteFile()
        {

        }

        private void LoadFileStructure()
        {
            FileStructure.Clear();

            string[] files = Directory.GetFiles(_modFolder,"*",SearchOption.AllDirectories);
            string[] dirs = Directory.GetDirectories(_modFolder,"*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                FileStructure.Add(new FileStructure{FileName = file,Include = Path.GetExtension(file).Contains(".fcs") ? false : true, Parent = Path.GetDirectoryName(file)});
            }

            foreach (string dir in dirs)
            {
                FileStructure.Add(new FileStructure { FileName = dir, Include = true, Parent = Path.GetDirectoryName(dir), IsDirectory = true});
            }

            //LoadPrevSettings
            if (File.Exists(_fileStructureSave))
            {
                var json = File.ReadAllText(_fileStructureSave);
                var save = JsonConvert.DeserializeObject<List<FileStructure>>(json);
                
                foreach (FileStructure saveStructure in save)
                {
                    foreach (FileStructure structure in FileStructure)
                    {
                        if (!structure.FileName.Equals(saveStructure.FileName)) continue;
                        structure.Include = saveStructure.Include;
                        break;
                    }
                }
            }
        }

        internal void SaveCurrentFileStructure()
        {
            var savedStructure = JsonConvert.SerializeObject(FileStructure, Formatting.Indented);
            File.WriteAllText(_fileStructureSave, savedStructure);
        }

        #endregion
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

    public class FileStructure : INotifyPropertyChanged
    {
        public string Parent { get; set; }
        public string FileName { get; set; }
        private bool _include;

        public bool Include
        {
            get => _include;
            set
            {
                _include = value;
                OnPropertyChanged();
            }
        }

        public bool IsDirectory { get; set; }
        
        public override string ToString()
        {
            return Parent;
        }

        internal string GetFileName()
        {
            return Path.GetFileName(FileName);
        }

        public string GetFileExtention()
        {
            if (!IsDirectory)
            {
                return Path.GetExtension(FileName);
            }

            return String.Empty;
        }

        internal string GetDirectoryName()
        {
            return Path.GetFileName(Parent);
        }

        internal bool IsInRoot(string root)
        {
            return Path.GetFileName(Parent).Equals(root);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
