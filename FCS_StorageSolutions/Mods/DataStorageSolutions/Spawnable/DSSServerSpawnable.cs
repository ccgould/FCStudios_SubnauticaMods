using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Server;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif


namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Spawnable
{
    internal class DSSServerSpawnable : SMLHelper.V2.Assets.Spawnable
    {
        public override string AssetsFolder => Mod.GetAssetFolder();

        public const string DSSServerTabID = "DSV";
        internal const string DSSServerFriendlyName = "Data Disk";
        internal const string DSSServerClassName = "DSSServer";
        internal const string DSSServerPrefabName = "DSS_ServerDataDisc";
        internal const string DSSServerDescription = "Data Storage for 48 items, formatted to accept all item categories. Place in a Wall Server Rack or Floor Server Rack to connect to Data Storage Network.";


        public DSSServerSpawnable() : base(DSSServerClassName, DSSServerFriendlyName, DSSServerDescription)
        {
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType, 1,90000, StoreCategory.Storage,true);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.DSSServerPrefab);

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

                prefab.SetActive(false);

                var storage = prefab.AddComponent<FCSStorage>();
                storage.Initialize(DSSServerClassName);
                prefab.SetActive(true);
                
                prefab.EnsureComponent<DSSServerController>();

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
