using System;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCSCommon.Utilities;
using FCSDeepDrillerBuildable = FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Buildable.FCSDeepDrillerBuildable;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Models.Upgrades
{
    internal class MaxOreCountUpgrade : UpgradeFunction
    {
        private int _amount;

        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                UpdateLabel();
            }
        }

        public override float PowerUsage => QPatch.Configuration.DDMaxOreCountUpgradePowerUsage;
        public override float Damage { get; } = 0;
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.MaxOreCount;
        public override string FriendlyName => "Max Ore Count";
        public TechType TechType { get; set; }

        public override string GetFunction()
        {
            return $"os.MaxOreCount({TechType},{Amount});";
        }

        public override void ActivateUpdate()
        {

        }

        public override void DeActivateUpdate()
        {

        }

        public override void TriggerUpdate()
        {

        }

        public override string Format()
        {
            var isActive = IsEnabled ? Language.main.Get("BaseBioReactorActive") : Language.main.Get("BaseBioReactorInactive");
            return $"{FriendlyName} | TechType: {TechType} Limit: {Amount} ({isActive})";
        }

        internal static bool IsValid(string[] paraResults, out Tuple<TechType, int> data)
        {
            data = null;
            try
            {
                if (paraResults.Length != 2)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Message(FCSDeepDrillerBuildable.IncorrectAmountOfParameterFormat("2", paraResults.Length), true);
                    return false;
                }

                int amount;
                if (int.TryParse(paraResults[1], out var result))
                {
                    amount = Convert.ToInt32(result);
                }
                else
                {
                    QuickLogger.Message(FCSDeepDrillerBuildable.IncorrectParameterFormat("TechType,INT", "OS.MaxOreCount(Silver,10);"), true);
                    return false;
                }

                TechType techType;
                if (BiomeManager.IsApproved(paraResults[0].ToTechType()))
                {
                    techType = paraResults[0].ToTechType();
                }
                else
                {
                    QuickLogger.Message(FCSDeepDrillerBuildable.NotOreErrorFormat(paraResults[0]), true);
                    return false;
                }

                data = new Tuple<TechType, int>(techType, amount);

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
    }
}