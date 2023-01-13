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
using FCS_HomeSolutions.Mods.DisplayBoard.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.DisplayBoard.Buildable
{
    internal partial class DisplayBoardBuildable : SMLHelper.Assets.Buildable
    {
        private readonly GameObject _prefab;
        internal const string DisplayBoardClassID = "DisplayBoard";
        internal const string DisplayBoardFriendly = "Display Board";

        internal const string DisplayBoardDescription = "Get information about your base with these lovely overhead displays.";

        internal const string DisplayBoardPrefabName = "FCS_DisplayBoard";
        internal const string DisplayBoardKitClassID = "DisplayBoard_Kit";
        internal const string DisplayBoardTabID = "DSB";
        public DisplayBoardBuildable() : base(DisplayBoardClassID, DisplayBoardFriendly, DisplayBoardDescription)
        {
            _prefab = ModelPrefab.GetPrefabFromGlobal(DisplayBoardPrefabName);
            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(DisplayBoardKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, DisplayBoardKitClassID.ToTechType(), 81000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, -0.3437063f, 0f);
                var size = new Vector3(1f, 0.474931f, 0.7709165f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

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
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = false;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = true;
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;
                constructable.forceUpright = true;
                constructable.rotationEnabled = true;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                
                prefab.AddComponent<DisplayBoardController>();
                MaterialHelpers.ChangeEmissionColor(string.Empty, prefab, Color.cyan);

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
                    new Ingredient(DisplayBoardKitClassID.ToTechType(), 1)
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
