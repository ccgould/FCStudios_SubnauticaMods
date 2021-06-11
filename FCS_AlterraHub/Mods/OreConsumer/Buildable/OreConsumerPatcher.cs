#if SUBNAUTICA
using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.OreConsumer.Mono;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCS_AlterraHub.Mods.OreConsumer.Buildable
{
    internal class OreConsumerPatcher : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public override TechType RequiredForUnlock => Mod.OreConsumerFragmentTechType;

        //public override string DiscoverMessage => $"{this.FriendlyName} Unlocked!";

        //public override bool AddScannerEntry => true;

        //public override int FragmentsToScan => 3;

        //public override float TimeToScanFragment => 5f;

        //public override bool DestroyFragmentOnScan => true;

        public static float OreProcessingTime { get; set; } = 90;

        public OreConsumerPatcher() : base(Mod.OreConsumerClassID, Mod.OreConsumerFriendly, Mod.OreConsumerDescription)
        {
            OnFinishedPatching += () =>
            {
                Mod.OreConsumerTechType = TechType;
            };
        }

        //public override PDAEncyclopedia.EntryData EncyclopediaEntryData
        //{
        //    get
        //    {
        //        PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData
        //        {
        //            key = "Alterra Hub__Alterra Ore Consumer",
        //            path = "fcs/Alterra Hub",
        //            nodes = new[] { "fcs", "Alterra Hub" },
        //            unlocked = false
        //        };
        //        return entry;
        //    }
        //}

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(AlterraHub.OreConsumerPrefab);

                var size = new Vector3(1.353966f, 2.503282f, 1.006555f);
                var center = new Vector3(0.006554961f, 1.394679f, 0.003277525f);

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

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<OreConsumerController>();
                prefab.AddComponent<FCSGameLoadUtil>();

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
                var prefab = GameObject.Instantiate(AlterraHub.OreConsumerPrefab);

                var size = new Vector3(1.353966f, 2.503282f, 1.006555f);
                var center = new Vector3(0.006554961f, 1.394679f, 0.003277525f);

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

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<OreConsumerController>();
                //prefab.AddComponent<FCSGameLoadUtil>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
            gameObject.Set(prefab);
            yield break;
        }
#endif

        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.OreConsumerIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
