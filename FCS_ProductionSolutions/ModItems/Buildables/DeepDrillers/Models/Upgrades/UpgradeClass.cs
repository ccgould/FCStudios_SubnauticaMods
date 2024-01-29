﻿namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models.Upgrades;

internal class UpgradeClass
{
    internal string FunctionName { get; set; }
    internal string FriendlyName { get; set; }
    internal string Template { get; set; }



    public override string ToString()
    {
        return FriendlyName;
    }
}