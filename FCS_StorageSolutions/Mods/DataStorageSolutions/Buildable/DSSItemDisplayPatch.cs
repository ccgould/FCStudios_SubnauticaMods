using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.ItemDisplay;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable
{
    internal class DSSItemDisplayPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public DSSItemDisplayPatch() : base(Mod.DSSItemDisplayClassName, Mod.DSSItemDisplayFriendlyName, Mod.DSSItemDisplayDescription)
        {
            OnFinishedPatching += () =>
            {
                var dssItemDisplayKit = new FCSKit(Mod.DSSItemDisplayKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                dssItemDisplayKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.DSSItemDisplayKitClassID.ToTechType(), 37500, StoreCategory.Storage);
                //    FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(TechType, new List<FcsEntryData>
                //    {
                //        new FcsEntryData
                //        {
                //            key = "HydroHarvester",
                //            unlocked = true,
                //            path = "fcs",
                //            timeCapsule = false,
                //            nodes = new []{ "fcs"},
                //            Description = "The hydroponic Harvester .....",
                //            Title = "Hydroponic Harvester",
                //            Verbose = true
                //        },
                //        new FcsEntryData
                //        {
                //            key = "HydroHarvester1",
                //            unlocked = true,
                //            path = "fcs",
                //            timeCapsule = false,
                //            nodes = new []{ "fcs"},
                //            Description = "The hydroponic Harvester .....",
                //            Title = "Hydroponic Harvester 1",
                //            Verbose = false
                //        }
                //    });

            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.DSSItemDisplayPrefab);

                var center = new Vector3(0f, -5.960464e-08f, 0.1698987f);
                var size = new Vector3(0.6147361f, 0.8183843f, 0.2793954f);


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
                prefab.AddComponent<DSSItemDisplayController>();
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
            return Mod.DSSItemDisplayIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.DSSItemDisplayIngredients;
        }
#endif
    }
}
