using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.TrashReceptacle.Mono;
using FCS_HomeSolutions.TrashRecycler.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_HomeSolutions.TrashReceptacle.Buildable
{
    internal class TrashRecyclerPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public TrashRecyclerPatch() : base(Mod.RecyclerClassID, Mod.RecyclerFriendly, Mod.RecyclerDescription)
        {
            OnFinishedPatching += () =>
            {
                var recyclerKit = new FCSKit(Mod.RecyclerKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                recyclerKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, recyclerKit.TechType, 157500, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.TrashRecyclerPrefab);

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
                var storageContainer = prefab.AddComponent<FCSStorage>();
                storageContainer.Initialize(Mod.RecyclerClassID);
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


#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            return Mod.TrashRecyclerIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.TrashRecyclerIngredients;
        }
#endif
    }
}
