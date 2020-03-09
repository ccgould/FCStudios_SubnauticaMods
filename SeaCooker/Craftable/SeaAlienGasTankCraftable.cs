using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AE.SeaCooker.Configuration;
using ARS_SeaBreezeFCS32.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace AE.SeaCooker.Craftable
{
    internal class SeaAlienGasTankCraftable : FcCraftable
    {
        private GameObject _tank;

        public SeaAlienGasTankCraftable(string classId, string friendlyName, string description, FcCraftingTab parentTab) : base(classId, friendlyName, description, parentTab)
        {
            _tank = QPatch.GlobalBundle.LoadAsset<GameObject>("SeaAlienGasTank");

            OnFinishedPatching += () =>
            {
                CraftDataHandler.SetEquipmentType(TechType,EquipmentType.NuclearReactor);
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(_tank);

            prefab.name = this.PrefabFileName;

            // Set collider
            var collider = prefab.GetComponent<BoxCollider>();
            collider.enabled = false;

            var rb = prefab.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Make the object drop slowly in water
            var wf = prefab.EnsureComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

            // Add fabricating animation
            var fabricatingA = prefab.EnsureComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            //// Set proper shaders (for crafting animation)
            //Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = prefab.GetComponentInChildren<Renderer>();
            //renderer.material.shader = marmosetUber;

            // Update sky applier
            var applier = prefab.EnsureComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = prefab.EnsureComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;


            PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();

            prefabID.ClassId = this.ClassID;

            var techTag = prefab.EnsureComponent<TechTag>();
            techTag.type = TechType;

            prefab.AddComponent<FCSTechFabricatorTag>();

            ApplySeaTankShaders(prefab);

            return prefab;
        }

        private void ApplySeaTankShaders(GameObject prefab)
        {
            MaterialHelpers.ApplyEmissionShader("SeaCooker_BaseColor", "SeaCooker_Emissive", prefab, QPatch.GlobalBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SeaCooker_BaseColor", "SeaCooker_Normal", prefab, QPatch.GlobalBundle);
            MaterialHelpers.ApplySpecShader("SeaCooker_BaseColor", "SeaCooker_Spec", prefab, 1, 6f, QPatch.GlobalBundle);
            MaterialHelpers.ApplyAlphaShader("SeaCooker_BaseColor", prefab);
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.SeaTreaderPoop, 2),
                    new Ingredient(TechType.Tank, 1),
                    new Ingredient(TechType.FilteredWater, 2)
                }
            };
            return customFabRecipe;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{ClassID}.png"));
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.SeaTreaderPoop, 2),
                    new Ingredient(TechType.Tank, 1),
                    new Ingredient(TechType.HydrochloricAcid, 1),
                    new Ingredient(TechType.BigFilteredWater, 2)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{ClassID}.png"));
        }
#endif

        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => Mod.BundleName;
    }
}
