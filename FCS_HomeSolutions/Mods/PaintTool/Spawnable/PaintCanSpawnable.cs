﻿using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.Handlers;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.PaintTool.Spawnable
{
    internal class PaintCanSpawnable : SMLHelper.Assets.Spawnable
    {
        private readonly GameObject _prefab;
        public override string AssetsFolder => Mod.GetAssetPath();
        internal const string PaintCanClassID = "PaintCan";
        internal const string PaintCanFriendly = "Paint Can";
        internal const string PaintCanDescription = "Plug into your Paint Tool to color FCStudios Appliances, Furniture, Machines, and LED Lights";
        internal const string PaintCanPrefabName = "FCS_PaintCan";


        public PaintCanSpawnable() : base(PaintCanClassID, PaintCanFriendly, PaintCanDescription)
        {
            _prefab = ModelPrefab.GetPrefab(PaintCanPrefabName);
            OnFinishedPatching += () =>
            { 
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType, 1,7500, StoreCategory.Home,true);
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

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
