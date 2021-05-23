using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Spawnables
{
    internal class PaintCanSpawnable : Spawnable
    {
        public override string AssetsFolder => Mod.GetAssetPath();

        public PaintCanSpawnable() : base(Mod.PaintCanClassID, Mod.PaintCanFriendly, Mod.PaintCanDescription)
        {
            OnFinishedPatching += () =>
            { 
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType, 7500, StoreCategory.Home);
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.PaintCanPrefab);

                prefab.AddComponent<PrefabIdentifier>();
                prefab.AddComponent<TechTag>().type = TechType;

                var pickUp = prefab.AddComponent<Pickupable>();
                pickUp.randomizeRotationWhenDropped = true;
                pickUp.isPickupable = true;

                var rigidBody = prefab.EnsureComponent<Rigidbody>();

                // Make the object drop slowly in water
                var wf = prefab.AddComponent<WorldForces>();
                wf.underwaterGravity = 0;
                wf.underwaterDrag = 10f;
                wf.enabled = true;
                wf.useRigidbody = rigidBody;

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
                placeTool.pickupable = pickUp;
                placeTool.drawTime = 0.5f;
                placeTool.dropTime = 1;
                placeTool.holsterTime = 0.35f;

                //Renderer
                var renderer = prefab.GetComponentInChildren<Renderer>();

                // Update sky applier
                var applier = prefab.GetComponent<SkyApplier>();
                if (applier == null)
                    applier = prefab.AddComponent<SkyApplier>();
                applier.renderers = new Renderer[] { renderer };
                applier.anchorSky = Skies.Auto;

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
}
