using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.DeepDriller
{
    public partial class DeepDrillerKitBuildable : Craftable
    {
        private GameObject _prefab;
        private Text _label;
        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";
        public override string IconFileName { get; } = "Kit_FCS.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } = new[] { "AIS", "DD" };
        internal static TechType TechTypeID { get; private set; }
        public override CraftTree.Type FabricatorType { get; } =
            FCSTechFabricatorBuildable.TechFabricatorCraftTreeType;

        public DeepDrillerKitBuildable() : base("DeepDrillerKit_DD", "Deep Driller Kit", "This kit allows you to make one Deep Driller unit")
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

            if (!FindAllComponents(prefab))
            {
                QuickLogger.Error("Failed to get all components");
                return null;
            }

            var model = prefab.GetComponentInChildren<Canvas>().gameObject;
            model.FindChild("Screen").SetActive(true);

            _label.text = FriendlyName;

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
                    new Ingredient(TechType.MapRoomHUDChip, 1),
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.ExosuitDrillArmModule, 1),
                    new Ingredient(TechType.Lubricant, 2),
                    new Ingredient(TechType.VehicleStorageModule, 1),
                }
            };

            return customFabRecipe;
        }

        public bool GetPrefabs()
        {
            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            _prefab = QPatch.Kit;

            //If the prefab isn't null lets add the shader to the materials
            if (_prefab != null)
            {
                //Lets apply the material shader
                Shaders.ApplyKitShaders(_prefab);
            }

            return true;
        }

        private bool FindAllComponents(GameObject prefab)
        {
            var canvasObject = prefab.GetComponentInChildren<Canvas>().gameObject;
            if (canvasObject == null)
            {
                QuickLogger.Error("Could not find the canvas");
                return false;
            }

            _label = canvasObject.FindChild("Screen").FindChild("Label").GetComponent<Text>();
            return true;
        }

    }
}
