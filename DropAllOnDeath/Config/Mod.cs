using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Options;
using System;
using System.IO;

namespace AE.DropAllOnDeath.Config
{
    internal static class Mod
    {
        #region Internal Properties
        internal static string ModName => "DropAllOnDeath";
        internal static string ModFolderName => $"FCS_{ModName}";
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static Config Configuration { get; set; }
        #endregion

        #region Internal Methods
        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
        }

        internal static void SaveConfig(bool isEnabled,bool spawnAtEscapePod)
        {
            Configuration.Enabled = isEnabled;
            Configuration.EscapePodRespawn = spawnAtEscapePod;
            SaveConfiguration();
        }
        #endregion

        #region Private Methods
        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }
        private static void SaveConfiguration()
        {
            var output = JsonConvert.SerializeObject(Configuration, Formatting.Indented);
            File.WriteAllText(ConfigurationFile().Trim(), output);
        }
        #endregion
        
        internal static bool IsConfigAvailable()
        {
            return File.Exists(ConfigurationFile());
        }

        private static void CreateModConfiguration()
        {
            var config = new Config();

            var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigurationFile()), saveDataJson);
        }

        private static Config LoadConfigurationData()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // == LoadData == //
            return JsonConvert.DeserializeObject<Config>(configJson, settings);
        }

        internal static Config LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }
    }

    internal class Options : ModOptions
    {
        private const string ToggleID = "DropAllOnDeathEnabled";
        private const string RespawnAtEscapePodID = "RespawnAtEscapePod";
        private bool _enabled;
        private bool _spawnAtEscapePod;

        public Options() : base("Drop All On Death Settings")
        {
            ToggleChanged += OnToggleChanged;
            _enabled = Mod.Configuration.Enabled;
            _spawnAtEscapePod = Mod.Configuration.EscapePodRespawn;
        }

        public void OnToggleChanged(object sender, ToggleChangedEventArgs e)
        {

            switch (e.Id)
            {
                case ToggleID:
                    _enabled = e.Value;
                    break;
                case RespawnAtEscapePodID:
                    _spawnAtEscapePod = e.Value;
                    break;
            }

            Mod.SaveConfig(_enabled,_spawnAtEscapePod);
        }

        public override void BuildModOptions()
        {
            AddToggleOption(ToggleID, "Enable Drop All On Death", _enabled);
            AddToggleOption(RespawnAtEscapePodID, "Respawn At Escape Pod", _spawnAtEscapePod);
        }
    }
}
