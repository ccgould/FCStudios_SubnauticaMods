using FCSCommon.Extensions;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCSTechFabricator.Mono.SeaCooker
{
    internal  class SeaGasTankCraftable : TechFabCraftable
    {
        public override TechType TechTypeID { get; set; }

        public override string[] StepsToFabricatorTab { get; } = new[] { "AE", "SC" };

        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";

        public SeaGasTankCraftable() : base("SeaGasTank_SC", "Sea Gas Tank", "This tank allows you too cook food in the Sea Cooker using Gaspod gas.",false,EquipmentType.Tank)
        {
            
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(QPatch.SeaGasTank);

            prefab.name = this.PrefabFileName;

            // Set collider
            var collider = prefab.GetComponent<BoxCollider>();
            collider.enabled = false;
            
            var rb = prefab.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Make the object drop slowly in water
            var wf = prefab.GetOrAddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

            // Add fabricating animation
            var fabricatingA = prefab.GetOrAddComponent<VFXFabricating>();
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
            var applier = prefab.GetOrAddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = prefab.GetOrAddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;


            PrefabIdentifier prefabID = prefab.GetOrAddComponent<PrefabIdentifier>();

            prefabID.ClassId = this.ClassID;

            var techTag = prefab.GetOrAddComponent<TechTag>();
            techTag.type = TechType;

            prefab.AddComponent<FCSTechFabricatorTag>();

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }

        public override GameObject OriginalPrefab { get; set; }

        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;

        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
    }
}
