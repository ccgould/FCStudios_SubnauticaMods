using System;
using FCSCommon.Utilities;
using QuantumTeleporter.Configuration;
using SMLHelper.V2.Handlers;

namespace QuantumTeleporter.Buildable
{
    internal partial class QuantumTeleporterBuildable
    {
        #region Public Properties

        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }

        #endregion

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(OnHoverKey, "On Hover");
            LanguageHandler.SetLanguageLine(UnitNotEmptyKey, "One or both of the container are not empty.");
            LanguageHandler.SetLanguageLine(StorageLabelKey, $"{Mod.FriendlyName} Receptacle");
            LanguageHandler.SetLanguageLine(NotEnoughPowerKey, "Not Enough Power To Teleport");
            LanguageHandler.SetLanguageLine(BaseNotEnoughPowerKey, "This base does not have enough power to teleport");
            LanguageHandler.SetLanguageLine(ToNotEnoughPowerKey, "Destination base does not have enough power to teleport");
            LanguageHandler.SetLanguageLine(OfflineKey, "OFFLINE");
            LanguageHandler.SetLanguageLine(OnlineKey, "ONLINE");
            LanguageHandler.SetLanguageLine(CoordinateKey, "Coordinates");
            LanguageHandler.SetLanguageLine(PowerAvailableKey, "Power Available");
            LanguageHandler.SetLanguageLine(ConfirmKey, "Confirm");
            LanguageHandler.SetLanguageLine(LocationKey, "Location");
            LanguageHandler.SetLanguageLine(GlobalTeleportKey, "Global Teleport Power Requirement");
            LanguageHandler.SetLanguageLine(InternalTeleportKey, "Internal Teleport Power Requirement");
            LanguageHandler.SetLanguageLine(ConfirmMessageKey, "Initiate Teleportation?");
            LanguageHandler.SetLanguageLine(PerUnitKey, "per unit");
        }

        private const string OnHoverKey = "QT_OnHover";
        private const string UnitNotEmptyKey = "QT_UnitNotEmpty";
        private const string StorageLabelKey = "QT_StorageLabel";
        private const string NotEnoughPowerKey = "QT_NotEnoughPower";
        private const string BaseNotEnoughPowerKey = "QT_BaseNotEnoughPower";
        private const string ToNotEnoughPowerKey = "QT_ToNotEnoughPower";
        private const string OnlineKey = "QT_Online";
        private const string OfflineKey = "QT_Offline";
        private const string CoordinateKey = "QT_Coordinates";
        private const string PowerAvailableKey = "QT_PowerAvailable";
        private const string ConfirmKey = "QT_Confirm";
        private const string LocationKey = "QT_Location";
        private const string GlobalTeleportKey = "QT_GlobalTeleport";
        private const string InternalTeleportKey = "QT_InternalTeleport";
        private const string ConfirmMessageKey = "QT_ConfirmMessage";
        private const string PerUnitKey = "QT_PerUnit";

        internal static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }

        internal static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        internal static string UnitNotEmpty()
        {
            return Language.main.Get(UnitNotEmptyKey);
        }

        internal static string Version()
        {
            return Language.main.Get("Version");
        }
        
        internal static string Submit()
        {
            return Language.main.Get("Submit");
        }

        internal static string NotEnoughPower()
        {
            return Language.main.Get(NotEnoughPowerKey);
        }

        internal static string BaseNotEnoughPower()
        {
            return Language.main.Get(BaseNotEnoughPowerKey);
        }

        internal static string ToBaseNotEnoughPower()
        {
            return Language.main.Get(ToNotEnoughPowerKey);
        }

        internal static string Online()
        {
            return Language.main.Get(OnlineKey);  
        }

        internal static string Offline()
        {
            return Language.main.Get(OfflineKey);
        }

        internal static string Coordinate()
        {
            return Language.main.Get(CoordinateKey);
        }

        internal static string PowerAvailable()
        {
            return Language.main.Get(PowerAvailableKey);
        }
        
        internal static string OpenDoor()
        {
            return Language.main.Get("OpenDoor");
        }
        
        internal static string CloseDoor()
        {
            return Language.main.Get("CloseDoor");
        }
        
        internal static string Confirm()
        {
            return Language.main.Get(ConfirmKey);
        }

        internal static string Cancel()
        {
            return Language.main.Get("Cancel");
        }

        internal static object Location()
        {
            return Language.main.Get(LocationKey);
        }

        internal static object GlobalTeleport()
        {
            return Language.main.Get(GlobalTeleportKey);
        }

        internal static object InternalTeleport()
        {
            return Language.main.Get(InternalTeleportKey);
        }

        public static string ConfirmMessage()
        {
            return Language.main.Get(ConfirmMessageKey);
        }

        public static string PerUnit()
        {
            return Language.main.Get(PerUnitKey);
        }
    }
}
