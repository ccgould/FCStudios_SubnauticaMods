using ExStorageDepot.Mono;

namespace ExStorageDepot.Buildable
{
    using FCSCommon.Extensions;
    using FCSCommon.Utilities;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    internal partial class ExStorageDepotBuildable : Buildable
    {

        private static readonly ExStorageDepotBuildable Singleton = new ExStorageDepotBuildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        public override TechType RequiredForUnlock { get; } = TechType.PowerCell;

        public override string AssetsFolder { get; } = $"ExStorageDepot/Assets";

        internal const string ModName = "ExStorageDepot";
        internal const string ModFriendly = "Ex-Storage Depot";
        internal const string BundleName = "exstoragedepotunitmodbundle";
        internal const string ModDesc = "Alterra Storage Solutions Ex-Storage Depot allows you to store a large amount of items outside your base.";
        public ExStorageDepotBuildable() : base(ModName, ModFriendly, ModDesc)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        internal static void PatchHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate(_prefab);

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.GetOrAddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = prefab.FindChild("model");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                //var beacon = prefab.AddComponent<Beacon>();

                //beacon.label = "DeepDriller";

                prefab.AddComponent<PrefabIdentifier>().ClassId = this.ClassID;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<ExStorageDepotController>();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");

            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.TitaniumIngot, 2),
                    new Ingredient(TechType.Glass, 1),
                    new Ingredient(TechType.PowerCell, 1),
                }
            };

            QuickLogger.Debug($"Created Ingredients");

            return customFabRecipe;
        }
    }
}
