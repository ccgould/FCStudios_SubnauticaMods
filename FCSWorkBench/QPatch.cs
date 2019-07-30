using FCSCommon.Exceptions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using FCSTechFabricator.Mono;
using FCSTechFabricator.Mono.DeepDriller;
using FCSTechFabricator.Mono.MarineTurbine;
using FCSTechFabricator.Mono.PowerStorage;
using FCSTechFabricator.Mono.SeaBreeze;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FCSTechFabricator
{
    public class QPatch
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
                if (GetPrefabs())
                {
                    RegisterItems();

                    FCSTechFabricatorBuildable.PatchHelper();

                    var harmony = HarmonyInstance.Create("com.fcstechfabricator.fcstudios");

                    harmony.PatchAll(Assembly.GetExecutingAssembly());

                    QuickLogger.Info("Finished patching");
                }
                else
                {
                    throw new PatchTerminatedException();
                }


            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void RegisterItems()
        {
            var freon = new FreonBuildable();
            freon.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(freon);

            var psKit = new PowerStorageKitBuildable();
            psKit.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(psKit);

            var ddKit = new DeepDrillerKitBuildable();
            ddKit.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(ddKit);

            var ddBatteryModule = new BatteryAttachmentBuildable();
            ddBatteryModule.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(ddBatteryModule);

            var jsKit = new JetStreamKitBuildable();
            jsKit.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(jsKit);

            var ddSolarModule = new SolarAttachmentBuildable();
            ddSolarModule.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(ddSolarModule);

            var ddFocusModule = new FocusAttachmentBuildable();
            ddFocusModule.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(ddFocusModule);

            var sbKit = new SeaBreezeKitBuildable();
            sbKit.Register();
            FCSTechFabricatorBuildable.ItemsList.Add(sbKit);
        }

        public static bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");

            AssetBundle assetBundle = AssetHelper.Asset("FCSTechFabricator", "fcstechfabricatormodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                throw new FileLoadException();
            }

            Bundle = assetBundle;


            //We have found the asset bundle and now we are going to continue by looking for the model.
            Kit = Bundle.LoadAsset<GameObject>("UnitContainerKit");

            //If the prefab isn't null lets add the shader to the materials
            if (Kit != null)
            {
                QuickLogger.Debug($"UnitContainerKit Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"UnitContainerKit Prefab Not Found!");
                return false;
            }


            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject batteryModule = Bundle.LoadAsset<GameObject>("Battery_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (batteryModule != null)
            {
                Shaders.ApplyDeepDriller(batteryModule);

                BatteryModule = batteryModule;

                QuickLogger.Debug($"Battery Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Battery Module  Prefab Not Found!");
                return false;
            }

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject solarModule = Bundle.LoadAsset<GameObject>("Solar_Panel_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (solarModule != null)
            {
                Shaders.ApplyDeepDriller(solarModule);

                SolarModule = solarModule;

                QuickLogger.Debug($"Solar Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Solar Module  Prefab Not Found!");
                return false;
            }

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject focusModule = Bundle.LoadAsset<GameObject>("Scanner_Screen_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (focusModule != null)
            {
                Shaders.ApplyDeepDriller(focusModule);

                FocusModule = focusModule;

                QuickLogger.Debug($"Focus Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Solar Module  Prefab Not Found!");
                return false;
            }

            return true;
        }

        public static GameObject BatteryModule { get; set; }

        public static GameObject FocusModule { get; private set; }

        public static GameObject SolarModule { get; private set; }

        public static GameObject Kit { get; private set; }

        public static AssetBundle Bundle { get; private set; }

    }
}
