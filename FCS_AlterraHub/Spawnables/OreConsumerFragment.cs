using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Spawnables
{
    internal class OreConsumerFragment : Spawnable
    {
        public OreConsumerFragment() : base("OreConsumerFragment","Ore Consumer Fragment","Fragment of an Ore Consumer Machine.")
        {
            OnFinishedPatching += () => { Mod.OreConsumerFragmentTechType = TechType; };
        }

        public override List<LootDistributionData.BiomeData> BiomesToSpawnIn => GetBiomeDistribution();


        private List<LootDistributionData.BiomeData> GetBiomeDistribution()
        {
#if SUBNAUTICA
            List<LootDistributionData.BiomeData> biomeDatas = new List<LootDistributionData.BiomeData>()
            {
                new LootDistributionData.BiomeData(){ biome = BiomeType.JellyshroomCaves_CaveFloor, count = 1, probability = 1f },
            };
#elif BELOWZERO
            List<LootDistributionData.BiomeData> biomeDatas = new List<LootDistributionData.BiomeData>()
            {
                new LootDistributionData.BiomeData(){ biome = BiomeType.TwistyBridges_Ground, count = 1, probability = 1f },
            };
#endif
            return biomeDatas;
        }

        public override WorldEntityInfo EntityInfo => new WorldEntityInfo() { cellLevel = LargeWorldEntity.CellLevel.Medium, classId = ClassID, localScale = Vector3.one, prefabZUp = false, slotType = EntitySlot.Type.Medium, techType = TechType };

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(Buildables.AlterraHub.OreConsumerPrefab);

                PrefabIdentifier prefabIdentifier = prefab.AddComponent<PrefabIdentifier>();
                prefabIdentifier.ClassId = this.ClassID;
                prefab.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
                prefab.AddComponent<TechTag>().type = this.TechType;


                Pickupable pickupable = prefab.EnsureComponent<Pickupable>();
                pickupable.isPickupable = false;

                ResourceTracker resourceTracker = prefab.EnsureComponent<ResourceTracker>();
                resourceTracker.prefabIdentifier = prefabIdentifier;
                resourceTracker.techType = this.TechType;
                resourceTracker.overrideTechType = TechType.Fragment;
                resourceTracker.rb = prefab.GetComponent<Rigidbody>();
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
