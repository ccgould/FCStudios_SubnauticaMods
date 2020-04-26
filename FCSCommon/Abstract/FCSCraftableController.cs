using System;
using UnityEngine;

namespace FCSCommon.Abstract
{
    internal abstract class FCSCraftableController : MonoBehaviour, IProtoEventListener
    {
        public virtual Action OnMonoUpdate { get; set; }
        public abstract bool IsInitialized { get; set; }

        public abstract void Initialize();

        public virtual string GetPrefabIDString()
        {
            throw new NotImplementedException();
        }

        public abstract void OnProtoSerialize(ProtobufSerializer serializer);

        public abstract void OnProtoDeserialize(ProtobufSerializer serializer);
    }
}
