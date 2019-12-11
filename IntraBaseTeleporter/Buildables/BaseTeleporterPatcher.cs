using System;
using System.Collections.Generic;
using System.IO;
using AE.IntraBaseTeleporter.Configuration;
using AE.IntraBaseTeleporter.Managers;
using AE.IntraBaseTeleporter.Mono;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace AE.IntraBaseTeleporter.Buildables
{
    internal partial class BaseTeleporterBuildable : Buildable
    {
        private static readonly BaseTeleporterBuildable Singleton = new BaseTeleporterBuildable();
        private readonly GameObject CubePrefab = CraftData.GetPrefabForTechType(TechType.PrecursorIonCrystal);
        public BaseTeleporterBuildable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab;
            try
            {
                prefab = UnityEngine.Object.Instantiate(_prefab);
                
                prefab.name = this.PrefabFileName;

                if (prefab != null)
                {
                    var model = prefab.FindChild("model");

                    SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                    skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                    skyApplier.anchorSky = Skies.Auto;

                    //========== Allows the building animation and material colors ==========// 

                    QuickLogger.Debug("Adding Constructible");

                    var center = new Vector3(0f, 1.205158f, -0.2082438f);
                    var size = new Vector3(2.100852f, 2.276049f, 2.51642f);

                    GameObjectHelpers.AddConstructableBounds(prefab, size,center);
                    
                    // Add constructible
                    var constructable = prefab.AddComponent<Constructable>();
                    constructable.allowedOnWall = false;
                    constructable.allowedOnGround = true;
                    constructable.allowedInSub = false;
                    constructable.allowedInBase = true;
                    constructable.allowedOnCeiling = false;
                    constructable.allowedOutside = false;
                    constructable.model = model;
                    constructable.rotationEnabled = true;
                    constructable.techType = TechType;

                    PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                    prefabID.ClassId = ClassID;

                    CreateDisplayedIonCube(prefab);

                    prefab.AddComponent<AnimationManager>();
                    prefab.AddComponent<TeleportManager>();
                    prefab.AddComponent<BTDisplayManager>();
                    prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                    prefab.AddComponent<BaseTeleporterController>();
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
            return prefab;
        }

        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechTypeHelpers.GetTechType("IntraBaseTeleporterKit_AE"), 1)
                }
            };
            return customFabRecipe;
        }

        public override string IconFileName => $"{Mod.ModName}.png";

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        internal static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Register();

            PatchHelpers.AddNewKit(
                FCSTechFabricator.Configuration.QuantumTeleporterKitClassID,
                null,
                Mod.FriendlyName,
                FCSTechFabricator.Configuration.QuantumTeleporterClassID,
                new[] { "ASTS", "ES" },
                null);

            Singleton.Patch();
        }

        private void CreateDisplayedIonCube(GameObject prefab)
        {
            GameObject ionSlot = prefab.FindChild("model")
                .FindChild("ion_cube_placeholder")?.gameObject;

            if (ionSlot != null)
            {
                QuickLogger.Debug("Ion Cube Display Object Created", true);
                var displayedIonCube = GameObject.Instantiate<GameObject>(CubePrefab);
                Pickupable pickupable = displayedIonCube.GetComponent<Pickupable>();
                pickupable.isPickupable = false;
                pickupable.destroyOnDeath = true;

                displayedIonCube.transform.SetParent(ionSlot.transform);
                displayedIonCube.transform.localPosition = new Vector3(0f, 0.0f, 0f);
                displayedIonCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                //displayedIonCube.transform.Rotate(new Vector3(0, 0, 90));
            }
            else
            {
                QuickLogger.Error("Cannot Find IonCube in the prefab");
            }
        }


        private static void Register()
        {
            
        }
    }
}
