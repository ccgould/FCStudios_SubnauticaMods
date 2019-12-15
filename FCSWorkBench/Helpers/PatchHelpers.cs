using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using FCSTechFabricator.Mono;
using FCSTechFabricator.Mono.SeaBreeze;
using FCSTechFabricator.Mono.SeaCooker;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator.Helpers
{
    public static class PatchHelpers
    {
        /// <summary>
        /// Applied settings for the FCSKit before its instantiated. 
        /// </summary>
        private static void RegisterKit()
        {
            //Find the screen on the model
            var model = QPatch.Kit.GetComponentInChildren<Canvas>().gameObject;
            model.FindChild("Screen").SetActive(true);

            //Make the rigid body isKinematic so it doesnt shake the cyclops
            var rb = QPatch.Kit.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Set collider
            var collider = QPatch.Kit.GetComponent<BoxCollider>();
            collider.enabled = false;

            // Make the object drop slowly in water
            var wf = QPatch.Kit.GetOrAddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

            // Add fabricating animation
            var fabricatingA = QPatch.Kit.GetOrAddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            //// Set proper shaders (for crafting animation)
            //Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = QPatch.Kit.GetComponentInChildren<Renderer>();
            //renderer.material.shader = marmosetUber;

            // Update sky applier
            var applier = QPatch.Kit.GetOrAddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = QPatch.Kit.GetOrAddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            //Allow this kit to be placed on surfaces in these situations
            var placeTool = QPatch.Kit.GetOrAddComponent<PlaceTool>();
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

            //Add the FCSTechFabricatorTag component
            QPatch.Kit.AddComponent<FCSTechFabricatorTag>();

            //Lets apply the material shader
            Shaders.ApplyKitShaders(QPatch.Kit);
        }
        
        private static List<FCSKitEntry> CreateKits()
        {
            var list = new List<FCSKitEntry>
            {
                new FCSKitEntry
                {
                    ClassID = "PowerStorageKit_PS",
                    FriendlyName = "Power Storage Kit",
                    Description = "A kit that allows you to build one Power Storage Unit",
                    FabricatorSteps = new[]{ "AIS", "PS" },
                    ModParent = Configuration.PowerStorageKitClassID
                },
                new FCSKitEntry
                {
                    ClassID = "DeepDrillerKit_DD",
                    FriendlyName = "Deep Driller Kit",
                    Description = "This kit allows you to make one Deep Driller unit",
                    FabricatorSteps = new[]{ "AIS", "DD" },
                    ModParent = Configuration.DeepDrillerClassID
                },
                new FCSKitEntry
                {
                    ClassID = "SeaCookerBuildableKit_SC",
                    FriendlyName = "Sea Cooker Unit Kit",
                    Description = "This kit allows you to make one Sea Cooker unit",
                    FabricatorSteps = new[]{ "AE", "SC" },
                    ModParent = Configuration.SeaCookerClassID
                },
                new FCSKitEntry
                {
                    ClassID = "JetStreamT242Kit_MT",
                    FriendlyName = "Jet Stream T242 Kit",
                    Description = "A kit that allows you to build one Jet Stream T242 Unit",
                    FabricatorSteps = new[]{ "AIS", "MT" },
                    ModParent = Configuration.AIJetStreamT242ClassID
                },
                new FCSKitEntry
                {
                    ClassID = "MarineMonitorKit_MT",
                    FriendlyName = "Marine Monitor Kit",
                    Description = "A kit that allows you to build one Marine Monitor",
                    FabricatorSteps = new[]{ "AIS", "MT" },
                    ModParent = Configuration.AIMarineMonitorClassID
                },
                new FCSKitEntry
                {
                    ClassID = "MiniFountainFilterKit_MFF",
                    FriendlyName = "Mini Fountain Filter Unit Kit",
                    Description = "This kit allows you to make one Mini Fountain Filter unit",
                    FabricatorSteps = new[]{ "AE", "MFF" },
                    ModParent = Configuration.MiniFountainFilterClassID
                },
                new FCSKitEntry
                {
                    ClassID = "ExStorageKit_ASTS",
                    FriendlyName = "Ex-Storage Unit Kit",
                    Description = "This kit allows you to make one ex-storage unit",
                    FabricatorSteps = new[]{ "ASTS", "ES" },
                    ModParent = Configuration.ExStorageClassID
                },
                new FCSKitEntry
                {
                    ClassID = "SeaBreezeKit_SB",
                    FriendlyName = "Sea Breeze Kit",
                    Description = "A kit that allows you to build one Seabreeze refrigerator.",
                    FabricatorSteps = new[]{ "ARS", "SB" },
                    ModParent = Configuration.SeaBreezeClassID
                },
                new FCSKitEntry
                {
                    ClassID = "BatteryAttachment_DD",
                    FriendlyName = "Deep Driller Powercell Attachment",
                    Description = "This specially made attachment allows you to run your deep driller off powercell power",
                    FabricatorSteps = new[]{ "AIS", "DD" },
                    ModParent = Configuration.DeepDrillerClassID,
                    Icon="BatteryAttachment_DD.png"
                },
                new FCSKitEntry
                {
                    ClassID = "SolarAttachment_DD",
                    FriendlyName = "Deep Driller Solar Attachment",
                    Description = "This specially made attachment allows you to run your deep driller off solar power.",
                    FabricatorSteps = new[]{ "AIS", "DD" },
                    ModParent = Configuration.DeepDrillerClassID,
                    Icon="SolarAttachment_DD.png"
                },
                new FCSKitEntry
                {
                    ClassID = "FocusAttachment_DD",
                    FriendlyName = "Deep Driller Focus Attachment",
                    Description = "This specially made attachment allows you to scan for one specific ore.",
                    FabricatorSteps = new[]{ "AIS", "DD" },
                    ModParent = Configuration.DeepDrillerClassID,
                    Icon="FocusAttachment_DD.png"
                },
                new FCSKitEntry
                {
                    ClassID = "AMMiniMedBayKit_AMS",
                    FriendlyName = "Alterra Medical MiniMedBay Kit",
                    Description = "This kit allows you to make one MiniMedBay unit",
                    FabricatorSteps = new[]{ "AMS", "MMB" },
                    ModParent = Configuration.MiniMedBayClassID
                },
                new FCSKitEntry
                {
                    ClassID = "PowerCellSocket_AIS",
                    FriendlyName = "Alterra Industrial Powercell Socket Kit",
                    Description = "This kit allows you to make one Powercell Socket unit",
                    FabricatorSteps = new[]{ "AIS", "PSS" },
                    ModParent = Configuration.PowerCellSocketClassID
                },
                new FCSKitEntry
                {
                    ClassID = "IntraBaseTeleporterKit_AE",
                    FriendlyName = "Intra-Base Teleporter Kit",
                    Description = "This kit allows you to make one Intra-Base Teleporter unit",
                    FabricatorSteps = new[]{ "AE", "IBT" },
                    ModParent = Configuration.QuantumTeleporterClassID
                },
            };

            return list;
        }

        private static void CreateModEntry()
        {
            //FCSTechFabricatorBuildable.AddMod(ModDivisions.AlterraMedicalSolutions.Key,ModDivisions.AlterraMedicalSolutions.ParentKey,"Mini MedBay","MMB");
        }

        private static List<FCSKitEntry> CreateModules()
        {
            var list = new List<FCSKitEntry>
            {
                new FCSKitEntry
                {
                    ClassID = "DrillerMK1_DD",
                    FriendlyName = "Deep Driller MK 1",
                    Description = "This upgrade allows deep driller to drill 15 resources per day.",
                    FabricatorSteps = new[] { "AIS", "DD" },
                    ModParent = Configuration.DeepDrillerClassID
                },
                new FCSKitEntry
                {
                    ClassID = "DrillerMK2_DD",
                    FriendlyName = "Deep Driller MK 2",
                    Description = "This upgrade allows deep driller to drill 22 resources per day.",
                    FabricatorSteps = new[] { "AIS", "DD" },
                    ModParent = Configuration.DeepDrillerClassID
                },
                new FCSKitEntry
                {
                    ClassID = "DrillerMK3_DD",
                    FriendlyName = "Deep Driller MK 3",
                    Description = "This upgrade allows deep driller to drill 30 resources per day.",
                    FabricatorSteps = new[] { "AIS", "DD" },
                    ModParent = Configuration.DeepDrillerClassID
                },
            };

            return list;
        }

        /// <summary>
        /// Gets the necessary prefabs from the global bundle for use in all mods.
        /// </summary>
        /// <returns></returns>
        internal static bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");

            #region Asset Bundle
            AssetBundle assetBundle = AssetHelper.Asset(Mod.ModFolderName, "fcstechfabricatormodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                throw new FileLoadException();
            }

            QPatch.Bundle = assetBundle;
            #endregion

            #region Color Item
            //We have found the asset bundle and now we are going to continue by looking for the model.
            QPatch.ColorItem = QPatch.Bundle.LoadAsset<GameObject>("ColorItem");

            //If the prefab isn't null lets add the shader to the materials
            if (QPatch.ColorItem != null)
            {
                QuickLogger.Debug($"ColorItem Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"UnitContainerKit Prefab Not Found!");
                return false;
            }
            #endregion

            #region Kit
            //We have found the asset bundle and now we are going to continue by looking for the model.
            QPatch.Kit = QPatch.Bundle.LoadAsset<GameObject>("UnitContainerKit");

            //If the prefab isn't null lets add the shader to the materials
            if (QPatch.Kit != null)
            {
                QuickLogger.Debug($"UnitContainerKit Prefab Found!");
                RegisterKit();
            }
            else
            {
                QuickLogger.Error($"UnitContainerKit Prefab Not Found!");
                return false;
            }
            #endregion

            #region Battery Module
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject batteryModule = QPatch.Bundle.LoadAsset<GameObject>("Battery_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (batteryModule != null)
            {
                Shaders.ApplyDeepDriller(batteryModule);

                QPatch.BatteryModule = batteryModule;

                QuickLogger.Debug($"Battery Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Battery Module  Prefab Not Found!");
                return false;
            }
            #endregion

            #region Solar Module
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject solarModule = QPatch.Bundle.LoadAsset<GameObject>("Solar_Panel_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (solarModule != null)
            {
                Shaders.ApplyDeepDriller(solarModule);

                QPatch.SolarModule = solarModule;

                QuickLogger.Debug($"Solar Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Solar Module  Prefab Not Found!");
                return false;
            }
            #endregion

            #region Focus Module
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject focusModule = QPatch.Bundle.LoadAsset<GameObject>("Scanner_Screen_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (focusModule != null)
            {
                Shaders.ApplyDeepDriller(focusModule);

                QPatch.FocusModule = focusModule;

                QuickLogger.Debug($"Focus Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Solar Module  Prefab Not Found!");
                return false;
            }
            #endregion

            #region SeaGas Tank
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject seaGasTank = QPatch.Bundle.LoadAsset<GameObject>("SeaGasTank");

            //If the prefab isn't null lets add the shader to the materials
            if (seaGasTank != null)
            {
                Shaders.ApplySeaTankShaders(seaGasTank);

                QPatch.SeaGasTank = seaGasTank;

                QuickLogger.Debug($"Sea Gas Tank Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Sea Gas Tank  Prefab Not Found!");
                return false;
            }
            #endregion

            #region SeaAlien Tank
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject seaAlienGasTank = QPatch.Bundle.LoadAsset<GameObject>("SeaAlienGasTank");

            //If the prefab isn't null lets add the shader to the materials
            if (seaAlienGasTank != null)
            {
                Shaders.ApplySeaTankShaders(seaAlienGasTank);

                QPatch.SeaAlienGasTank = seaAlienGasTank;

                QuickLogger.Debug($"Sea Alien Gas Tank Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Sea Alien Gas Tank  Prefab Not Found!");
                return false;
            }
            #endregion

            #region Freon
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject freon = QPatch.Bundle.LoadAsset<GameObject>("Freon_Bottle");

            //If the prefab isn't null lets add the shader to the materials
            if (freon != null)
            {
                Shaders.ApplyFreonShaders(freon);

                QPatch.Freon = freon;

                QuickLogger.Debug($"Freon Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Freon Prefab Not Found!");
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Registers all FCSTechFab items into the craft tree
        /// </summary>
        internal static void RegisterItems()
        {
            //if (TechTypeHandler.ModdedTechTypeExists()
            //var sandGlass = new SaNDGlass();
            //sandGlass.Patch();
            //FCSTechFabricatorBuildable.AddTechType(sandGlass.TechType, sandGlass.StepsToFabricatorTab);
            //QuickLogger.Debug($"Patched {sandGlass.FriendlyName}");

            QuickLogger.Debug("Registering Items.");

            CreateModEntry();

            //foreach (FCSKitEntry kit in CreateKits())
            //{


            //    QuickLogger.Debug($"Registering {kit.FriendlyName}");

            //    var kitCraftable = new KitCraftable(kit.ClassID, kit.FriendlyName, kit.Description, kit.Icon, kit.FabricatorSteps);
            //    kitCraftable.Patch();
            //    if (!TechTypeHandler.ModdedTechTypeExists(kit.ModParent)) continue;
            //    FCSTechFabricatorBuildable.AddTechType(kitCraftable.TechType, kit.FabricatorSteps);

            //    QuickLogger.Debug($"Patched {kitCraftable.FriendlyName}");
            //}

            //foreach (FCSKitEntry module in CreateModules())
            //{
            //    if (!TechTypeHandler.ModdedTechTypeExists(module.ModParent)) continue;

            //    QuickLogger.Debug($"Registering {module.FriendlyName}");

            //    var moduleCraftable = new ModuleCraftable(module.ClassID, module.FriendlyName, module.Description, module.Icon);
            //    moduleCraftable.Patch();

            //    FCSTechFabricatorBuildable.AddTechType(moduleCraftable.TechType, module.FabricatorSteps);

            //    QuickLogger.Debug($"Patched {moduleCraftable.FriendlyName}");
            //}

            //if (TechTypeHandler.ModdedTechTypeExists(Configuration.SeaBreezeClassID))
            //{
            //    var freon = new FreonBuildable();
            //    freon.Patch();
            //    FCSTechFabricatorBuildable.AddTechType(freon.TechType, freon.StepsToFabricatorTab);
            //    QuickLogger.Debug($"Patched {freon.FriendlyName}");
            //}

            //if (TechTypeHandler.ModdedTechTypeExists(Configuration.SeaCookerClassID))
            //{
            //    var scGtank = new SeaGasTankCraftable();
            //    scGtank.Patch();
            //    FCSTechFabricatorBuildable.AddTechType(scGtank.TechType, scGtank.StepsToFabricatorTab);
            //    QuickLogger.Debug($"Patched {scGtank.FriendlyName}");

            //    var scAGtank = new SeaAlienGasTankCraftable();
            //    scAGtank.Patch();
            //    FCSTechFabricatorBuildable.AddTechType(scAGtank.TechType, scAGtank.StepsToFabricatorTab);
            //    QuickLogger.Debug($"Patched {scAGtank.FriendlyName}");
            //}
        }

        public static bool AddNewKit(string classID, string description, string unitName, string modParent, string[] fabricatorSteps, string icon, TechType requiredForUnlock = TechType.None, UnitType type = UnitType.Kit)
        {
            try
            {
                if (string.IsNullOrEmpty(description))
                {
                    description = $"A kit that allows you to build one {unitName} Unit";
                }

                string suffix = String.Empty;

                if (type == UnitType.Kit) suffix = "kit";


                var kit = new FCSKitEntry
                {
                    ClassID = classID,
                    FriendlyName = $"{unitName} {suffix}",
                    Description = description,
                    FabricatorSteps = fabricatorSteps,
                    ModParent = modParent,
                    RequiredForUnlock = requiredForUnlock,
                    Icon = icon
                };

                QuickLogger.Debug($"Registering {kit.FriendlyName}");
                var kitCraftable = new KitCraftable(kit.ClassID, kit.FriendlyName, kit.Description, kit.Icon, kit.FabricatorSteps);
                kitCraftable.Patch();
                FCSTechFabricatorBuildable.AddTechType(kitCraftable.TechType, kit.FabricatorSteps);
                QuickLogger.Debug($"Patched {kitCraftable.FriendlyName}");
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[AddKit() PatchHelpers]: {e.Message}");
                return false;
            }
        }

        public static bool AddNewModule(string classID, string description, string unitName, string modParent, string[] fabricatorSteps)
        {
            try
            {
                if (string.IsNullOrEmpty(description))
                {
                    description = $"A kit that allows you to build one {unitName} module";
                }

                var module = new FCSKitEntry
                {
                    ClassID = classID,
                    FriendlyName = $"{unitName} kit",
                    Description = description,
                    FabricatorSteps = fabricatorSteps,
                    ModParent = modParent
                };

                QuickLogger.Debug($"Registering {module.FriendlyName}");

                var moduleCraftable = new ModuleCraftable(module.ClassID, module.FriendlyName, module.Description, module.Icon);
                moduleCraftable.Patch();

                FCSTechFabricatorBuildable.AddTechType(moduleCraftable.TechType, module.FabricatorSteps);

                QuickLogger.Debug($"Patched {moduleCraftable.FriendlyName}");
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[AddKit() PatchHelpers]: {e.Message}");
                return false;
            }
        }

        public static void AddTechType(TechType techType, string[] stepsToFabricatorTab)
        {
            FCSTechFabricatorBuildable.AddTechType(techType,stepsToFabricatorTab);
        }
    }

    public enum UnitType
    {
        Kit,
        Unit
    }
}

