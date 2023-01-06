using System;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Spawnables
{
    internal class PillPatch : Spawnable
    {
        private GameObject _prefab;
        public override string AssetsFolder => Mod.GetAssetFolder();
        public PillPatch(string classId, string friendlyName, string description, GameObject prefab) : base(classId, friendlyName, description)
        {
            _prefab = prefab;
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

                //Eatable
                var eatable = prefab.AddComponent<Eatable>();
                eatable.decomposes = false;
                eatable.waterValue = 0;
                eatable.foodValue = 1;
                eatable.despawns = true;

                //Renderer
                var renderer = prefab.GetComponentInChildren<Renderer>();

                prefab.AddComponent<PillComponent>();

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
    }

    internal class PillComponent : MonoBehaviour
    {
        internal float Amount { get; set; }

        private void Awake()
        {
            if (gameObject.name.StartsWith("Red"))
            {
                Amount = 120;
            }

            if (gameObject.name.StartsWith("Green"))
            {
                Amount = 60;
            }

            if (gameObject.name.StartsWith("Blue"))
            {
                Amount = 30;
            }
        }
    }
}
