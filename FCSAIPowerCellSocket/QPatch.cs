
using FCSAIPowerCellSocket.Buildables;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.Reflection;
using FCSAIPowerCellSocket.Configuration;
using FlexibleTechFabricator;
using FlexibleTechFabricator.Craftables;
using QModManager.API.ModLoading;

namespace FCSAIPowerCellSocket
{
    [QModCore]
    public class QPatch
    {
        private const string PowerStorageClassId = "FCSPowerStorage";

        [QModPatch]
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                AIPowerCellSocketBuildable.PatchSMLHelper();

                var craftable1 = new FCSKit(PowerStorageClassId,Mod.ModFriendlyName,new );
                craftable1.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
           

                //PatchHelpers.AddNewKit(
                //    FCSTechFabricator.Configuration.PowerCellSocketKitClassID,
                //    null,
                //    Mod.ModName,
                //    FCSTechFabricator.Configuration.PowerCellSocketClassID,
                //    new[] { "AIS", "PSS" },
                //    null);



                var harmony = HarmonyInstance.Create("com.fcsaipowercellsocket.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }
}
