using System.Text.RegularExpressions;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine.UI;

namespace AlterraGen.Buildables
{
    internal partial class AlterraGenBuildable
    {
        private const string OpenDumpContainerKey = "AG_OpenDumpContainer";
        private const string PowerStateLBLKey = "AG_PowerStateLBL";
        private const string PowerUnitLBLKey = "AG_PowerUnitLBL";
        private const string BatteryPercentageLBLKey = "AG_BatteryPercentageLBL";
        private const string BreakerStateLBLKey = "AG_BreakerStateLBL";
        internal string BuildableName { get; set; }

        internal TechType TechTypeID { get; set; }

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = FriendlyName;
            TechTypeID = TechType;

            LanguageHandler.SetLanguageLine(OpenDumpContainerKey, "Open Dump Container");
            LanguageHandler.SetLanguageLine(PowerStateLBLKey, "Power State:");
            LanguageHandler.SetLanguageLine(PowerUnitLBLKey, "kW:");
            LanguageHandler.SetLanguageLine(BatteryPercentageLBLKey, "Battery Percentage:");
            LanguageHandler.SetLanguageLine(BreakerStateLBLKey, "Breaker State:");
        }

        internal static string OpenDumpContainer()
        {
            return Language.main.Get(OpenDumpContainerKey);
        }

        internal static string PowerStateLBL()
        {
            return Language.main.Get(PowerStateLBLKey);
        }

        internal static string PowerUnitLBL()
        {
            return Language.main.Get(PowerUnitLBLKey);
        }

        internal static string BatteryPercentageLBL()
        {
            return Language.main.Get(BatteryPercentageLBLKey);
        }

        internal static string BreakerStateLBL()
        {
            return Language.main.Get(BreakerStateLBLKey);
        }

        internal static string Active()
        {
            return Language.main.Get("BaseNuclearReactorActive");
        }

        internal static string InActive()
        {
            return Language.main.Get("BaseNuclearReactorInactive");
        }

        internal static string NotAllowedItem()
        {
            return Language.main.Get("TimeCapsuleItemNotAllowed");
        }
    }
}
