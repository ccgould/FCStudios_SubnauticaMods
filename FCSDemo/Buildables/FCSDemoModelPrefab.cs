using System;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using UnityEngine;

namespace FCSDemo.Buildables
{
    internal static class FCSDemoModel
    {
        public static GameObject GetPrefabs(string prefabName)
        {
            try
            {
                QuickLogger.Info($"Loading Prefab : {prefabName}");
                LoadAssetV2(prefabName, AssetHelper.Asset(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),Mod.BundleName), out GameObject go);
                return go;
            }
            catch (Exception e)
            {
                QuickLogger.Error<FCSDemoBuidable>(e.Message);
                return null;
            }
        }

        private static bool LoadAssetV2(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
        {
            QuickLogger.Info("Loading Asset");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);


            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                QuickLogger.Info($"Loaded Prefab {prefabName}");

                if (applyShaders)
                {
                    //Lets apply the material shader
                    //AlterraHub.ReplaceShadersV2(prefab);
                    QuickLogger.Info($"Applied shaderes to prefab {prefabName}");
                }

                go = prefab;
                QuickLogger.Info($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");

            go = null;
            return false;
        }

    }
}
