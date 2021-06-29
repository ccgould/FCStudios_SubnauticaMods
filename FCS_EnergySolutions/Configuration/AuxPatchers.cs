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
            { $"{ModKey}_PowerStorageNotEmpty","Please remove all powercells before trying to deconstruct."},
            { $"{ModKey}_PowerStorageChargeMode","Charge Mode"},
            { $"{ModKey}_PowerStorageChargeModeDesc","In this mode PowerStorage will only charge the powercells for later use."},
            { $"{ModKey}_PowerStorageDischargeMode","Discharge Mode"},
            { $"{ModKey}_PowerStorageDischargeModeDesc","In this mode PowerStorage will use the powercells to power the habitat."},
            { $"{ModKey}_PowerStorageAutoMode","Auto Mode"},
            { $"{ModKey}_PowerStorageClickToAddPowercells","Click to add powercells"},
            { $"{ModKey}_PowerStorageAutoModeDesc","Automatically switches the mode of PowerStorage based on if the habitat goes into Emergency or PoweredOff"},
            { $"{ModKey}_NotUnderWater","Cannot operate out of water."},
            { $"{ModKey}_NotUnderWaterDesc","Turbine cannot function  above 3 meters"},
            { $"{ModKey}_NoSlotsAvailable","No charging slots available."},
            { $"{ModKey}_NotOnPlatform","JetStreamT242 must be built on a base platform to operate. This is best for efficiency and stability."},
            { $"{ModKey}_SolarClusterHover","(sun: {0}% charge: {1} /{2}) (Power Generation Per Second: {3})"},
            { $"{ModKey}_RemoveAllTelepowerConnections","Please remove all connections to in this pylon before deconstruction."},
            { $"{ModKey}_RemoveAllTelepowerConnectionsPush","Please remove all connections to this pylon before switching to push mode."},
            { $"{ModKey}_RemoveAllTelepowerConnectionsPull","Please remove all connections to this pylon before switching to pull mode."},
            { $"{ModKey}_MaximumConnectionsReached","The maximum amount of connections have been reached. Please use an upgrade to get more slots."},
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
            return LanguageDictionary.ContainsKey(key) ? Language.main.Get(key) : "N/A";
        }

        internal static string JetStreamOnHover()
        {
            return GetLanguage($"{ModKey}_JetStreamT242OnHover");
        }

        public static string JetStreamOnHoverInteractionFormatted(string keyBind,string state)
        {
            return string.Format(GetLanguage( $"{ModKey}_JetStreamOnHoverInteractionFormatted"), keyBind,state);
        }

        public static string OnlyPowercellsAllowed()
        {
            return GetLanguage($"{ModKey}_OnlyPowercellsAllowed");
        }

        public static string PowerStorageNotEmpty()
        {
            return GetLanguage($"{ModKey}_PowerStorageNotEmpty");
        }

        public static string PowerStorageChargeMode()
        {
            return GetLanguage($"{ModKey}_PowerStorageChargeMode");
        }

        public static string PowerStorageChargeModeDesc()
        {
            return GetLanguage($"{ModKey}_PowerStorageChargeModeDesc");
        }

        public static string PowerStorageDischargeMode()
        {
            return GetLanguage($"{ModKey}_PowerStorageDischargeMode");
        }

        public static string PowerStorageDischargeModeDesc()
        {
            return GetLanguage($"{ModKey}_PowerStorageDischargeModeDesc");
        }

        public static string PowerStorageAutoMode()
        {
            return GetLanguage($"{ModKey}_PowerStorageAutoMode");
        }

        public static string PowerStorageAutoModeDesc()
        {
            return GetLanguage($"{ModKey}_PowerStorageAutoModeDesc");
        }        
        public static string PowerStorageClickToAddPowercells()
        {
            return GetLanguage($"{ModKey}_PowerStorageClickToAddPowercells");
        }

        public static string NotUnderWater()
        {
            return GetLanguage($"{ModKey}_NotUnderWater");
        }

        public static string NotUnderWaterDesc()
        {
            return GetLanguage($"{ModKey}_NotUnderWaterDesc");
        }

        public static string NoSlotsAvailable()
        {
            return GetLanguage($"{ModKey}_NoSlotsAvailable");
        }

        public static string NotOnPlatform()
        {
            return GetLanguage($"{ModKey}_NotOnPlatform");
        }

        public static string SolarClusterHover(int sun, int charge, int capacity,int produce)
        {
            return string.Format(GetLanguage($"{ModKey}_SolarClusterHover"),sun,charge,capacity,produce);
        }

        public static string RemoveAllTelepowerConnections()
        {
            return GetLanguage($"{ModKey}_RemoveAllTelepowerConnections");
        }

        public static string RemoveAllTelepowerConnectionsPush()
        {
            return GetLanguage($"{ModKey}_RemoveAllTelepowerConnectionsPush");
        }

        public static string RemoveAllTelepowerConnectionsPull()
        {
            return GetLanguage($"{ModKey}_RemoveAllTelepowerConnectionsPull");
        }

        public static string MaximumConnectionsReached()
        {
            return GetLanguage($"{ModKey}_MaximumConnectionsReached");
        }

        public static string WindSurferOnHover()
        {
            return $"{ModKey}_WindSurferOnHover";
        }
    }
}
