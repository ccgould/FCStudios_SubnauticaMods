using System;
using FCS_AlterraHub.Core.Extensions;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Enumerators;
using FCSCommon.Utilities;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models.Upgrades;

internal class MinOreCountUpgrade : UpgradeFunction
{
    public int Amount { get; set; }
    public override float PowerUsage => 0.2f;
    public override float Damage { get; }
    public override UpgradeFunctions UpgradeType => UpgradeFunctions.MaxOreCount;
    public override string FriendlyName => "Min Ore Count";
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

    internal static bool IsValid(string[] paraResults, out Tuple<TechType, int> data)
    {
        data = null;
        try
        {
            if (paraResults.Length != 2)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Debug($"Incorrect amount of parameters expected 2 got {paraResults.Length}", true);
                return false;
            }

            if (paraResults[0].ToTechType() == TechType.None)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Debug($"Incorrect TechType Value", true);
                return false;
            }

            QuickLogger.Debug(
                int.TryParse(paraResults[1], out var result)
                    ? $"Converted to number {result}"
                    : "Incorrect type in parameter expected: INT ex: OreSystem.MinOreCount(Silver,10); .", true);

            var techType = paraResults[0].ToTechType();
            var amount = Convert.ToInt32(result);
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

    public override string Format()
    {
        return $"{FriendlyName} | {Amount} Ores";
    }
}