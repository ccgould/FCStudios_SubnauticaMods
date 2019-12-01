using FCSTechFabricator.Helpers;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCSTechFabricator.Mono.SeaBreeze
{
    public class FreonBuildable : TechFabCraftable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } =  { "ARS", "SB" };
        public override TechType TechTypeID { get; set; }

        public override GameObject OriginalPrefab { get; set; } = QPatch.Freon;

        public FreonBuildable() : 
            base("Freon_ARS", "Freon", "Freon gives you four weeks of SeaBreeze cooling on Planet 4546B.")
        {
            
        }

        public override void Register()
        {
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
            collider.enabled = false;

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

            OriginalPrefab.AddComponent<Freon>();

            OriginalPrefab.AddComponent<FCSTechFabricatorTag>();
        }
        
        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }
    }
}
