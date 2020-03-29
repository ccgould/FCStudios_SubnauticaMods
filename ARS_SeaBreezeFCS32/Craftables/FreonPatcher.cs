using System;
using System.Collections.Generic;
using System.IO;
using ARS_SeaBreezeFCS32.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Craftables
{
    internal class FreonPatcher : FcCraftable
    {
        private static GameObject OriginalPrefab; 
        
        public FreonPatcher(string classId, string friendlyName, string description, FcCraftingTab parentTab) : base(classId, friendlyName, description, parentTab)
        {
            OnFinishedPatching += () =>
            {
                AddComponentsToPrefab();

                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(OriginalPrefab);

                prefab.name = this.PrefabFileName;

                PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();

                prefabID.ClassId = this.ClassID;

                var techTag = prefab.EnsureComponent<TechTag>();
                techTag.type = TechType;

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
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
                    new Ingredient(TechType.GasPod, 1),
                    new Ingredient(TechType.AcidMushroom, 1),
                    new Ingredient(TechType.Titanium, 1),
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

        private void AddComponentsToPrefab()
        {
            var go = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(FcAssetBundlesService.PublicAPI.GlobalBundleName);

            if (go != null)
            {
                OriginalPrefab = go.LoadAsset<GameObject>("Freon_Bottle");
            }
            else
            {
                QuickLogger.Error<FreonPatcher>("Couldnt find bundle in the bundle service.");
                return;
            }
            
            if (OriginalPrefab == null)
            {
                QuickLogger.Error("Freon prefab not found");
                return;
            }

            var rigidbody = OriginalPrefab.AddComponent<Rigidbody>();

            // Make the object drop slowly in water
            var wf = OriginalPrefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;
            wf.useRigidbody = rigidbody;

            // Add fabricating animation
            var fabricatingA = OriginalPrefab.AddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;
            
            //// Set proper shaders (for crafting animation)
            Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = OriginalPrefab.GetComponentInChildren<Renderer>();

            renderer.material.shader = marmosetUber;

            // Update sky applier
            var applier = OriginalPrefab.GetComponent<SkyApplier>();
            if (applier == null)
                applier = OriginalPrefab.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = OriginalPrefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Set collider
            var collider = OriginalPrefab.GetComponent<BoxCollider>();
            collider.enabled = true;
            collider.isTrigger = true;

            var placeTool = OriginalPrefab.AddComponent<PlaceTool>();
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

            OriginalPrefab.EnsureComponent<FCSTechFabricatorTag>();

            ApplyShaders();

        }

        private void ApplyShaders()
        {
            MaterialHelpers.ApplySpecShader("Freon_Bottle", $"Freon_Freon_Bottle_Specular", OriginalPrefab, 1, 6f, QPatch.GlobalBundle);
            MaterialHelpers.ApplyNormalShader("Freon_Bottle", "Freon_Freon_Bottle_Normal", OriginalPrefab, QPatch.GlobalBundle);
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => Mod.BundleName;
    }
}
