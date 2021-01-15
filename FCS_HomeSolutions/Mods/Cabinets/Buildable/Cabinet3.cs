using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Cabinets.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Cabinets.Buildable
{
    internal class Cabinet3Buildable : SMLHelper.V2.Assets.Buildable
    {
        private GameObject _locker;

        public Cabinet3Buildable() : base(Mod.Cabinet3ClassID, Mod.Cabinet3Friendly, Mod.Cabinet3Description)
        {
            _locker = Resources.Load<GameObject>("Submarine/Build/Locker");

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(Mod.Cabinet3KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.Cabinet3KitClassID.ToTechType(), 6000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.Cabinet3Prefab);
                GameObject container = GameObject.Instantiate(_locker);

                // Update container renderers
                GameObject cargoCrateModel = container.FindChild("model");
                Renderer[] cargoCrateRenderers = cargoCrateModel.GetComponentsInChildren<Renderer>();
                container.transform.parent = prefab.transform;
                foreach (Renderer rend in cargoCrateRenderers)
                {
                    rend.enabled = false;
                }
                container.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                container.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
                container.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                container.SetActive(true);


                // Update colliders
                GameObject builderTrigger = container.FindChild("Builder Trigger");
                GameObject collider = container.FindChild("Collider");
                BoxCollider builderCollider = builderTrigger.GetComponent<BoxCollider>();
                builderCollider.isTrigger = false;
                builderCollider.enabled = false;
                BoxCollider objectCollider = collider.GetComponent<BoxCollider>();
                objectCollider.isTrigger = false;
                objectCollider.enabled = false;

                // Delete constructable bounds
                ConstructableBounds cb = container.GetComponent<ConstructableBounds>();
                GameObject.DestroyImmediate(cb);


                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 1.43428f, 0f);
                var size = new Vector3(0.7484856f, 2.553802f, 0.6422229f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<CabinetController>();

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.Cabinet3KitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }
#elif BELOWZERO

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.Cabinet3KitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }
#endif

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
    }
}
