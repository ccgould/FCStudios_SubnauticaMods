using System;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono;
using FCSCommon.Utilities;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models.Upgrades;

internal class ConnectToBaseUpgrade : UpgradeFunction
{
    public override float PowerUsage => 0.1f;
    public override float Damage { get; }
    public override UpgradeFunctions UpgradeType => UpgradeFunctions.ConnectToBase;
    public override string FriendlyName => "Connect To Base";

    public override string GetFunction()
    {
        return $"os.ConnectToBase({BaseID});";
    }

    public override void ActivateUpdate()
    {
        if (Mono != null)
        {
            var controller = (DeepDrillerController)Mono;
            controller.CanBeSeenByTransceiver = true;
            var manager = HabitatService.main.GetHabitatByID(BaseID);
            manager.RegisterDevice(controller);
            controller.Manager = manager;
            UpdateLabel();
        }
    }

    public override void DeActivateUpdate()
    {
        if (Mono != null)
        {
            var controller = (DeepDrillerController)Mono;
            controller.CanBeSeenByTransceiver = false;
            var manager = HabitatService.main.GetHabitatByID(BaseID);
            manager.UnRegisterDevice(controller);
            controller.Manager = null;
            UpdateLabel();
        }
    }

    public override void TriggerUpdate()
    {
        if (Mono != null)
        {
            var controller = (DeepDrillerController)Mono;
            controller.CanBeSeenByTransceiver = !controller.CanBeSeenByTransceiver;
            var manager = HabitatService.main.GetHabitatByID(BaseID);
            manager.RegisterDevice(controller);
            controller.Manager = manager;
            UpdateLabel();
        }
    }

    internal static bool IsValid(string[] paraResults, out HabitatManager baseManager)
    {
        baseManager = null;
        try
        {
            if (paraResults.Length != 1)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Debug($"Incorrect amount of parameters expected 1 got {paraResults.Length}", true);
                QuickLogger.Message(AuxPatchers.IncorrectParameterFormat("STRING", "OS.ConnectToBase(BS000);"), true);
                return false;
            }


            baseManager = HabitatService.main.GetHabitatByID(Convert.ToInt32(paraResults[0]));

            if (baseManager == null)
            {
                QuickLogger.Message(AuxPatchers.BaseIDErrorFormat(paraResults[0]), true);
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

    public int BaseID { get; set; }

    public override string Format()
    {
        return $"{FriendlyName}: {BaseID}";
    }
}