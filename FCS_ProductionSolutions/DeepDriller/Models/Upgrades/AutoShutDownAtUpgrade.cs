using System;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCSCommon.Utilities;

namespace FCS_ProductionSolutions.DeepDriller.Models.Upgrades
{
    internal class AutoShutDownAtUpgrade : UpgradeFunction
    {
        public double Time { get; set; }

        public override float PowerUsage => 0.1f;
        public override float Damage { get; }
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.AutoShutdownAt;
        public override string FriendlyName => "Auto Shut Down At";

        public override void ActivateUpdate()
        {

        }

        public override void DeActivateUpdate()
        {

        }

        public override void TriggerUpdate()
        {

        }

        internal static bool IsValid(string[] paraResults, out double data)
        {
            data = 0.0;
            try
            {
                if (paraResults.Length != 1)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 1 got {paraResults.Length}", true);
                    return false;
                }

                data = GetTime(paraResults[0]);
            }
            catch (Exception e)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private static double GetTime(string time)
        {
            switch (time)
            {
                case "night":
                    return DayNightCycle.main.timePassedAsDouble += 1200.0 - DayNightCycle.main.timePassed % 1200.0;

                case "day":
                    return DayNightCycle.main.timePassedAsDouble += 1200.0 - DayNightCycle.main.timePassed % 1200.0 + 600.0;

            }

            return 0.0;
        }

        public override string Format()
        {
            return $"{FriendlyName}: {Time} ";
        }
    }
}