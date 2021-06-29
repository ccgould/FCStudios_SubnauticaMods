using System;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Mods.DeepDriller.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.Mono;
using FCSCommon.Utilities;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Models.Upgrades
{
    internal class ConnectToBaseUpgrade : UpgradeFunction
    {
        public override float PowerUsage => 0.1f;
        public override float Damage { get; }
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.ConnectToBase;
        public override string FriendlyName => "Connect To Base";

        public override string GetFunction()
        {
            return $"os.ConnectToBase({BaseName});";
        }

        public override void ActivateUpdate()
        {
            if (Mono != null)
            {
                var controller = ((FCSDeepDrillerController)Mono);
                controller.CanBeSeenByTransceiver = true;
                var manager = BaseManager.FindManagerByFriendlyID(BaseName);
                manager.RegisterDevice(controller);
                controller.Manager = manager;
                UpdateLabel();
            }
        }

        public override void DeActivateUpdate()
        {
            if (Mono != null)
            {
                var controller = ((FCSDeepDrillerController)Mono);
                controller.CanBeSeenByTransceiver = false;
                var manager = BaseManager.FindManagerByFriendlyID(BaseName);
                manager.UnRegisterDevice(controller);
                controller.Manager = null;
                UpdateLabel();
            }
        }

        public override void TriggerUpdate()
        {
            if (Mono != null)
            {
                var controller = ((FCSDeepDrillerController)Mono);
                controller.CanBeSeenByTransceiver = !controller.CanBeSeenByTransceiver;
                var manager = BaseManager.FindManagerByFriendlyID(BaseName);
                manager.RegisterDevice(controller);
                controller.Manager = manager;
                UpdateLabel();
            }
        }

        internal static bool IsValid(string[] paraResults, out BaseManager baseManager)
        {
            baseManager = null;
            try
            {
                if (paraResults.Length != 1)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 1 got {paraResults.Length}", true);
                    QuickLogger.Message(FCSDeepDrillerBuildable.IncorrectParameterFormat("STRING", "OS.ConnectToBase(BS000);"), true);
                    return false;
                }

                baseManager = BaseManager.FindManagerByFriendlyID(paraResults[0]);

                if (baseManager == null)
                {
                    QuickLogger.Message(FCSDeepDrillerBuildable.BaseIDErrorFormat(paraResults[0]), true);
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

        public string BaseName { get; set; }

        public override string Format()
        {
            return $"{FriendlyName}: {BaseName}";
        }
    }
}