using System;
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
                LoadAssetV2(prefabName, AssetHelper.Asset(Mod.BundleName), out GameObject go);
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
            QuickLogger.Debug("Loading Asset");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            QuickLogger.Debug($"Loaded Prefab {prefabName}");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    AlterraHub.ReplaceShadersV2(prefab, AlterraHub.BasePrimaryCol);
                    AlterraHub.ReplaceShadersV2(prefab, AlterraHub.BaseSecondaryCol);
                    AlterraHub.ReplaceShadersV2(prefab, AlterraHub.BaseDecalsExterior);
                    AlterraHub.ReplaceShadersV2(prefab, AlterraHub.BaseTexDecals);
                    AlterraHub.ReplaceShadersV2(prefab, AlterraHub.BaseLightsEmissiveController);
                    AlterraHub.ReplaceShadersV2(prefab, AlterraHub.BaseDecalsEmissiveController);
                    QuickLogger.Debug($"Applied shaderes to prefab {prefabName}");
                }

                go = prefab;
                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");

            go = null;
            return false;
        }

    }
}
