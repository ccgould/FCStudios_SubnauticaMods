using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FCSModdingUtility
{
    public partial class ModItemViewModel: BaseViewModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Author")]
        public string Author { get; set; }


        [JsonProperty("Version")]
        public string Version { get; set; }


        [JsonIgnore]
        public string DLLVersion { get; set; }

        [JsonProperty("Game")]
        public string Game { get; set; }


        private List<string> _dependancies;

        [JsonProperty("Dependencies")]
        public List<string> Dependencies
        {
            get => _dependancies;
            set
            {
                _dependancies = value;
                DependenciesCount = value.Count;
            }
        }

        [JsonIgnore]
        public int DependenciesCount { get; set; }


        private Dictionary<string, string> _versionDependencies;

        [JsonProperty("VersionDependencies")]
        public Dictionary<string, string> VersionDependencies
        {
            get => _versionDependencies;
            set
            {
                _versionDependencies = value;
                VersionDependenciesCount = value.Count;
            }
        }


        [JsonIgnore]
        public int VersionDependenciesCount { get; set; }

        [JsonProperty("LoadBefore")]
        public List<string> LoadBefore { get; set; }

        [JsonProperty("AssemblyName")]
        public string AssemblyName { get; set; }

        [JsonProperty("EntryMethod")]
        public string EntryMethod { get; set; }

        [JsonProperty("Enable")]
        public bool Enable { get; set; }

        [JsonIgnore]
        public string ModSize { get; set; }

        [JsonIgnore]
        private string _location;

        [JsonIgnore]
        public string Location
        {
            get => _location;
            set
            {
                _location = value; 
                ModSize = GetModSize();
                DLLVersion = GetVersionNumber();
            }
        }
    }


    public partial class ModItemViewModel
    {
        [JsonIgnore] public ICommand FixBTNCommand { get; set; }
        
        public static ModItemViewModel FromJson(string json) => JsonConvert.DeserializeObject<ModItemViewModel>(json);

        public ModItemViewModel()
        {
            FixBTNCommand = new RelayCommand(FixBTNCommandMethod);
        }

        private string GetModSize()
        {
            if (string.IsNullOrEmpty(_location)) return "N/A";

            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(_location));
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
            string[] files = Directory.GetFiles(Path.GetDirectoryName(_location), "*.dll");

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
            var export = this.ToJson();
        }
    }

    public static class Serialize
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
