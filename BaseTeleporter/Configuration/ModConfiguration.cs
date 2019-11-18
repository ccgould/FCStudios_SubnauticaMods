using System.Collections.Generic;
using FCSCommon.Interfaces;

namespace AE.BaseTeleporter.Configuration
{
    internal class ModConfiguration
    {
        public string Game { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string AssemblyName { get; set; }
        public string EntryMethod { get; set; }
        public List<string> Dependencies { get; set; }
        public List<string> LoadAfter { get; set; }
        public List<string> LoadABefore { get; set; }
        public bool Enable { get; set; }

        //Add Custom Configuration
        public Config Config { get; set; }
    }

    internal class Config
    {
        public bool UseSpoolTime { get; set; } = true;
        public float SpoolTime { get; set; } = 3.0f;
        public float Cooldown { get; set; } = 3.0f;
        public float PowerUsage { get; set; } = 75.0f;
    }
}
