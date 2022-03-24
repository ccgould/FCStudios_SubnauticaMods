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
using FCS_HomeSolutions.Mods.TV.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.TV.Buildable
{
    internal class TVPatch : SMLHelper.V2.Assets.Buildable
    {
        protected GameObject _prefab;
        protected Settings _settings;

        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public TVPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings) : base(classId, friendlyName, description)
        {
            _prefab = prefab;
            _settings = settings;

            OnStartedPatching += () =>
            {
                QuickLogger.Debug("Patched Kit");
                var kit = new FCSKit(settings.KitClassID, friendlyName, Path.Combine(AssetsFolder, $"{classId}.png"));
                kit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, settings.KitClassID.ToTechType(), 78750, StoreCategory.Home);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

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
                prefab.AddComponent<TVController>();

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
            var customFabRecipe = new TechData()
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
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
