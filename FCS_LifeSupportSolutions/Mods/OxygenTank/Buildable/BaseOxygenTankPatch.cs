using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCS_LifeSupportSolutions.Mods.OxygenTank.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_LifeSupportSolutions.Mods.BaseOxygenTank.Buildable
{
    internal class BaseOxygenTankPatch : SMLHelper.Assets.Buildable
    {
        private bool _isKitType;
        private string _iconName;
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

        private TechType kitTechType = TechType.None;

        public BaseOxygenTankPatch(string classID, string iconName,bool isKitType = false) : base(classID, Mod.BaseOxygenTankFriendly, Mod.BaseOxygenTankDescription)
        {
            _iconName = iconName;
            _isKitType = isKitType;

            OnStartedPatching+= () =>
            {
                if (!isKitType) return;
                var baseOxygenTank = new FCSKit($"{Mod.BaseOxygenTankClassID}_Kit", FriendlyName,
                    Path.Combine(AssetsFolder, $"{iconName}.png"));
                baseOxygenTank.Patch();
                kitTechType = baseOxygenTank.TechType;
            };


            OnFinishedPatching += () =>
            {
                if (!isKitType) return;
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, kitTechType, 49250, StoreCategory.LifeSupport);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.BaseOxygenTankPrefab);

                var center = new Vector3(-2.488494e-05f, 0.6907129f, 0.02741182f);
                var size = new Vector3(1.997957f, 1.303595f, 1.819634f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 


                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

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

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<BaseOxygenTankController>();
                MaterialHelpers.ChangeEmissionColor(string.Empty, prefab, Color.cyan);


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
            if (_isKitType)
            {
                return TrashRecyclerIngredients;
            }
            return Mod.BaseOxygenTankIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{_iconName}.png"));
        }


        internal static RecipeData TrashRecyclerIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient($"{Mod.BaseOxygenTankClassID}_Kit".ToTechType(), 1),
            }
        };
    }
}
