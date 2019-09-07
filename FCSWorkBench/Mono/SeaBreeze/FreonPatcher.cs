using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.SeaBreeze
{
    public partial class FreonBuildable : Craftable
    {
        private Text _label;
        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } = new[] { "ARS", "SB" };
        internal static TechType TechTypeID { get; private set; }
        public override CraftTree.Type FabricatorType { get; } =
            FCSTechFabricatorBuildable.TechFabricatorCraftTreeType;

        public FreonBuildable() : base("Freon_ARS", "Freon", "Freon gives you four weeks of SeaBreeze cooling on Planet 4546B.")
        {
            if (!GetPrefabs())
            {
                QuickLogger.Error("Failed to retrieve all prefabs");
            }

            OnFinishedPatching = () =>
            {
                TechTypeID = this.TechType;
                //Add the new TechType Hand Equipment type
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };

        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(this._prefab);

            prefab.name = this.PrefabFileName;

            if (!GetPrefabs())
            {
                QuickLogger.Error("Failed to get all components");
                return null;
            }

            // Make the object drop slowly in water
            var wf = prefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

            // Add fabricating animation
            var fabricatingA = prefab.AddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();

            prefabID.ClassId = this.ClassID;

            //// Set proper shaders (for crafting animation)
            //Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = prefab.GetComponentInChildren<Renderer>();
            //renderer.material.shader = marmosetUber;

            // Update sky applier
            var applier = prefab.GetComponent<SkyApplier>();
            if (applier == null)
                applier = prefab.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = prefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Set collider
            var collider = prefab.GetComponent<BoxCollider>();

            var placeTool = prefab.AddComponent<PlaceTool>();
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

            var techTag = prefab.AddComponent<TechTag>();
            techTag.type = this.TechType;

            prefab.AddComponent<Freon>();

            prefab.AddComponent<FCSTechFabricatorTag>();

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {

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
    }
}
