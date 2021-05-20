using System;
using System.IO;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mono.AlterraHubDepot.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHubDepot.Buildable
{
    internal class AlterraHubDepotPatcher : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public override TechType RequiredForUnlock => Mod.AlterraHubDepotFragmentTechType;

        public override string DiscoverMessage => $"{this.FriendlyName} Unlocked!";

        public override bool AddScannerEntry => true;

        public override int FragmentsToScan => 1;

        public override float TimeToScanFragment => 5f;

        public override bool DestroyFragmentOnScan => false;

        public AlterraHubDepotPatcher() : base(Mod.AlterraHubDepotClassID, Mod.AlterraHubDepotFriendly, Mod.AlterraHubDepotDescription)
        {
            OnFinishedPatching += () =>
            {
                Mod.AlterraHubDepotTechType = TechType;
            };
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
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

                prefab.SetActive(false);
                var fcsStorage = prefab.AddComponent<FCSStorage>();
                fcsStorage.Initialize(48,6,8,Mod.AlterraHubDepotFriendly, Mod.AlterraHubDepotClassID);
                prefab.SetActive(true);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<AlterraHubDepotController>();
                prefab.AddComponent<FCSGameLoadUtil>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
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
                var prefab = GameObject.Instantiate(AlterraHub.AlterraHubDepotPrefab);

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
                prefab.AddComponent<AlterraHubDepotController>();
                //prefab.AddComponent<FCSGameLoadUtil>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
            gameObject.Set(prefab);
            yield break;
        }
#endif

        protected override TechData GetBlueprintRecipe()
        {
            return Mod.AlterraHubDepotIngredients;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
