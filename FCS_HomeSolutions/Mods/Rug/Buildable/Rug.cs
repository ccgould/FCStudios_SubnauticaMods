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
using FCS_HomeSolutions.Mods.Rug.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.Rug.Buildable
{
    internal class Rug : SMLHelper.V2.Assets.Buildable
    {
        protected GameObject _prefab;
        protected Settings _settings;

        public override TechGroup GroupForPDA => _settings.GroupForPDA;
        public override TechCategory CategoryForPDA => _settings.CategoryForPDA;
        public override string AssetsFolder => Mod.GetAssetPath();

        public Rug() : base("FCSRug", "Rug", "A stylish rug from the corporation that loves you. Requires floor.")
        {
            _settings = new Settings
            {
                KitClassID = "rug_kit",
                AllowedInBase = true,
                AllowedOutside = false,
                AllowedOnGround = true,
                AllowedInSub = true,
                AllowedOnConstructables = true,
                RotationEnabled = true,
                Cost = 1500,
                Center = new Vector3(0, 0f, 0f),
                Size = new Vector3(0f, 0f, 0f),
                CategoryForPDA = TechCategory.InteriorModule,
                GroupForPDA = TechGroup.InteriorModules
            };

            if (string.IsNullOrWhiteSpace(_settings.IconName))
            {
                _settings.IconName = ClassID;
            }
            _prefab = ModelPrefab.GetPrefabFromGlobal("FCS_Rug01");


            OnStartedPatching += () =>
            {
                QuickLogger.Debug("Patched Kit");
                var kit = new FCSKit(_settings.KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{_settings.IconName}.png"));
                kit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, _settings.KitClassID.ToTechType(), _settings.Cost, StoreCategory.Home);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

                var model = prefab.FindChild(_settings.ModelName);

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = _settings.AllowedOutside;
                constructable.allowedInBase = _settings.AllowedInBase;
                constructable.allowedOnGround = _settings.AllowedOnGround;
                constructable.allowedOnWall = _settings.AllowedOnWall;
                constructable.rotationEnabled = _settings.RotationEnabled;
                constructable.allowedOnCeiling = _settings.AllowedOnCeiling;
                constructable.allowedInSub = _settings.AllowedInSub;
                constructable.allowedOnConstructables = _settings.AllowedOnConstructables;
                constructable.model = model;
                constructable.techType = TechType;

                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<RugController>();
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
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(_settings.KitClassID.ToTechType(),1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{_settings.IconName}.png"));
        }
    }
}
