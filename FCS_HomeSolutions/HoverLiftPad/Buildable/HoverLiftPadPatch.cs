using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.HoverLiftPad.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_HomeSolutions.Buildables
{
    internal class HoverLiftPadPatch : Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public HoverLiftPadPatch() : base(Mod.HoverLiftPadClassID, Mod.HoverLiftPadFriendly, Mod.HoverLiftPadDescription)
        {
            OnFinishedPatching += () =>
            {
                var hoverLiftPadKit = new FCSKit(Mod.HoverLiftPadKitClassID, Mod.HoverLiftPadFriendly, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                hoverLiftPadKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, hoverLiftPadKit.TechType, 30000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.HoverLiftPadPrefab);

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
                prefab.AddComponent<HoverLiftPadController>();
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
            return Mod.HoverLiftPadIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.HoverLiftPadIngredients;
        }
#endif
    }
}
