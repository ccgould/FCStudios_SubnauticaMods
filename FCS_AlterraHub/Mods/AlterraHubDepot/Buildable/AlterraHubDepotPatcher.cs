
using System;
using FCS_AlterraHub.Helpers;
using System.Collections;
using System.IO;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mods.AlterraHubDepot.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
using System.Collections.Generic;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif


namespace FCS_AlterraHub.Mods.AlterraHubDepot.Buildable
{
    internal class AlterraHubDepotPatcher : SMLHelper.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();
        public override TechType RequiredForUnlock => TechType;
        public override bool UnlockedAtStart => false;

        public AlterraHubDepotPatcher() : base(Mod.AlterraHubDepotClassID, Mod.AlterraHubDepotFriendly, Mod.AlterraHubDepotDescription)
        {
            OnFinishedPatching += () =>
            {
                Mod.AlterraHubDepotTechType = TechType;
            };
        }

        public override string DiscoverMessage => $"{FriendlyName} Unlocked!";
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
            var prefab = Register();
            gameObject.Set(prefab);
            yield break;
        }

        private GameObject Register()
        {
            var prefab = GameObject.Instantiate(Buildables.AlterraHub.AlterraHubDepotPrefab);

            var size = new Vector3(0.8698499f, 1.545477f, 0.4545232f);
            var center = new Vector3(0f, 0.1363693f, 0.4475707f);

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
            prefab.AddComponent<AlterraHubDepotController>();

            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            return prefab;
        }


        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.AlterraHubDepotIngredients;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
