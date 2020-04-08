using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_HydroponicHarvesters.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCS_HydroponicHarvesters.Buildables
{
    internal partial class HydroponicHarvestersBuidable
    {
        private const string MaxKey = "Max";
        private const string HighKey = "High";
        private const string MinKey = "Min";
        private const string LowKey = "Low";
        private const string OffKey = "Off";
        internal string BuildableName { get; set; }

        internal TechType TechTypeID { get; set; }

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = FriendlyName;
            TechTypeID = TechType;

            LanguageHandler.SetLanguageLine(MaxKey, "Max");
            LanguageHandler.SetLanguageLine(HighKey, "High");
            LanguageHandler.SetLanguageLine(MinKey, "Min");
            LanguageHandler.SetLanguageLine(LowKey, "Low");
            LanguageHandler.SetLanguageLine(OffKey, "Off");
        }

        internal static string Max()
        {
            return Language.main.Get(MaxKey);
        }

        internal static string High()
        {
            return Language.main.Get(HighKey);
        }

        internal static string Min()
        {
            return Language.main.Get(MinKey);
        }

        internal static string Low()
        {
            return Language.main.Get(LowKey);
        }

        internal static string Off()
        {
            return Language.main.Get(OffKey);
        }


    }
}
