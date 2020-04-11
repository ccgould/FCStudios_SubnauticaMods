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
        private const string PingingKey = "OS_Pinging";
        private const string PingKey = "OS_Ping";
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
            LanguageHandler.SetLanguageLine(PingKey, "PING");
            LanguageHandler.SetLanguageLine(PingingKey, "PINGING");
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

        internal static string BeaconAttached()
        {
            return Language.main.Get(BeaconAttachedKey);
        }

        internal static string Pinging()
        {
            return Language.main.Get(PingingKey);
        }
        
        internal static string Ping()
        {
            return Language.main.Get(PingKey);
        }
        
        #endregion


    }
}
