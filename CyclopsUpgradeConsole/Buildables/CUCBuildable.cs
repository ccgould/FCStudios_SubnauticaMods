using CyclopsUpgradeConsole.Mono;

namespace CyclopsUpgradeConsole.Buildables
{
    using SMLHelper.V2.Assets;
    using System;
    using SMLHelper.V2.Crafting;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using Configuration;
    using FCSCommon.Extensions;
    using FCSCommon.Helpers;
    using FCSCommon.Utilities;
    using SMLHelper.V2.Utility;

    internal partial class CUCBuildable : Buildable
    {
        public CUCBuildable() : base(Mod.ModClassName, Mod.ModFriendlyName, Mod.ModDescription)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public override GameObject GetGameObject()
        {
            try
            {
                if (GetPrefabs())
                {
                    var prefab = GameObject.Instantiate(Prefab);

                    DisableUpgradesMeshes(prefab);

                    //Scale the object
                    prefab.transform.localScale += new Vector3(0.24f, 0.24f, 0.24f);

                    var size = new Vector3(0.5268422f, 1.300237f, 0.3058454f);
                    var center = new Vector3(-0.001188397f, 0.2168427f, 0.1745552f);

                    GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                    var model = prefab.FindChild("model");

                    //========== Allows the building animation and material colors ==========// 
                    Shader shader = Shader.Find("MarmosetUBER");
                    Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                    SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                    skyApplier.renderers = renderers;
                    skyApplier.anchorSky = Skies.Auto;
                    //========== Allows the building animation and material colors ==========// 

                    // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                    var lwe = prefab.AddComponent<LargeWorldEntity>();
                    lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                    // Add constructible
                    var constructable = prefab.AddComponent<Constructable>();

                    constructable.allowedOutside = false;
                    constructable.allowedInBase = false;
                    constructable.allowedOnGround = false;
                    constructable.allowedOnWall = true;
                    constructable.rotationEnabled = false;
                    constructable.allowedOnCeiling = false;
                    constructable.allowedInSub = true;
                    constructable.allowedOnConstructables = false;
                    constructable.model = model;
                    constructable.techType = TechType;
                    
                    PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                    prefabID.ClassId = ClassID;

                    prefab.AddComponent<CUCDisplayManager>();
                    prefab.AddComponent<CUCController>();
                    return prefab;
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

        private void DisableUpgradesMeshes(GameObject prefab)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObjectHelpers.FindGameObject(prefab, $"module_{i+1}")?.SetActive(false);
            }
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.CyclopsUpgradeConsoleKitClassID.ToTechType(),1)
                }
            };
            return customFabRecipe;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(_assetFolder, $"{ClassID}.png")));
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.CyclopsUpgradeConsoleKitClassID.ToTechType(),1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
#endif
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;
    }
}