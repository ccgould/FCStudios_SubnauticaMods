using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCS_LifeSupportSolutions.Mods.BaseUtilityUnit.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_LifeSupportSolutions.Mods.BaseUtilityUnit.Buildable
{
    internal class BaseUtilityUnitPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

        public BaseUtilityUnitPatch() : base(Mod.BaseUtilityUnityClassName, Mod.BaseUtilityUnitFriendlyName, Mod.BaseUtilityUnitDescription)
        {
            OnStartedPatching += () =>
            {
                var baseUtilityUnitPatch = new FCSKit(Mod.BaseUtilityUnityKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                baseUtilityUnitPatch.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.BaseUtilityUnityKitClassID.ToTechType(), 202500, StoreCategory.LifeSupport);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.BaseUtilityUnitPrefab);

                var center = new Vector3(-0.01851273f, 1.397769f, 0f);
                var size = new Vector3(2.703247f, 2.499594f, 2.679479f);

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
                constructable.allowedOutside = true;
                constructable.allowedInBase = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<BaseUtilityUnitController>();

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);

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
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            return Mod.BaseUtilityUnitIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
