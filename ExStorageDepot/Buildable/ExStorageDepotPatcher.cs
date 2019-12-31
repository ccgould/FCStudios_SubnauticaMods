using ExStorageDepot.Configuration;
using ExStorageDepot.Mono;
using FCSCommon.Helpers;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Utility;

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

        internal static readonly ExStorageDepotBuildable Singleton = new ExStorageDepotBuildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        public override TechType RequiredForUnlock { get; } = TechType.PowerCell;

        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";

        public ExStorageDepotBuildable() : base(Mod.ClassID, Mod.ModFriendly, Mod.ModDesc)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        internal static void PatchHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            PatchHelpers.AddNewKit(
                FCSTechFabricator.Configuration.ExStorageKitClassID,
                null,
                Mod.ModFriendly,
                FCSTechFabricator.Configuration.ExStorageClassID,
                new[] { "ASTS", "ES" },
                null);

            Singleton.Patch();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate(_prefab);

                var container2 = GameObject.Instantiate(CreateStorage());
                container2.name = "StorageContainerUnit";
                container2.transform.parent = prefab.transform;

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.EnsureComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = prefab.FindChild("model");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;
                constructable.allowedOnConstructables = Player.main.GetDepth() > 1;
                
                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                //var beacon = prefab.AddComponent<Beacon>();

                //beacon.label = "DeepDriller";

                var center = new Vector3(0f, 1.579518f,0f);
                var size = new Vector3(2.669801f, 2.776958f, 2.464836f);

                GameObjectHelpers.AddConstructableBounds(prefab, size,center);

                //prefab.AddComponent<MonoClassTest>();
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

        private GameObject CreateStorage()
        {
            GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/Locker");
            var container = GameObject.Instantiate(originalPrefab);

            // Update container renderers
            GameObject cargoCrateModel = container.FindChild("model");
            Renderer[] cargoCrateRenderers = cargoCrateModel.GetComponentsInChildren<Renderer>();

            foreach (Renderer rend in cargoCrateRenderers)
            {
                rend.enabled = false;
            }
            container.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            container.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
            container.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            container.SetActive(true);

            // Update colliders
            GameObject builderTrigger = container.FindChild("Builder Trigger");
            GameObject collider = container.FindChild("Collider");
            BoxCollider builderCollider = builderTrigger.GetComponent<BoxCollider>();
            builderCollider.isTrigger = false;
            builderCollider.enabled = false;
            BoxCollider objectCollider = collider.GetComponent<BoxCollider>();
            objectCollider.isTrigger = false;
            objectCollider.enabled = false;

            // Delete constructable bounds
            ConstructableBounds cb = container.GetComponent<ConstructableBounds>();
            GameObject.DestroyImmediate(cb);

            return container;
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
                    new Ingredient(TechTypeHelpers.GetTechType(FCSTechFabricator.Configuration.ExStorageKitClassID), 1)
                }
            };

            QuickLogger.Debug($"Created Ingredients");

            return customFabRecipe;
        }
    }
}
