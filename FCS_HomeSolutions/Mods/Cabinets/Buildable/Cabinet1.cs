using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Cabinets.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.Cabinets.Buildable
{
    internal class Cabinet1Buildable : SMLHelper.V2.Assets.Buildable
    {
        private GameObject _locker;
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        
        public Cabinet1Buildable() : base(Mod.Cabinet1ClassID, Mod.Cabinet1Friendly, Mod.Cabinet1Description)
        {
            _locker = Resources.Load<GameObject>("Submarine/Build/Locker");

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(Mod.Cabinet1KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.Cabinet1KitClassID.ToTechType(), 6000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.Cabinet1Prefab);
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

                var center = new Vector3(-0.1788177f, 0.5337045f, 0f);
                var size = new Vector3(2.776398f, 0.757907f, 0.631052f);

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

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.Cabinet1KitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
