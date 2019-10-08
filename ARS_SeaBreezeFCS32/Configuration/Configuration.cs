using System.Collections.Generic;

namespace ARS_SeaBreezeFCS32.Configuration
{
    public class Config
    {
        public int StorageLimit { get; set; }
    }

    public class ModConfiguration
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string AssemblyName { get; set; }
        public string EntryMethod { get; set; }
        public List<string> Dependencies { get; set; }
        public List<string> LoadAfter { get; set; }
        public Config Config { get; set; }
        public bool Enable { get; set; }
    }
}
