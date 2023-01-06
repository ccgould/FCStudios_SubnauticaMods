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
using FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Buildable
{
    internal partial class FireExtinguisherRefuelerBuildable : SMLHelper.Assets.Buildable
    {
        private readonly GameObject _prefab;
        internal const string FireExtinguisherRefuelerClassID = "FireExtinguisherRefueler";
        internal const string FireExtinguisherRefuelerFriendly = "Fire Extinguisher Refueler";

        internal const string FireExtinguisherRefuelerDescription =
            "Keep your habitat safe! Holds and recharges Fire Extinguishers. Wall-mounted.";

        internal const string FireExtinguisherRefuelerPrefabName = "FCS_FireExtinguisherRefueler";
        internal const string FireExtinguisherRefuelerKitClassID = "FireExtinguisherRefueler_Kit";
        internal const string FireExtinguisherRefuelerTabID = "FER";

        public FireExtinguisherRefuelerBuildable() : base(FireExtinguisherRefuelerClassID,
            FireExtinguisherRefuelerFriendly, FireExtinguisherRefuelerDescription)
        {
            _prefab = ModelPrefab.GetPrefabFromGlobal(FireExtinguisherRefuelerPrefabName);
            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(FireExtinguisherRefuelerKitClassID, FriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType,
                    FireExtinguisherRefuelerKitClassID.ToTechType(), 60000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0.002549291f, 0.04286739f, 0.2148812f);
                var size = new Vector3(0.9426436f, 0.5879701f, 0.4037365f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();

                var feItemSize =
#if SUBNAUTICA
                    CraftData.GetItemSize
#else
                    TechData.GetItemSize
#endif
                        (TechType.FireExtinguisher);

                UWEHelpers.CreateStorageContainer(prefab, prefab.FindChild("StorageRoot"), ClassID,
                    "Fire Extinguisher Receptacle", feItemSize.x, feItemSize.y);

                prefab.AddComponent<FireExtinguisherRefuelerController>();
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", ClassID);

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
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(FireExtinguisherRefuelerKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
    }
}