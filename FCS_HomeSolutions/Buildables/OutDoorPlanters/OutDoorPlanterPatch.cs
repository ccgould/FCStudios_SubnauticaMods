using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mono.OutDoorPlanters;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Buildables.OutDoorPlanters
{
    internal class OutDoorPlanterPatch : Buildable
    {
        private GameObject _prefab;
        private Settings _settings;
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();
        public OutDoorPlanterPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings) : base(classId, friendlyName, description)
        {
            _settings = settings;
            _prefab = prefab;
            OnStartedPatching += () =>
            {
                QuickLogger.Debug("Patched Kit");
                var outDoorPlanterkit = new FCSKit(_settings.KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                outDoorPlanterkit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.SmartPlanterPotKitClassID.ToTechType(), 21000, StoreCategory.Home);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                //Disable the object so we can fill in the properties before awake
                prefab.SetActive(false);
                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        material.shader = shader;
                    }
                }

                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = true;
                constructable.allowedInBase = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = true;
                constructable.model = model;
                constructable.techType = TechType;

                var sRoot = GameObjectHelpers.FindGameObject(prefab, "StorageRoot");
                var slot1 = GameObjectHelpers.FindGameObject(prefab, "slot1");
                var slot2 = GameObjectHelpers.FindGameObject(prefab, "slot2");
                var slot3 = GameObjectHelpers.FindGameObject(prefab, "slot3");
                var slot4 = GameObjectHelpers.FindGameObject(prefab, "slot4");
                var slot_big = GameObjectHelpers.FindGameObject(prefab, "slot_big");
                var sRootComp = sRoot.AddComponent<ChildObjectIdentifier>();

                var gPlant = GameObjectHelpers.FindGameObject(prefab, "grownPlant");
                gPlant.AddComponent<ChildObjectIdentifier>();

                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                
                var storage = prefab.AddComponent<StorageContainer>();
                storage.storageRoot = sRootComp;
                storage.storageLabel = FriendlyName;
                storage.height = 2;
                storage.width = 2;

                var planter = prefab.AddComponent<Planter>();
                planter.slots = new []
                {
                    slot1.transform,
                    slot2.transform,
                    slot3.transform,
                    slot4.transform,
                };

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                planter.bigSlots = new []{ slot_big.transform };
                planter.grownPlantsRoot = gPlant.transform;
                planter.storageContainer = storage;
                planter.environment = Planter.PlantEnvironment.Dynamic;
                planter.constructable = constructable;
                planter.isIndoor = true;
                prefab.SetActive(true);
                prefab.AddComponent<OutDoorPlanterController>();
                
                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                //MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.SmartPlanterIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
