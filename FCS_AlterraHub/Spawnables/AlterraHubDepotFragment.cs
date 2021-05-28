using System;
using System.Collections;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using SMLHelper.V2.Assets;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Spawnables
{
    internal class AlterraHubDepotFragment : Spawnable
    {
        public AlterraHubDepotFragment() : base("AlterraHubDepotFragment", "AlterraHub Depot Fragment",
            "Fragment of an AlterraHub Depot Machine.")
        {
        }


        public override WorldEntityInfo EntityInfo => new WorldEntityInfo()
        {
            cellLevel = LargeWorldEntity.CellLevel.Medium, classId = ClassID, localScale = Vector3.one,
            prefabZUp = false, slotType = EntitySlot.Type.Medium, techType = TechType
        };

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(AlterraHub.AlterraHubDepotFragmentPrefab);

                PrefabIdentifier prefabIdentifier = prefab.AddComponent<PrefabIdentifier>();
                prefabIdentifier.ClassId = this.ClassID;
                prefab.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
                prefab.AddComponent<TechTag>().type = this.TechType;

                var rb = prefab.GetComponentInChildren<Rigidbody>();

                if (rb == null)
                {
                    rb = prefab.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                }

                Pickupable pickupable = prefab.AddComponent<Pickupable>();
                pickupable.isPickupable = false;

                ResourceTracker resourceTracker = prefab.AddComponent<ResourceTracker>();
                prefab.AddComponent<AlterraHubDepotFragmentSpawn>();
                resourceTracker.prefabIdentifier = prefabIdentifier;
                resourceTracker.techType = this.TechType;
                resourceTracker.overrideTechType = TechType.Fragment;
                resourceTracker.rb = rb;
                resourceTracker.pickupable = pickupable;
                return prefab;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> oreConsumerFragment)
        {
            oreConsumerFragment.Set(GetGameObject());
            yield break;
        }
    }

    internal class AlterraHubDepotFragmentSpawn:MonoBehaviour
    {
    }
}
