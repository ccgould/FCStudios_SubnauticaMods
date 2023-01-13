using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Spawnables
{
    internal class FoodSpawnable : Spawnable
    {
        private PeeperBarFoodItemData _foodItemData;
        public override string AssetsFolder => Mod.GetAssetPath();
        public decimal Cost { get; set; }

        public FoodSpawnable(PeeperBarFoodItemData foodItem) : base(foodItem.ClassId, foodItem.Friendly, foodItem.Description)
        {
            _foodItemData = foodItem;
            Cost = foodItem.Cost;
            OnFinishedPatching += () =>
            {
                // Add the new TechType to Hand Equipment type.
                SMLHelper.Handlers.CraftDataHandler.SetEquipmentType(this.TechType, EquipmentType.Hand);

                // Set quick slot type.
                SMLHelper.Handlers.CraftDataHandler.SetQuickSlotType(this.TechType, QuickSlotType.Selectable);
            };


        }

        public override GameObject GetGameObject()
        {
            try
            {
                if (_foodItemData.Prefab == null)
                {
                    _foodItemData.Prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //Set collider
                    _foodItemData.Prefab.EnsureComponent<BoxCollider>();
                }

                var prefab = GameObject.Instantiate(_foodItemData.Prefab);

                prefab.AddComponent<PrefabIdentifier>();
                prefab.AddComponent<TechTag>().type = TechType;

                var rigidBody = prefab.EnsureComponent<Rigidbody>();

                var pickUp = prefab.AddComponent<Pickupable>();
                pickUp.randomizeRotationWhenDropped = true;
                pickUp.isPickupable = true;
                
                // Make the object drop slowly in water
                var wf = prefab.AddComponent<WorldForces>();
                wf.underwaterGravity = 0;
                wf.underwaterDrag = 10f;
                wf.enabled = true;
                wf.useRigidbody = rigidBody;
                
                //Renderer
                var renderer = prefab.GetComponentInChildren<Renderer>();

                // Update sky applier
                var applier = prefab.GetComponent<SkyApplier>();
                if (applier == null)
                    applier = prefab.AddComponent<SkyApplier>();
                applier.renderers = new Renderer[] { renderer };
                applier.anchorSky = Skies.Auto;

                var foodItem = prefab.AddComponent<Eatable>();
                foodItem.waterValue = _foodItemData.Water;
                foodItem.foodValue = _foodItemData.Food;

                var placeTool = prefab.AddComponent<PlaceTool>();
                placeTool.allowedInBase = true;
                placeTool.allowedOnBase = false;
                placeTool.allowedOnCeiling = false;
                placeTool.allowedOnConstructable = true;
                placeTool.allowedOnGround = true;
                placeTool.allowedOnRigidBody = true;
                placeTool.allowedOnWalls = false;
                placeTool.allowedOutside = true;
                placeTool.rotationEnabled = true;
                placeTool.enabled = true;
                placeTool.hasAnimations = false;
                placeTool.hasBashAnimation = false;
                placeTool.hasFirstUseAnimation = false;
                placeTool.mainCollider = prefab.GetComponentInChildren<Collider>();
                placeTool.pickupable = pickUp;
                placeTool.drawTime = 0.5f;
                placeTool.dropTime = 1;
                placeTool.holsterTime = 0.35f;

                // Set large world entity
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

                MaterialHelpers.ChangeEmissionColor(string.Empty, prefab, Color.cyan);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }

    internal struct PeeperBarFoodItemData
    {
        internal GameObject Prefab;
        internal string ClassId;
        internal string Friendly;
        internal string Description;
        internal decimal Cost;
        internal int Food;
        internal int Water;
    }
}
