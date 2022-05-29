using System;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Models.Upgrades
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

        public override float PowerUsage => CalculatePowerUsage();

        private float CalculatePowerUsage()
        {
            ;
            if (OreCount <= QPatch.Configuration.DDDefaultOrePerDay)
            {
                return QPatch.Configuration.DDDefaultOperationalPowerUsage + OreCount * QPatch.Configuration.DDOrePowerUsageBelowDefault;
            }
            
            return Mathf.Pow((QPatch.Configuration.DDDefaultOperationalPowerUsage + QPatch.Configuration.DDOrePowerUsage) * (OreCount / QPatch.Configuration.DDDefaultOrePerDay), 1.2f);
        }

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
                    QuickLogger.Message(FCSDeepDrillerBuildable.IncorrectAmountOfParameterFormat("1", paraResults.Length), true);
                    return false;
                }
                
                if (int.TryParse(paraResults[0], out var result))
                {
                    if (result is < 0 or > 100) return false;
                    amountPerDay = result;
                }
                else
                {
                    QuickLogger.Message(FCSDeepDrillerBuildable.IncorrectParameterFormat("INT", "OS.OresPerDay(10);"), true);
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