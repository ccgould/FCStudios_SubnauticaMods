using FCSCommon.Exceptions;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Configuration;
using FCSTechFabricator.Models;
using FCSTechFabricator.Mono;
using FCSTechFabricator.Mono.DeepDriller;
using FCSTechFabricator.Mono.MarineTurbine;
using FCSTechFabricator.Mono.PowerStorage;
using FCSTechFabricator.Mono.SeaBreeze;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FCSTechFabricator
{
    public class QPatch
    {
        internal static Configuration.Configuration Configuration { get; set; }

        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                // == Load Configuration == //
                string configJson = File.ReadAllText(Information.ConfigurationFile().Trim());

                //LoadData
                Configuration = JsonConvert.DeserializeObject<Configuration.Configuration>(configJson);


                if (GetPrefabs())
                {
                    FCSTechFabricatorBuildable.PatchHelper();
                    RegisterItems();
                    QuickLogger.Debug(FCSTechFabricatorBuildable.TechFabricatorCraftTreeType.ToString());

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
            freon.Patch();
            FCSTechFabricatorBuildable.AddTechType(freon.TechType, freon.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {freon.FriendlyName}");

            var psKit = new PowerStorageKitBuildable();
            psKit.Patch();
            FCSTechFabricatorBuildable.AddTechType(psKit.TechType, psKit.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {psKit.FriendlyName}");


            var ddKit = new DeepDrillerKitBuildable();
            ddKit.Patch();
            FCSTechFabricatorBuildable.AddTechType(ddKit.TechType, ddKit.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {ddKit.FriendlyName}");

            var ddBatteryModule = new BatteryAttachmentBuildable();
            ddBatteryModule.Patch();
            FCSTechFabricatorBuildable.AddTechType(ddBatteryModule.TechType, ddBatteryModule.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {ddBatteryModule.FriendlyName}");


            var jsKit = new JetStreamKitBuildable();
            jsKit.Patch();
            FCSTechFabricatorBuildable.AddTechType(jsKit.TechType, jsKit.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {jsKit.FriendlyName}");

            var mmKit = new MarineMonitorKitBuildable();
            mmKit.Patch();
            FCSTechFabricatorBuildable.AddTechType(mmKit.TechType, mmKit.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {mmKit.FriendlyName}");


            var ddSolarModule = new SolarAttachmentBuildable();
            ddSolarModule.Patch();
            FCSTechFabricatorBuildable.AddTechType(ddSolarModule.TechType, ddSolarModule.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {ddSolarModule.FriendlyName}");


            var ddFocusModule = new FocusAttachmentBuildable();
            ddFocusModule.Patch();
            FCSTechFabricatorBuildable.AddTechType(ddFocusModule.TechType, ddFocusModule.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {ddFocusModule.FriendlyName}");


            var sbKit = new SeaBreezeKitBuildable();
            sbKit.Patch();
            FCSTechFabricatorBuildable.AddTechType(sbKit.TechType, sbKit.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {sbKit.FriendlyName}");

            var exKit = new ExStorageKitBuildable();
            exKit.Patch();
            FCSTechFabricatorBuildable.AddTechType(exKit.TechType, exKit.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {exKit.FriendlyName}");

            var ddMk1 = new DrillerMK1Craftable();
            ddMk1.Patch();
            FCSTechFabricatorBuildable.AddTechType(ddMk1.TechType, ddMk1.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {ddMk1.FriendlyName}");

            var ddMk2 = new DrillerMK2Craftable();
            ddMk2.Patch();
            FCSTechFabricatorBuildable.AddTechType(ddMk2.TechType, ddMk2.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {ddMk2.FriendlyName}");

            var ddMk3 = new DrillerMK3Craftable();
            ddMk3.Patch();
            FCSTechFabricatorBuildable.AddTechType(ddMk3.TechType, ddMk3.StepsToFabricatorTab);
            QuickLogger.Debug($"Patched {ddMk3.FriendlyName}");

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
                RegisterKit();
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

        private static void RegisterKit()
        {
            var model = Kit.GetComponentInChildren<Canvas>().gameObject;
            model.FindChild("Screen").SetActive(true);

            var rb = Kit.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Set collider
            var collider = Kit.GetComponent<BoxCollider>();
            collider.enabled = false;

            // Make the object drop slowly in water
            var wf = Kit.GetOrAddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

            // Add fabricating animation
            var fabricatingA = Kit.GetOrAddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            //// Set proper shaders (for crafting animation)
            //Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = Kit.GetComponentInChildren<Renderer>();
            //renderer.material.shader = marmosetUber;

            // Update sky applier
            var applier = Kit.GetOrAddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = Kit.GetOrAddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            var placeTool = Kit.GetOrAddComponent<PlaceTool>();
            placeTool.allowedInBase = true;
            placeTool.allowedOnBase = false;
            placeTool.allowedOnCeiling = false;
            placeTool.allowedOnConstructable = true;
            placeTool.allowedOnGround = true;
            placeTool.allowedOnRigidBody = true;
            placeTool.allowedOnWalls = false;
            placeTool.allowedOutside = false;
            placeTool.rotationEnabled = true;
            placeTool.enabled = true;
            placeTool.hasAnimations = false;
            placeTool.hasBashAnimation = false;
            placeTool.hasFirstUseAnimation = false;
            placeTool.mainCollider = collider;
            placeTool.pickupable = pickupable;
            placeTool.drawTime = 0.5f;
            placeTool.dropTime = 1;
            placeTool.holsterTime = 0.35f;

            Kit.AddComponent<FCSTechFabricatorTag>();

            //Lets apply the material shader
            Shaders.ApplyKitShaders(Kit);
        }

        public static GameObject BatteryModule { get; set; }

        public static GameObject FocusModule { get; private set; }

        public static GameObject SolarModule { get; private set; }

        public static GameObject Kit { get; private set; }

        public static AssetBundle Bundle { get; private set; }

    }
}
