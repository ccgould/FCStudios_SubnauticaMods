using System.IO;
using System.Reflection;
using SMLHelper.V2.Crafting;

namespace FCS_HomeSolutions.Configuration
{
    internal static class Mod
    {
        internal const string ModName = "HomeSolutions";
        internal const string ModFriendlyName = "Alterra Home Solutions";
        internal const string ModBundleName = "fcsalterrahomesolutions";

        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }
    }
}
