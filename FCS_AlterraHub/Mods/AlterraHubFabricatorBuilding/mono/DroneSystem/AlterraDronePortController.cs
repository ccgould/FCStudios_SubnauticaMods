using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mono;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class AlterraDronePortController : FcsDevice, IDroneDestination
    {
        public Transform BaseTransform { get; set; }
        public List<Transform> GetPaths()
        {
            return new List<Transform>();
        }

        public void Offload(Dictionary<TechType, int> order, Action onOffloadCompleted)
        {
            onOffloadCompleted?.Invoke();
        }

        public override void Awake()
        {
            base.Awake();
            BaseTransform = transform;
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanDeconstruct(out string reason)
        {
            throw new System.NotImplementedException();
        }

        public override void OnConstructedChanged(bool constructed)
        {
            throw new System.NotImplementedException();
        }
    }
}
