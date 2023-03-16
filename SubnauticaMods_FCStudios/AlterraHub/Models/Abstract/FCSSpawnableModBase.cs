using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Structs;
using FCSCommon.Helpers;
using SMLHelper.Utility;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract
{
    public abstract class FCSSpawnableModBase : SMLHelper.Assets.Spawnable
    {
        private GameObject _prefab;
        private FCSModItemSettings _settings;
        public override string AssetsFolder { get; }

        public GameObject Prefab { get; private set; }

        protected FCSSpawnableModBase(FCSModItemSettings settings) : base(settings.ClassId, settings.FriendlyName, settings.Description)
        {
            AssetsFolder = ModRegistrationService.GetModPackData(settings.MODNAME)?.GetAssetPath();
            _prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(settings.PrefabName, ModRegistrationService.GetModPackData(Main.MODNAME)?.GetBundleName(),FileSystemHelper.ModDirLocation);
            _settings = settings;
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);

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
            placeTool.allowedInBase = _settings.AllowedInBase;
            placeTool.allowedOnBase = _settings.AllowedOnBase;
            placeTool.allowedOnCeiling = _settings.AllowedOnCeiling;
            placeTool.allowedOnConstructable = _settings.AllowedOnConstructables;
            placeTool.allowedOnGround = _settings.AllowedOnGround;
            placeTool.allowedOnRigidBody = _settings.AllowOnRigidBody;
            placeTool.allowedOnWalls = _settings.AllowedOnWall;
            placeTool.allowedOutside = _settings.AllowedOutside;
            placeTool.rotationEnabled = _settings.RotationEnabled;
            placeTool.enabled = true;
            placeTool.hasAnimations = _settings.HasAnimations;
            placeTool.hasBashAnimation = _settings.HasBashAnimation;
            placeTool.hasFirstUseAnimation = _settings.HasFirstAnimation;
            placeTool.mainCollider = collider;
            placeTool.pickupable = pickUp;
            placeTool.drawTime = _settings.DrawTime;
            placeTool.dropTime = _settings.DropTime;
            placeTool.holsterTime = _settings.HolsterTime;

            //Renderer
            var renderer = prefab.GetComponentInChildren<Renderer>();

            // Update sky applier
            var applier = prefab.GetComponent<SkyApplier>();
            if (applier == null)
                applier = prefab.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            Prefab = prefab;

            gameObject.Set(prefab);
            yield break;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
