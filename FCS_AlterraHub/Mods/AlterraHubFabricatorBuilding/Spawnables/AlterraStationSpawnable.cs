using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Spawnables
{
    internal class AlterraStationSpawnable : Spawnable
    {
        public AlterraStationSpawnable() : base(Mod.AlterraHubStationClassID, Mod.AlterraHubStationFriendly, Mod.AlterraHubStationDescription)
        {
            OnFinishedPatching += () =>
            {
                var spawnLocation = new Vector3(82.70f, -316.9f, -1434.7f);
                var spawnRotation = Quaternion.Euler(348.7f, 326.24f, 43.68f);
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType, spawnLocation, spawnRotation));
            };
        }

        public override WorldEntityInfo EntityInfo => new()
        {
            classId = ClassID,
            cellLevel = LargeWorldEntity.CellLevel.Global,
            localScale = Vector3.one,
            slotType = EntitySlot.Type.Large,
            techType = this.TechType
        };
        
        public override GameObject GetGameObject()
        {
            var prefab = GameObject.Instantiate(AlterraHub.AlterraHubFabricatorPrefab);
            
            prefab.SetActive(false);

            prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            prefab.EnsureComponent<PrefabIdentifier>().classId = ClassID;
            prefab.EnsureComponent<ImmuneToPropulsioncannon>();
            prefab.EnsureComponent<TechTag>().type = TechType;
            prefab.EnsureComponent<SkyApplier>().renderers = prefab.GetComponentsInChildren<Renderer>();
            
            var rb = prefab.EnsureComponent<Rigidbody>();
            rb.mass = 10000f;
            rb.isKinematic = true;

            foreach (Collider col in prefab.GetComponentsInChildren<Collider>())
            {
                col.gameObject.EnsureComponent<ConstructionObstacle>();
            }

            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            
            prefab.AddComponent<AlterraFabricatorStationController>();
            prefab.AddComponent<PortManager>();
            
            return prefab;
        }
    }
}