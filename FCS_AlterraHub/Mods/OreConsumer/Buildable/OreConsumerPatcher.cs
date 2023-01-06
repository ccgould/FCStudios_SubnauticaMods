using System;
using System.Collections;
using SMLHelper.Crafting;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.OreConsumer.Mono;
using FCSCommon.Utilities;
using SMLHelper.Utility;
using UnityEngine;
using System.Collections.Generic;

#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_AlterraHub.Mods.OreConsumer.Buildable
{
    internal class OreConsumerPatcher : SMLHelper.Assets.Buildable
    {
        private readonly GameObject _prefab;
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();
        public override TechType RequiredForUnlock => TechType;
        public override bool UnlockedAtStart => false;

        public static float OreProcessingTime { get; set; } = 90;

        internal const string OreConsumerClassID = "OreConsumer";
        internal const string OreConsumerFriendly = "Alterra Ore Consumer";
        internal const string OreConsumerDescription = " Turns your ores into credits to use at the Alterra Hub. The Ore Consumer is always very hungry: keep it well fed.";
        internal const string OreConsumerPrefabName = "FCS_OreConsumer";
        public override string DiscoverMessage => $"{FriendlyName} Unlocked!";
        public OreConsumerPatcher() : base(OreConsumerClassID, OreConsumerFriendly, OreConsumerDescription)
        {
            _prefab = AlterraHub.GetPrefab(OreConsumerPrefabName);
            OnFinishedPatching += () =>
            {
                Mod.OreConsumerTechType = TechType;
                
            };
        }

        public override List<TechType> CompoundTechsForUnlock => GetUnlocks();

        private List<TechType> GetUnlocks()
        {
            var list = new List<TechType>();

            foreach (var ingredient in this.GetBlueprintRecipe().Ingredients)
            {
                list.Add(ingredient.techType);
            }

            return list;
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);

            var center = new Vector3(0f, 3.477769f, 0f);
            var size = new Vector3(6.227153f, 6.457883f, 5.26316f);

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
            constructable.forceUpright = true;
            constructable.placeDefaultDistance = 5;
            constructable.placeMinDistance = 5;
            constructable.placeMaxDistance = 10;

            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = ClassID;

            prefab.AddComponent<TechTag>().type = TechType;
            prefab.AddComponent<OreConsumerController>();

            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            gameObject.Set(prefab);
            yield break;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Glass, 2),
                    new Ingredient(TechType.PlasteelIngot, 2),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Silicone, 2),
                    new Ingredient(TechType.Lubricant, 1)
                }
            };
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
