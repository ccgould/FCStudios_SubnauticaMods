using System;
using System.Reflection;
using FCSCommon.Utilities;
using FCSDemo.Buildables;
using FCSDemo.Configuration;
using FCSDemo.Model;
using HarmonyLib;
using SMLHelper.Handlers;

namespace FCSDemo
{

    public class Main
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

  
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
                else
                {
                    QuickLogger.Error($"Failed to load Configuration.");
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
