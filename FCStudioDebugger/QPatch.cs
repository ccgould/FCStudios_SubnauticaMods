using FCSCommon.Utilities;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace FCStudioDebugger
{
    public static class QPatch
    {
        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                var harmony = HarmonyInstance.Create("com.fcsdebugger.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                GetGlassShader();

                QuickLogger.Info("Finished patching");

            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void GetGlassShader()
        {
            //var ResourcePath = "Submarine/Build/WaterPark";

            var gameObject = CraftData.GetPrefabForTechType(TechType.DepletedReactorRod);

            if (gameObject == null)
            {
                QuickLogger.Error("Depleted Reactor Rod is null");
                return;
            }

            var renderer = gameObject.GetComponentsInChildren<Renderer>();
            //renderer.material.shader = marmosetUber;

            if (renderer == null)
            {
                QuickLogger.Error($"Render is null canceling operation");
                return;
            }

            QuickLogger.Info($"Render count {renderer.Length}");


            foreach (var render in renderer)
            {
                QuickLogger.Info($"Material Name = {render.material.name}");
                if (render.material.name.StartsWith("depleted_nuclear_reactor_rod_glass"))
                {
                    QuickLogger.Info($"Material Shader Name = {render.material}");

                    var keywords = render.material.shaderKeywords;


                    render.material.DisableKeyword("MARMO_SIMPLE_GLASS");

                    foreach (string keyword in keywords)
                    {
                        QuickLogger.Info($"Shader Keyword {keyword}");
                    }
                }
            }
        }
    }
}
