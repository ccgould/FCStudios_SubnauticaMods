using System;
using System.Collections;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif


namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable
{
    internal class DSSFloorServerRackPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        private TechType kitTechType;

        public DSSFloorServerRackPatch() : base(Mod.DSSFloorServerRackClassName, Mod.DSSFloorServerRackFriendlyName, Mod.DSSFloorServerRackDescription)
        {
            OnStartedPatching += () =>
            {
                var DSSFloorServerRackKit = new FCSKit(Mod.DSSFloorServerRackKitClassID, Mod.DSSFloorServerRackFriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                DSSFloorServerRackKit.Patch();
                kitTechType = DSSFloorServerRackKit.TechType;
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, kitTechType, 184078, StoreCategory.Storage);
            };
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            GameObject prefab  = null;
            try
            {
                prefab = GameObject.Instantiate(ModelPrefab.DSSFloorServerRackPrefab);

                var size = new Vector3(1.218514f, 1.895477f, 0.8515267f);
                var center = new Vector3(0f, 1.132018f, 0f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = false;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DSSFloorServerRackController>();

                prefab.SetActive(false);
                var storage = prefab.AddComponent<FCSStorage>();
                storage.Initialize(Mod.DSSFloorServerRackClassName);
                prefab.SetActive(true);

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            GameObject prefab = null;
            try
            {
                prefab = GameObject.Instantiate(ModelPrefab.DSSFloorServerRackPrefab);

                var size = new Vector3(1.218514f, 1.895477f, 0.8515267f);
                var center = new Vector3(0f, 1.132018f, 0f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = false;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DSSFloorServerRackController>();

                prefab.SetActive(false);
                var storage = prefab.AddComponent<FCSStorage>();
                storage.Initialize(Mod.DSSFloorServerRackClassName);
                prefab.SetActive(true);

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            gameObject.Set(prefab);
            yield break;
        }
#endif

        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.DSSFloorServerRackIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}