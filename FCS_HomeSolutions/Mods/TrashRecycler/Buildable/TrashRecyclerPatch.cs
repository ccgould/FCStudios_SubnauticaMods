using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.TrashRecycler.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif


namespace FCS_HomeSolutions.Mods.TrashRecycler.Buildable
{
    internal class TrashRecyclerPatch : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        internal const string RecyclerClassID = "Recycler";
        internal const string RecyclerFriendly = "Recycler";
        internal const string RecyclerDescription = "Recycle your trash and get your resources back";
        internal const string RecyclerPrefabName = "FCS_Recycler";
        internal const string RecyclerKitClassID = "Recycler_Kit";
        internal const string RecyclerTabID = "RR";

        private TechType kitTechType;

        public TrashRecyclerPatch() : base(RecyclerClassID, RecyclerFriendly, RecyclerDescription)
        {
            _prefab = _prefab = ModelPrefab.GetPrefabFromGlobal( RecyclerPrefabName);
            OnStartedPatching += () =>
            {
                var recyclerKit = new FCSKit(RecyclerKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                recyclerKit.Patch(); 
                kitTechType =recyclerKit.TechType;
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, kitTechType, 157500, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                var size = new Vector3(5.240145f, 2.936445f, 3.069993f);
                var center = new Vector3(0.01669312f, 1.519108f, 0.06677395f);

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

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.SetActive(false);
                prefab.GetComponentInChildren<Canvas>()?.gameObject.SetActive(false);
                var storageContainer = prefab.AddComponent<FCSStorage>();
                storageContainer.Initialize(ClassID);
                storageContainer.enabled = false;
                prefab.SetActive(true);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<TrashRecyclerController>();
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
                    new Ingredient(RecyclerKitClassID.ToTechType(), 1),
                }
            };
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}