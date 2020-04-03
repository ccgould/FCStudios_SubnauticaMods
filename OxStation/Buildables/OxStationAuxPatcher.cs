using SMLHelper.V2.Handlers;

namespace MAC.OxStation.Buildables
{
    internal partial class OxStationBuildable
    {
        #region Private Members
        private const string PowerUsageKey = "OS_PowerUsage";
        private const string PerMinuteKey = "OS_PerMinute";
        private const string TakeOxygenKey = "OS_TakeOxygen";
        private const string BeaconAttachedKey = "OS_BeaconAttached";
        #endregion

        #region Internal Properties
        internal static string BuildableName { get; private set; }
        internal static TechType TechTypeID { get; private set; }
        #endregion

        #region Private Methods
        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(PowerUsageKey, "Power Usage");
            LanguageHandler.SetLanguageLine(PerMinuteKey, "per minute");
            LanguageHandler.SetLanguageLine(TakeOxygenKey, "Take Oxygen");
            LanguageHandler.SetLanguageLine(BeaconAttachedKey, "Remove attached beacon to deconstruct.");
        }
        #endregion

        #region Internal Methods
        internal static string PowerUsage()
        {
            return Language.main.Get(PowerUsageKey);
        }

        internal static string PerMinute()
        {
            return Language.main.Get(PerMinuteKey);
        }


        internal static string TakeOxygen()
        {
            return Language.main.Get(TakeOxygenKey);
        }

        internal static string Damaged()
        {
            return Language.main.Get("Damaged");
        }
        #endregion

        public static string BeaconAttached()
        {
            return Language.main.Get(BeaconAttachedKey);
        }
    }
}
