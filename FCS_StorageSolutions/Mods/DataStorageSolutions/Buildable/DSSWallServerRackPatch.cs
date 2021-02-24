using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable
{
    internal class DSSWallServerRackPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public DSSWallServerRackPatch() : base(Mod.DSSWallServerRackClassName, Mod.DSSWallServerRackFriendlyName, Mod.DSSWallServerRackDescription)
        {
            OnFinishedPatching += () =>
            {
                var DSSWallServerRackKit = new FCSKit(Mod.DSSWallServerRackKitClassID, Mod.DSSWallServerRackFriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                DSSWallServerRackKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, DSSWallServerRackKit.TechType, 65625, StoreCategory.Storage);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.DSSWallServerRackPrefab);

                var size = new Vector3(0.427167f, 0.7129324f, 0.3643499f);
                var center = new Vector3(0f, 0f, 0.2369909f);

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

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DSSWallServerRackController>();

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
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
            return Mod.DSSWallServerRackIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.DSSWallServerRackIngredients;
        }
#endif
    }
}