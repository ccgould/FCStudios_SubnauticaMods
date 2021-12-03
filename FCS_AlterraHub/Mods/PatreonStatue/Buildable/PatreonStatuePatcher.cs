using System;
using System.Collections;
using SMLHelper.V2.Crafting;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.PatreonStatue.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_AlterraHub.Mods.PatreonStatue.Buildable
{
    internal class PatreonStatuePatcher : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();
        
        internal const string PatreonStatueClassID = "PatreonStatue";
        internal const string PatreonStatueFriendly = "Patreon Statue";
        internal const string PatreonStatueDescription = "N/A";
        internal const string PatreonStatuePrefabName = "FCS_PatreonStatue";

        public PatreonStatuePatcher() : base(PatreonStatueClassID, PatreonStatueFriendly, PatreonStatueDescription)
        {
            _prefab = AlterraHub.GetPrefab(PatreonStatuePrefabName);
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                var center = new Vector3(0f, 1.32669f, 0.1855279f);
                var size = new Vector3(1.439185f, 2.406436f, 1.32865f);

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

                constructable.allowedOutside = false;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;
                constructable.forceUpright = true;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<PatreonStatueController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);

            var center = new Vector3(0f, 1.32669f, 0.1855279f);
            var size = new Vector3(1.439185f, 2.406436f, 1.32865f);

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

            constructable.allowedOutside = false;
            constructable.allowedInBase = true;
            constructable.allowedOnGround = true;
            constructable.allowedOnWall = false;
            constructable.rotationEnabled = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedInSub = false;
            constructable.allowedOnConstructables = false;
            constructable.model = model;
            constructable.techType = TechType;
            constructable.forceUpright = true;
            
            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = ClassID;

            prefab.AddComponent<TechTag>().type = TechType;
            prefab.AddComponent<PatreonStatueController>();

            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            gameObject.Set(prefab);
            yield break;
        }
#endif

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 1),
                }
            };
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
