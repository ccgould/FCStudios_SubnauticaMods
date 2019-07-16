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
                Test();

                QuickLogger.Info("Finished patching");

            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void Test()
        {
            var ResourcePath = "Submarine/Build/Bioreactor";
            var h = Resources.Load<GameObject>(ResourcePath);
            if (h == null)
            {
                QuickLogger.Error("H is null");
                return;
            }

            var prefab = GameObject.Instantiate(h);
            if (prefab == null)
            {
                QuickLogger.Error("prefab is null");
                return;
            }

            var model = prefab.GetComponent<BaseBioReactor>();
            if (model == null)
            {
                QuickLogger.Error("Model is null");
            }

            QuickLogger.Info("IN TEST");

            QuickLogger.Info(prefab.name);

            QuickLogger.Info(prefab.transform.childCount.ToString());

            Renderer[] renderers = prefab.GetComponents<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                QuickLogger.Info("In for loop");
                //renderer.material.mainTextureOffset = Vector2.up;
                QuickLogger.Info(renderer.material.name);
            }
        }
    }
}
