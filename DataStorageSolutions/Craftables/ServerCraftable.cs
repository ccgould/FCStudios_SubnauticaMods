using System;
using System.Collections.Generic;
using System.IO;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Objects;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace DataStorageSolutions.Craftables
{
    internal class ServerCraftable : FcCraftable
    {
        public ServerCraftable(string classId, string friendlyName, string description, FcCraftingTab parentTab) : base(classId, friendlyName, description, parentTab)
        {
            OnFinishedPatching += () =>
            {
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {

                if (DSSModelPrefab.GetPrefabs())
                {
                    var prefab = GameObject.Instantiate(DSSModelPrefab.ServerPrefab);

                    prefab.name = this.PrefabFileName;

                    PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();
                    prefabID.ClassId = this.ClassID;

                    var techTag = prefab.EnsureComponent<TechTag>();
                    techTag.type = TechType;

                    var center = new Vector3(0f, 0.06423706f, 0.01254272f);
                    var size = new Vector3(0.3725086f, 0.06717008f, 0.4063118f);

                    GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                    //Make the rigid body isKinematic so it doesnt shake the cyclops
                    var rb = prefab.AddComponent<Rigidbody>();
                    rb.isKinematic = true;

                    // Set collider
                    var collider = prefab.GetComponentInChildren<Collider>();
                    collider.enabled = true;
                    collider.isTrigger = true;

                    QuickLogger.Debug("Collider");

                    // Make the object drop slowly in water
                    var wf = prefab.EnsureComponent<WorldForces>();
                    wf.underwaterGravity = 0;
                    wf.underwaterDrag = 10f;
                    wf.enabled = true;

                    // Add fabricating animation
                    var fabricatingA = prefab.EnsureComponent<VFXFabricating>();
                    fabricatingA.localMinY = -0.1f;
                    fabricatingA.localMaxY = 0.6f;
                    fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
                    fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
                    fabricatingA.scaleFactor = 1.0f;

                    //// Set proper shaders (for crafting animation)
                    var renderer = prefab.GetComponentInChildren<Renderer>();


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

                    //var storageRoot = GameObjectHelpers.FindGameObject(prefab, "StorageRoot");
                    //storageRoot.EnsureComponent<ChildObjectIdentifier>();

                    prefab.AddComponent<DSSServerController>();

                    //Add the FCSTechFabricatorTag component
                    prefab.AddComponent<FCSTechFabricatorTag>();

                    return prefab;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
            return null;
        }

#if SUBNAUTICA
        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{ClassID}.png"));
        }

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            return Mod.ServerIngredients;
        }
#elif BELOWZERO
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{ClassID}.png"));
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType

            return Mod.ServerIngredients;
        }

#endif
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => Mod.BundleName;
    }
}