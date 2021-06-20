using System;
using System.Collections;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSDataBox.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubDepot.Spawnable
{
    internal class FCSDataBoxSpawnable : SMLHelper.V2.Assets.Spawnable
    {
        public FCSDataBoxSpawnable() : base("FCSDataBox", "FCS Data Box", "")
        {
            OnFinishedPatching += () =>
            {
                Mod.FCSDataBoxTechType = TechType;
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
                var prefab = GameObject.Instantiate(AlterraHub.DataBoxPrefab);

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

                //Pickupable pickupable = prefab.EnsureComponent<Pickupable>();
                //pickupable.isPickupable = false;

                //ResourceTracker resourceTracker = prefab.EnsureComponent<ResourceTracker>();
                //resourceTracker.prefabIdentifier = prefabIdentifier;
                //resourceTracker.techType = TechType;
                //resourceTracker.overrideTechType = TechType.Databox;
                //resourceTracker.rb = rb;
                //resourceTracker.pickupable = pickupable;

                prefab.EnsureComponent<GenericHandTarget>();
                prefab.EnsureComponent<FCSDataBoxController>();

                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
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
