using System;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models.Upgrades;

internal class OresPerDayUpgrade : UpgradeFunction
{
    private int _oreCount;
    public override UpgradeFunctions UpgradeType => UpgradeFunctions.OresPerDay;

    public override float PowerUsage => CalculatePowerUsage();

    public override float Damage { get; }

    public override string FriendlyName => "Ores Per Day";

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

    private float CalculatePowerUsage()
    {
        ;
        if (OreCount <= Plugin.Configuration.DDDefaultOrePerDay)
        {
            return Plugin.Configuration.DDDefaultOperationalPowerUsage + OreCount * Plugin.Configuration.DDOrePowerUsageBelowDefault;
        }

        return Mathf.Pow((Plugin.Configuration.DDDefaultOperationalPowerUsage + Plugin.Configuration.DDOrePowerUsage) * (OreCount / Plugin.Configuration.DDDefaultOrePerDay), 1.2f);
    }

    public override void ActivateUpdate()
    {
        if (Mono != null)
        {
            ((DeepDrillerController)Mono).GetOreGenerator().SetOresPerDay(OreCount);
        }
    }

    public override void DeActivateUpdate()
    {
        if (Mono != null)
        {
            ((DeepDrillerController)Mono).GetOreGenerator().SetOresPerDay(12);
        }
    }

    public override void TriggerUpdate()
    {
        if (Mono != null)
        {
            ((DeepDrillerController)Mono).GetOreGenerator().SetOresPerDay(OreCount);
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
                QuickLogger.Message(AuxPatchers.IncorrectAmountOfParameterFormat("1", paraResults.Length), true);
                return false;
            }

            if (int.TryParse(paraResults[0], out var result))
            {
                if (result is < 0 or > 100) return false;
                amountPerDay = result;
            }
            else
            {
                QuickLogger.Message(AuxPatchers.IncorrectParameterFormat("INT", "OS.OresPerDay(10);"), true);
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
        //
        //((DeepDrillerController)Mono)?.DisplayHandler?.UpdateDisplayValues();
        return $"{FriendlyName} | {OreCount} ({isActive})";
    }


}