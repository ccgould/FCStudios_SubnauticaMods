using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_HomeSolutions.BaseOperator.Mono;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_HomeSolutions.BaseOperator.Buildable
{
    internal class BaseOperatorPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public BaseOperatorPatch() : base(Mod.BaseOperatorClassID, Mod.BaseOperatorFriendly, Mod.BaseOperatorDescription)
        {
            OnFinishedPatching += () =>
            {
                var baseOperatorKit = new FCSKit(Mod.BaseOperatorKitClassID, Mod.BaseOperatorFriendly, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                baseOperatorKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, baseOperatorKit.TechType, 30000, StoreCategory.Home);

            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.BaseOperatorPrefab);

                //var size = new Vector3(1.353966f, 2.503282f, 1.006555f);
                //var center = new Vector3(0.006554961f, 1.394679f, 0.003277525f);

                //GameObjectHelpers.AddConstructableBounds(prefab, size, center);

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
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<BaseOperatorController>();
                return prefab;

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }


#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            return Mod.BaseOperatorIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.BaseOperatorIngredients;
        }
#endif
    }
}
