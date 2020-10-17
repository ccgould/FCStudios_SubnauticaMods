using System;
using FCS_AlterraHub.Buildables;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Spawnables
{
    internal  class FCSKit : Spawnable
    {
        private readonly string _iconPath;

        public FCSKit(string classId, string friendlyName,string iconPath) : base(classId, friendlyName, $"A kit that allows you to build one {friendlyName} Unit")
        {
            _iconPath = iconPath;
            OnFinishedPatching += () =>
            {
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }
        
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(AlterraHub.KitPrefab);

                prefab.name = this.PrefabFileName;

                PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();

                prefabID.ClassId = this.ClassID;

                var techTag = prefab.EnsureComponent<TechTag>();
                techTag.type = TechType;

                var center = new Vector3(0f, 0.2518765f, 0f);
                var size = new Vector3(0.5021304f, 0.5062426f, 0.5044461f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                
                ApplyKitComponents(prefab);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }
        
        private void ApplyKitComponents(GameObject prefab)
        {
            // Set collider
            var collider = prefab.GetComponentInChildren<Collider>();

            // Make the object drop slowly in water
            var wf = prefab.EnsureComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

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

            //Allow this kit to be placed on surfaces in these situations
            var placeTool = prefab.EnsureComponent<PlaceTool>();
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
            placeTool.mainCollider = collider;
            placeTool.pickupable = pickupable;
            placeTool.drawTime = 0.5f;
            placeTool.dropTime = 1;
            placeTool.holsterTime = 0.35f;
            prefab.GetComponentInChildren<Text>().text = FriendlyName;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(_iconPath));
        }
    }
}
