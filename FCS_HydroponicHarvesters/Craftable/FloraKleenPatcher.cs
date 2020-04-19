using System;
using System.Collections.Generic;
using System.IO;
using FCS_HydroponicHarvesters.Buildables;
using FCS_HydroponicHarvesters.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Craftable
{
    internal class FloraKleenPatcher : FcCraftable
    {
        private static GameObject OriginalPrefab;
        private GameObject _prefab;

        public FloraKleenPatcher(string classId, string friendlyName, string description, FcCraftingTab parentTab) : base(classId, friendlyName, description, parentTab)
        {
            OnFinishedPatching += () =>
            {
                //AddComponentsToPrefab();

                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        private void ApplyShaders()
        {
            MaterialHelpers.ApplySpecShader("Freon_Bottle", $"Freon_Freon_Bottle_Specular", OriginalPrefab, 1, 6f, QPatch.GlobalBundle);
            MaterialHelpers.ApplyNormalShader("Freon_Bottle", "Freon_Freon_Bottle_Normal", OriginalPrefab, QPatch.GlobalBundle);
        }

        public override GameObject GetGameObject()
        {
            try
            {
                if (_prefab == null)
                {
                    QuickLogger.Error("Bottle Prefab is null", true);
                    return null;
                }
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();
                prefabID.ClassId = this.ClassID;

                var techTag = prefab.EnsureComponent<TechTag>();
                techTag.type = TechType;

                //Add the FCSTechFabricatorTag component
                prefab.AddComponent<FCSTechFabricatorTag>();

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        internal void Register()
        {

            HydroponicHarvestersModelPrefab.GetPrefabs();

            _prefab = HydroponicHarvestersModelPrefab.BottlePrefab;

            if (_prefab == null)
            {
                QuickLogger.Error("FloraKleen prefab is null");
                return;
            }

            GameObjectHelpers.AddConstructableBounds(_prefab, new Vector3(0.1969692f, 0.25098f, 0.1916926f), new Vector3(0, -0.01675579f, 0));

            QuickLogger.Debug("Added Constructable Bounds");

            //Make the rigid body isKinematic so it doesnt shake the cyclops
            var rb = _prefab.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            QuickLogger.Debug("Added Rigid Body");

            // Set collider
            var collider = _prefab.GetComponentInChildren<Collider>();
            collider.enabled = true;
            collider.isTrigger = true;
            QuickLogger.Debug("Added Getting Collider");
            
            // Make the object drop slowly in water
            var wf = _prefab.EnsureComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;
            QuickLogger.Debug("Ensuring World Forces");
            
            // Add fabricating animation
            var fabricatingA = _prefab.EnsureComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;
            QuickLogger.Debug("Ensuring VFXFabricating");

            //// Set proper shaders (for crafting animation)
            var renderer = _prefab.GetComponentInChildren<Renderer>();
            QuickLogger.Debug("Getting Renderer");

            // Update sky applier
            var applier = _prefab.EnsureComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;
            QuickLogger.Debug("Ensuring SkyApplier");

            // We can pick this item
            var pickupable = _prefab.EnsureComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;
            QuickLogger.Debug("Ensuring Pickupable");

            //Allow this kit to be placed on surfaces in these situations
            var placeTool = _prefab.EnsureComponent<PlaceTool>();
            placeTool.allowedInBase = true;
            placeTool.allowedOnBase = false;
            placeTool.allowedOnCeiling = false;
            placeTool.allowedOnConstructable = true;
            placeTool.allowedOnGround = true;
            placeTool.allowedOnRigidBody = true;
            placeTool.allowedOnWalls = false;
            placeTool.allowedOutside = false;
            placeTool.rotationEnabled = true;
            placeTool.enabled = true;
            placeTool.hasAnimations = false;
            placeTool.hasBashAnimation = false;
            placeTool.hasFirstUseAnimation = false;
            placeTool.mainCollider = collider;
            placeTool.pickupable = pickupable;
            placeTool.drawTime = 0.5f;
            placeTool.dropTime = 1;
            placeTool.holsterTime = 0.35f;
            QuickLogger.Debug("Ensuring PlaceTool");
        }


#if SUBNAUTICA
        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{ClassID}.png"));
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
                    new Ingredient(TechType.FilteredWater, 2),
                    new Ingredient(TechType.Salt, 1),
                }
            };
            return customFabRecipe;
        }
#elif BELOWZERO
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{ClassID}.png"));
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
                    new Ingredient(TechType.GasPod, 1),
                    new Ingredient(TechType.AcidMushroom, 1),
                    new Ingredient(TechType.Titanium, 1),
                }
            };
            return customFabRecipe;
        }

#endif
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => Mod.BundleName;
    }
}