using FCS_DeepDriller.Buildable;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace FCS_DeepDriller.Ores
{
    internal class SandSpawnable : Spawnable
    {
        public SandSpawnable() : base("Sand_DD", "Sand Bag", "Sand can be used to make glass.")
        {
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(FCSDeepDrillerBuildable.SandPrefab);

            prefab.name = this.PrefabFileName;

            // We can pick this item
            var pickupable = prefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Make the object drop slowly in water
            var wf = prefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

            prefab.AddComponent<TechTag>().type = TechType;

            prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;

            return prefab;
        }

        public override string AssetsFolder => $"FCS_DeepDriller/Assets";

        private static readonly SandSpawnable Singleton = new SandSpawnable();

        internal static void PatchHelper()
        {
            Singleton.Patch();
        }
    }
}
