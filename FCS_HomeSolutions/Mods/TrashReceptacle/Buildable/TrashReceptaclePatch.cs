using System;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.TrashReceptacle.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.TrashReceptacle.Buildable
{
    internal class TrashReceptaclePatch : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        internal const string TrashReceptacleClassID = "TrashReceptacle";
        internal const string TrashReceptacleFriendly = "Trash Receptacle";

        internal const string TrashReceptacleDescription = "Use the Trash Receptacle to quickly send your trash to the recycler from the inside of your base";

        internal const string TrashReceptaclePrefabName = "FCS_TrashReceptacle";
        internal const string TrashReceptacleKitClassID = "TrashReceptacle_Kit";
        internal const string TrashReceptacleTabID = "TR";

        public TrashReceptaclePatch() : base(TrashReceptacleClassID, TrashReceptacleFriendly, TrashReceptacleDescription)
        {
            _prefab = _prefab = ModelPrefab.GetPrefabFromGlobal(TrashReceptaclePrefabName);
            OnFinishedPatching += () =>
            {
                var trashReceptacleKit = new FCSKit(TrashReceptacleKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                trashReceptacleKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, trashReceptacleKit.TechType, 78750, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                var size = new Vector3(0.8523935f, 1.468681f, 0.5830949f);
                var center = new Vector3(0f, -0.007400334f, 0.3679641f);

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
                constructable.allowedOnGround = false;
                constructable.allowedOnWall = true;
                constructable.rotationEnabled = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<TrashReceptacleController>();
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
            return new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TrashReceptacleKitClassID.ToTechType(), 1),
            }
        };
    }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
