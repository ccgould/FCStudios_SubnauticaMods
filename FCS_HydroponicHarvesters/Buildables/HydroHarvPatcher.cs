using System;
using System.Collections.Generic;
using System.IO;
using FCS_HydroponicHarvesters.Configuration;
using FCS_HydroponicHarvesters.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Buildables
{
    internal partial class HydroponicHarvestersBuildable : Buildable
    {
        private Vector3 _bCenter;
        private Vector3 _bSize;
        private TechType _kit;
        private GameObject _prefab;
        private List<Vector3> _bubbleLocations;

        public HydroponicHarvestersBuildable(string classId, string friendly, string description, Vector3 bSize, Vector3 bCenter, TechType kit, GameObject prefab,List<Vector3> bubbles) : base(classId, friendly, description)
        {
            _bCenter = bCenter;
            _bSize = bSize;
            _kit = kit;
            _prefab = prefab;
            _bubbleLocations = bubbles;

            OnFinishedPatching += () =>
            {
                AdditionalPatching();
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                GameObjectHelpers.AddConstructableBounds(prefab, _bSize, _bCenter);

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

                constructable.allowedOutside = true;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                var light = prefab.GetComponentInChildren<Light>();

                var lightR = prefab.AddComponent<RegistredLightSource>();
                lightR.hostLight = light;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                AddBubbles(prefab);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<HydroHarvController>();
                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName); 
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        private void AddBubbles(GameObject prefab)
        {
            foreach (Vector3 bubbleLocation in _bubbleLocations)
            {
                MaterialHelpers.AddNewBubbles(prefab, bubbleLocation, new Vector3(270f, 266f, 0f));
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
                    new Ingredient(_kit,1)
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
                    new Ingredient(Mod.FCSDemoKitClassID.ToTechType(),1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
#endif
        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;
    }
}
