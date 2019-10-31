using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSModdingUtility
{
    public class DialogStrings
    {
        public const string IDDesc = "Id: Your unique mod id. Can only contain alphanumeric characters and underscores. (required)\nType: string\nExample: \"BestMod\"";
        public const string DisplayName = "The display name of your mod. Just like the mod id, but it can contain any type of characters. (required)\nType: string\nExample: \"Best Mod\"";
        public const string Enable = "Whether or not to enable the mod. (optional, defaults to true)\nType: bool\nExample: true";
        public const string EntryMethod = "The method which is called to load the mod. The method must be public, static, and have no parameters. (required)\nType: string\nExample: \"BestMod.QMod.Patch\"";
        public const string AssemblyName = "The name of the DLL file which contains the mod. (required)\nType: string\nExample: \"BestMod.dll\"";
        public const string LoadBefore = "Specify mods that will be loaded after your mod. If a mod in this list isn't found, it is simply ignored. (optional, defaults to[])\nType: string[] \nExample: [ \"AModID\", \"SomeOtherModID\" ]";
        public const string VersionDependencies = "Just like Dependencies, but you can specify a version range for the needed mods. (optional, default to {})\nType: Dictionary<string, string>\nExample: { \"SMLHelper\": \"2.x\", \"AnotherVeryImportantMod\": \">1.2.3\" }\nNote: The versioning system which is used is SemVer. ";
        public const string Dependencies = "Other mods that your mod needs. If a dependency is not found, the mod isn't loaded. (optional, defaults to[])\nType: string[]\nExample: [ \"DependencyModID\" ]";
        public const string Game = "The game that this mod is for. Can be \"Subnautica\", \"BelowZero\", or \"Both\" (optional, defaults to \"Subnautica\")\nType: string\nExample: \"Subnautica\"";
        public const string Version = "The mod version. This needs to be updated every time your mod gets updated (please update it). (required)\nType: string\nExample: \"1.0.0\"";
        public const string Author = "Your username. Should be the same across all your mods. Can contain any type of characters. (required)\nType: string\nExample: \"Awesome Guy\"";
        public const string LoadAfter = "Specify mods that will be loaded before your mod. If a mod in this list isn't found, it is simply ignored. (optional, defaults to[])\nType: string[]\nExample: [ \"AnotherModID\" ]";
    }
}
