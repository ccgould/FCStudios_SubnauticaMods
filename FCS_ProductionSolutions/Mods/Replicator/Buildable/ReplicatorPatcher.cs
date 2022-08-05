using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.Replicator.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_ProductionSolutions.Mods.Replicator.Buildable
{
    internal partial class ReplicatorBuildable : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;

        internal const string ReplicatorTabID = "RM";
        internal const string ReplicatorFriendlyName = "Replicator";
        internal const string ReplicatorModName = "Replicator";
        public const string ReplicatorDescription = "Duplicates non-living material scanned by the Matter Analyzer.";
        internal static string ReplicatorKitClassID => $"{ReplicatorModName}_Kit";
        internal static string ReplicatorClassName => ReplicatorModName;
        internal static string ReplicatorPrefabName => "Replica-Fabricator";


        public ReplicatorBuildable() : base(ReplicatorClassName, ReplicatorFriendlyName, ReplicatorDescription)
        {

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(ReplicatorKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, ReplicatorKitClassID.ToTechType(), 196875, StoreCategory.Production);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            GameObject prefab = null;
            try
            {
                prefab = GameObject.Instantiate(ModelPrefab.ReplicatorPrefab);

                prefab.name = this.PrefabFileName;
                
                var center = new Vector3(0f, 1.219271f, 0f);
                var size = new Vector3(0.7963083f, 2.037489f, 0.8331643f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

				SubRoot subRoot = prefab.GetComponentInParent<SubRoot>();
				if (subRoot != null)
				{
                    // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                    var lwe = prefab.AddComponent<LargeWorldEntity>();
                    lwe.cellLevel = LargeWorldEntity.CellLevel.Global;
				}
                
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
                constructable.allowedOutside = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;


                prefab.SetActive(false);
                var storageContainer = prefab.AddComponent<ReplicatorSlot>();
                storageContainer.Initialize(25, 1, 1, FriendlyName, ClassID);
                storageContainer.enabled = false;
                prefab.SetActive(true);
                
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<ReplicatorController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
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
                prefab = GameObject.Instantiate(ModelPrefab.ReplicatorPrefab);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 1.219271f, 0f);
                var size = new Vector3(0.7963083f, 2.037489f, 0.8331643f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 


                SubRoot subRoot = prefab.GetComponentInParent<SubRoot>();
                if (subRoot == null)
                {
                    // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                    var lwe = prefab.AddComponent<LargeWorldEntity>();
                    lwe.cellLevel = LargeWorldEntity.CellLevel.Global;
                }

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;


                prefab.AddComponent<ReplicatorSlot>();

                UWEHelpers.CreateStorageContainer(prefab, prefab.FindChild("StorageRoot"), ClassID, FriendlyName, 1, 30);


                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<ReplicatorController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
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
                    new Ingredient(ReplicatorKitClassID.ToTechType(), 1)
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
