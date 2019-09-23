using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.SeaBreeze
{
    public partial class FreonBuildable : Craftable
    {
        private Text _label;
        private string _resourcePath;
        private GameObject _gameObject;
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

            Register();

            OnFinishedPatching = () =>
            {
                TechTypeID = this.TechType;
                //Add the new TechType Hand Equipment type
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        private void Register()
        {
            var rigidbody = _prefab.AddComponent<Rigidbody>();

            // Make the object drop slowly in water
            var wf = _prefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;
            wf.useRigidbody = rigidbody;

            // Add fabricating animation
            var fabricatingA = _prefab.AddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;


            //// Set proper shaders (for crafting animation)
            //Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = _prefab.GetComponentInChildren<Renderer>();
            //renderer.material.shader = marmosetUber;

            // Update sky applier
            var applier = _prefab.GetComponent<SkyApplier>();
            if (applier == null)
                applier = _prefab.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = _prefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Set collider
            var collider = _prefab.GetComponent<BoxCollider>();
            collider.enabled = false;

            var placeTool = _prefab.AddComponent<PlaceTool>();
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

            _prefab.AddComponent<Freon>();

            _prefab.AddComponent<FCSTechFabricatorTag>();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(this._prefab);

            prefab.name = this.PrefabFileName;

            var techTag = prefab.AddComponent<TechTag>();
            techTag.type = this.TechType;

            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();

            prefabID.ClassId = this.ClassID;

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }
    }
}
