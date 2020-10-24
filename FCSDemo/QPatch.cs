using System;
using System.Reflection;
using FCSCommon.Utilities;
using FCSDemo.Buildables;
using FCSDemo.Configuration;
using FCSDemo.Model;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace FCSDemo
{
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public static void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            try
            {
                var harmony = new Harmony("com.fcsdemo.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                if (Configuration.Prefabs != null)
                {
                    foreach (ModEntry modEntry in Configuration.Prefabs)
                    {
                        QuickLogger.Info($"Added Prefab {modEntry.ClassID}");
                        modEntry.Prefab = FCSDemoModel.GetPrefabs(modEntry.PrefabName);
                        var prefab = new FCSDemoBuidable(modEntry);
                        prefab.Patch();
                    }
                }

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }
}
