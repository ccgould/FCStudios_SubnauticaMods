using System;
using FCS_DeepDriller.Buildable.MK1;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSCommon.Utilities;

namespace FCS_DeepDriller.Model.Upgrades
{
    internal class OresPerDayUpgrade : UpgradeFunction
    {
        public int OreCount { get; set; }

        public override float PowerUsage => 1.0f;
        public override float Damage => 0.5f;
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.OresPerDay;
        public override string FriendlyName => "Ores Per Day";

        public override void ActivateUpdate()
        {
            if (Mono != null)
            {
                ((FCSDeepDrillerController)Mono).OreGenerator.SetOresPerDay(OreCount);
            }
        }

        public override void DeActivateUpdate()
        {
            if (Mono != null)
            {
                ((FCSDeepDrillerController)Mono).OreGenerator.SetOresPerDay(12);
            }
        }

        public override void TriggerUpdate()
        {
            if (Mono != null)
            {
                ((FCSDeepDrillerController)Mono).OreGenerator.SetOresPerDay(OreCount);
            }
        }

        internal void UpdateFunction()
        {

        }

        internal static bool IsValid(string[] paraResults, out int amountPerDay)
        {
            amountPerDay = 0;
            try
            {
                if (paraResults.Length != 1)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Message(string.Format(FCSDeepDrillerBuildable.IncorrectAmountOfParameterFormat(),"1", paraResults.Length), true);
                    return false;
                }

                if (int.TryParse(paraResults[0], out var result))
                {
                    amountPerDay = Convert.ToInt32(result);
                }
                else
                {
                    QuickLogger.Message(string.Format(FCSDeepDrillerBuildable.IncorrectParameterFormat(),"INT", "OS.OresPerDay(10);"), true);
                    return false;
                }
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

        public override string Format()
        {
            return $"{FriendlyName} | {OreCount}";
        }
    }
}