using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubDepot.Spawnable
{
    internal class AlterraHubDepotFragment : SMLHelper.V2.Assets.Spawnable
    {
        public AlterraHubDepotFragment() : base("AlterraHubDepotFragmentPart", "AlterraHub Depot Fragment",
            "Fragment of an AlterraHub Depot Machine.")
        {
            OnFinishedPatching += () =>
            {
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawns(new List<SpawnInfo>
                {
                    //AlterraDepot
                    new SpawnInfo(TechType, new Vector3(80.7f, -322.9f, -1448.0f),
                        new Quaternion(-0.4f, 0.5f, 0.1f, 0.8f)),

                    new SpawnInfo(TechType, new Vector3(82.6f, -322.1f, -1446.5f),
                        new Quaternion(-0.6f, 0.6f, 0.5f, 0.1f)),

                    new SpawnInfo(TechType, new Vector3(81.3f, -323.1f, -1448.8f),
                        new Quaternion(-0.4f, 0.5f, 0.1f, 0.8f)),

                    new SpawnInfo(TechType, new Vector3(85.0f, -320.1f, -1446.0f),
                        new Quaternion(-0.6f, 0.6f, 0.5f, 0.1f)),

                    new SpawnInfo(TechType, new Vector3(80.7f, -321.0f, -1445.2f),
                        new Quaternion(-0.3f, 0.9f, -0.2f, 0.2f)),

                    new SpawnInfo(TechType, new Vector3(79.9f, -322.6f, -1446.8f),
                        new Quaternion(-0.5f, 0.4f, -0.1f, 0.8f)),
                });
            };
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

                PrefabIdentifier prefabIdentifier = prefab.EnsureComponent<PrefabIdentifier>();
                prefabIdentifier.ClassId = this.ClassID;
                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
                prefab.EnsureComponent<TechTag>().type = TechType;

                var rb = prefab.GetComponentInChildren<Rigidbody>();

                if (rb == null)
                {
                    rb = prefab.EnsureComponent<Rigidbody>();
                    rb.isKinematic = true;
                }

                Pickupable pickupable = prefab.EnsureComponent<Pickupable>();
                pickupable.isPickupable = false;

                ResourceTracker resourceTracker = prefab.EnsureComponent<ResourceTracker>();
                resourceTracker.prefabIdentifier = prefabIdentifier;
                resourceTracker.techType = TechType;
                resourceTracker.overrideTechType = TechType.Fragment;
                resourceTracker.rb = rb;
                resourceTracker.pickupable = pickupable;
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
            }

            return null;
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> oreConsumerFragment)
        {
            oreConsumerFragment.Set(GetGameObject());
            yield break;
        }
    }
}
