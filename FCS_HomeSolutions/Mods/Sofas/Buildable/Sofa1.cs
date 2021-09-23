using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class Sofa1Buildable : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _bench;
        private readonly GameObject _prefab;
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        internal const string Sofa1ClassID = "Sofa1";
        internal const string Sofa1Friendly = "Sofa 1";
        internal const string Sofa1Description = "Don’t put butt on the floor, put it on this comfy people-shelf!";
        internal const string Sofa1PrefabName = "Sofia01";
        internal const string Sofa1KitClassID = "Sofa1_Kit";

        public Sofa1Buildable() : base(Sofa1ClassID, Sofa1Friendly, Sofa1Description)
        {
            _prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(Sofa1PrefabName, FCSAssetBundlesService.PublicAPI.GlobalBundleName);

            OnStartedPatching += () =>
            {
                var kit = new FCSKit(Sofa1KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                kit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Sofa1KitClassID.ToTechType(), 9000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };

            _bench = Resources.Load<GameObject>("Submarine/Build/Bench");
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = AddChair();

                var mesh = GameObject.Instantiate(_prefab);
                mesh.SetActive(false);

                prefab.name = this.PrefabFileName;

                // Disable renderers
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                    rend.enabled = false;

                var model = mesh.FindChild("model");

                SkyApplier skyApplier = mesh.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                //Add constructible
                var constructable = prefab.GetComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                mesh.transform.SetParent(prefab.transform, false);
                mesh.SetActive(true);

                PrefabIdentifier prefabID = prefab.GetComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                // Get bench
                var bench = prefab.GetComponent<Bench>();
                bench.cinematicController.animatedTransform.localPosition = new Vector3(bench.cinematicController.animatedTransform.localPosition.x, bench.cinematicController.animatedTransform.localPosition.y, bench.cinematicController.animatedTransform.localPosition.z + 0.31f);

                // Update constructable bounds
                var constructableBounds = prefab.GetComponent<ConstructableBounds>();
                constructableBounds.bounds = new OrientedBounds(new Vector3(constructableBounds.bounds.position.x, constructableBounds.bounds.position.y, constructableBounds.bounds.position.z),
                    new Quaternion(constructableBounds.bounds.rotation.x, constructableBounds.bounds.rotation.y, constructableBounds.bounds.rotation.z, constructableBounds.bounds.rotation.w),
                    new Vector3(constructableBounds.bounds.extents.x * 0.3f, constructableBounds.bounds.extents.y, constructableBounds.bounds.extents.z * 0.3f));

                // Modify box colliders
                var collider = prefab.FindChild("Collider").GetComponent<BoxCollider>();
                collider.size = new Vector3(0.85f, 0.43f, 0.85f);
                var builderTrigger = prefab.FindChild("Builder Trigger").GetComponent<BoxCollider>();
                builderTrigger.size = new Vector3(0.85f, 0.43f, 0.85f);
                
                prefab.EnsureComponent<TechTag>().type = TechType;

                prefab.AddComponent<SofaController>();
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, mesh, Color.cyan);
                MaterialHelpers.ChangeMaterialColor(AlterraHub.BaseSecondaryCol, mesh, Color.gray);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public GameObject AddChair(float additionalHeight = 0f)
        {
            var starShipChair = GameObject.Instantiate(_bench);

            foreach (Transform tr in starShipChair.transform)
            {
                tr.localPosition = new Vector3(tr.localPosition.x, tr.localPosition.y + additionalHeight, tr.localPosition.z);
            }

            Renderer[] renderers = starShipChair.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.enabled = true;
            }

            // Add large world entity
            var lwe = starShipChair.EnsureComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;



            //starShipChair.transform.parent = prefab.transform;
            //UWE.Utils.ZeroTransform(starShipChair);
            //starShipChair.transform.localPosition = position;
            //starShipChair.transform.localRotation = rotation;

            var cb = starShipChair.GetComponentInChildren<ConstructableBounds>();
            cb.bounds.size = Vector3.zero;
            cb.bounds.position = Vector3.zero;

            return starShipChair;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new(Sofa1KitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
