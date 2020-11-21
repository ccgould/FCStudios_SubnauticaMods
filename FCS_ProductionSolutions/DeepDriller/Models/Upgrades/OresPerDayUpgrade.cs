using System;
using FCS_ProductionSolutions.DeepDriller.Mono;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSDeepDrillerBuildable = FCS_ProductionSolutions.DeepDriller.Buildable.FCSDeepDrillerBuildable;

namespace FCS_ProductionSolutions.DeepDriller.Models.Upgrades
{
    internal class OresPerDayUpgrade : UpgradeFunction
    {
        private int _oreCount;

        public int OreCount
        {
            get => _oreCount;
            set
            {
                _oreCount = value;
                TriggerUpdate();
            }
        }

        public override string GetFunction()
        {
            return $"os.OresPerDay({OreCount});";
        }

        public override float PowerUsage => QPatch.DeepDrillerMk2Configuration.OrePerDayUpgradePowerUsage + (OreCount * QPatch.DeepDrillerMk2Configuration.OreReductionValue);
        public override float Damage { get; }

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
                UpdateLabel();
            }
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
            var isActive = IsEnabled ? Language.main.Get("BaseBioReactorActive") : Language.main.Get("BaseBioReactorInactive");
            ((FCSDeepDrillerController)Mono)?.DisplayHandler?.UpdateDisplayValues();
            return $"{FriendlyName} | {OreCount} ({isActive})";
        }


    }
}