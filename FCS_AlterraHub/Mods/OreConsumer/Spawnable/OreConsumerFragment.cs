using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.OreConsumer.Spawnable
{
    internal class OreConsumerFragment : SMLHelper.V2.Assets.Spawnable
    {
        public OreConsumerFragment() : base("OreConsumerFragmentPart","Ore Consumer Fragment","Fragment of an Ore Consumer Machine.")
        {
            OnFinishedPatching += () =>
            {
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawns(new List<SpawnInfo>
                {
                    //OreConsumersFrags
                    new SpawnInfo(TechType, new Vector3(75.5f, -318.7f, -1434.7f),
                        new Quaternion(-0.3f, 0.2f, 0.2f, 0.9f)),

                    new SpawnInfo(TechType, new Vector3(87.6f, -324.1f, -1454.3f),
                        new Quaternion(-0.3f, 0.2f, 0.2f, 0.9f)),

                    new SpawnInfo(TechType, new Vector3(77.1f, -318.9f, -1436.5f),
                        new Quaternion(-0.3f, 0.9f, -0.2f, 0.3f)),
                });
            };
        }


        public override WorldEntityInfo EntityInfo => new WorldEntityInfo() { cellLevel = LargeWorldEntity.CellLevel.Medium, classId = ClassID, localScale = Vector3.one, prefabZUp = false, slotType = EntitySlot.Type.Medium, techType = TechType };

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(AlterraHub.OreConsumerFragPrefab);

                PrefabIdentifier prefabIdentifier = prefab.EnsureComponent<PrefabIdentifier>();
                prefabIdentifier.ClassId = this.ClassID;

                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
 
                prefab.EnsureComponent<TechTag>().type = this.TechType;

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
                resourceTracker.techType = this.TechType;
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


#if SUBNAUTICA
        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Mod.GetIconPath(ClassID)));
        }
#elif BELOWZERO
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Mod.GetIconPath(ClassID));
        }
#endif
    }
}
