using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.FormattingStation;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.ItemDisplay;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable
{
    internal class DSSFormattingStationPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public DSSFormattingStationPatch() : base(Mod.DSSFormattingStationClassName, Mod.DSSFormattingStationFriendlyName, Mod.DSSFormattingStationDescription)
        {
            OnFinishedPatching += () =>
            {
                var dssFormattingStationKit = new FCSKit(Mod.DSSFormattingStationKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                dssFormattingStationKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.DSSFormattingStationKitClassID.ToTechType(), 105000, StoreCategory.Storage);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.DSSFormattingStationPrefab);

                var center = new Vector3(0f, 0.1705714f, 0.2497025f);
                var size = new Vector3(0.7182501f, 0.9331782f, 0.500595f);


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
                prefab.AddComponent<DSSFormattingStationController>();
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
            return Mod.DSSFormattingStationIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.DSSFormattingStationIngredients;
        }
#endif
    }
}
