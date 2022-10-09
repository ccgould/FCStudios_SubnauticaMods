using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubPod.Spawnable
{
    internal class AlterraHubPatch : SMLHelper.V2.Assets.Spawnable
    {
        public AlterraHubPatch() : base(Mod.AlterraHubStationClassID, Mod.AlterraHubStationFriendly, Mod.AlterraHubStationDescription)
        {
            OnFinishedPatching += () =>
            {
                var spawnLocation = new Vector3(-110f, -27.33f, 557.4f);
                var spawnRotation = Quaternion.Euler(Vector3.zero);
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

            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID, 1f, 2f, 0.2f);

            //prefab.AddComponent<AlterraFabricatorStationController>();
            prefab.AddComponent<PortManager>();

            return prefab;
        }
    }
}
