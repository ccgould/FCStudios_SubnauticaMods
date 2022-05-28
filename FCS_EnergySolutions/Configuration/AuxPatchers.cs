using System.Collections.Generic;
using SMLHelper.V2.Handlers;

namespace FCS_EnergySolutions.Configuration
{
    internal static class AuxPatchers
    {
        private const string ModKey = "AES";

        private static readonly Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_JetStreamT242OnHover","{0} Information: (charge: {1}/{2}) | (production per minute: {3})"},
            { $"{ModKey}_WindSurferOnHover","WindSurfer {0} Information: (charge: {1}/{2}) | (production per minute: {3})"},
            { $"{ModKey}_JetStreamOnHoverInteractionFormatted","Press {0} to Turn on / off Turbine. Current State {1}"},
            { $"{ModKey}_OnlyPowercellsAllowed","Only powercells are allowed."},
            { $"{ModKey}_UniversalChargerNotEmpty","Please remove all powercells before trying to deconstruct."},
            { $"{ModKey}_UniversalChargerCannotChangeMode","Cannot switch mode because charger is not empty"},
            { $"{ModKey}_UniversalChargerSwitchMode","Switch Charger Mode"},
            { $"{ModKey}_UniversalChargerSwitchModeDesc","Changes the charger between charging powercells and batteries"},
            { $"{ModKey}_NotUnderWater","Cannot operate out of water."},
            { $"{ModKey}_NotUnderWaterDesc","Turbine cannot function  above 3 meters"},
            { $"{ModKey}_NotOnPlatform","JetStreamT242 must be built on a base platform to operate. This is best for efficiency and stability."},
            { $"{ModKey}_SolarClusterHover","(sun: {0}% charge: {1} /{2}) (Power Generation Per Second: {3})"},
            { $"{ModKey}_RemoveAllTelepowerConnections","Please remove all connections to in this pylon before deconstruction."},
            { $"{ModKey}_RemoveAllTelepowerConnectionsPush","Please remove all connections to this pylon before switching to push mode."},
            { $"{ModKey}_RemoveAllTelepowerConnectionsPull","Please remove all connections to this pylon before switching to pull mode."},
            { $"{ModKey}_MaximumConnectionsReached","The maximum amount of connections have been reached. Please use an upgrade to get more slots."},
            { $"{ModKey}_SelectAMode","Select a Mode or Connect to another Pylon."},
            { $"{ModKey}_WindSurferMaxReached","Maximum platform allowance reached. No more platforms can be added."},
        };

        internal static void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }

        private static string GetLanguage(string key)
        {
            var newKey = $"{ModKey}_{key}";
            return LanguageDictionary.ContainsKey(newKey) ? Language.main.Get(newKey) : "N/A";
        }

        internal static string JetStreamOnHover()
        {
            return GetLanguage("JetStreamT242OnHover");
        }

        public static string JetStreamOnHoverInteractionFormatted(string keyBind,string state)
        {
            return string.Format(GetLanguage( "JetStreamOnHoverInteractionFormatted"), keyBind,state);
        }


        public static string UniversalChargerNotEmpty()
        {
            return GetLanguage("UniversalChargerNotEmpty");
        }

        public static string UniversalChargerCannotChangeMode()
        {
            return GetLanguage("UniversalChargerCannotChangeMode");
        }

        public static string NotUnderWater()
        {
            return GetLanguage("NotUnderWater");
        }

        public static string NotUnderWaterDesc()
        {
            return GetLanguage("NotUnderWaterDesc");
        }        
        
        public static string UniversalChargerSwitchMode()
        {
            return GetLanguage("UniversalChargerSwitchMode");
        }

        public static string UniversalChargerSwitchModeDesc()
        {
            return GetLanguage("UniversalChargerSwitchModeDesc");
        }

        public static string NotOnPlatform()
        {
            return GetLanguage("NotOnPlatform");
        }

        public static string SolarClusterHover(int sun, int charge, int capacity,int produce)
        {
            return string.Format(GetLanguage("SolarClusterHover"),sun,charge,capacity,produce);
        }

        public static string RemoveAllTelepowerConnections()
        {
            return GetLanguage("RemoveAllTelepowerConnections");
        }

        public static string RemoveAllTelepowerConnectionsPush()
        {
            return GetLanguage("RemoveAllTelepowerConnectionsPush");
        }

        public static string RemoveAllTelepowerConnectionsPull()
        {
            return GetLanguage("RemoveAllTelepowerConnectionsPull");
        }

        public static string MaximumConnectionsReached()
        {
            return GetLanguage("MaximumConnectionsReached");
        }

        public static string WindSurferOnHover()
        {
            return GetLanguage("WindSurferOnHover");
        }

        public static string SelectAMode()
        {
            return GetLanguage("SelectAMode");
        }

        public static string WindSurferMaxReached()
        {
            return GetLanguage("WindSurferMaxReached");
        }
    }
}
