using System;
using FCS_DeepDriller.Buildable.MK1;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Objects;
using FCSCommon.Utilities;

namespace FCS_DeepDriller.Model.Upgrades
{
    internal class MaxOreCountUpgrade : UpgradeFunction
    {
        public int Amount { get; set; }
        public override float PowerUsage => 0.2f;
        public override float Damage { get; }
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.MaxOreCount;
        public override string FriendlyName => "Max Ore Count";
        public TechType TechType { get; set; }

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
            return $"{FriendlyName} | TechType: {TechType} Limit: {Amount}";
        }

        internal static bool IsValid(string[] paraResults, out Tuple<TechType, int> data)
        {
            data = null;
            try
            {
                if (paraResults.Length != 2)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Message(string.Format(FCSDeepDrillerBuildable.IncorrectAmountOfParameterFormat(), "2", paraResults.Length), true);
                    return false;
                }

                int amount;
                if (int.TryParse(paraResults[1], out var result))
                {
                    amount = Convert.ToInt32(result);
                }
                else
                {
                    QuickLogger.Message(string.Format(FCSDeepDrillerBuildable.IncorrectParameterFormat(), "TechType,INT", "OS.MaxOreCount(Silver,10);"), true);
                    return false;
                }

                TechType techType;
                if (BiomeManager.IsApproved(paraResults[0].ToTechType()))
                {
                    techType = paraResults[0].ToTechType();
                }
                else
                {
                    QuickLogger.Message(string.Format(FCSDeepDrillerBuildable.NotOreErrorFormat(), paraResults[0]), true);
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