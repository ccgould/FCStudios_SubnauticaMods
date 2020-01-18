using System;
using UnityEngine;

namespace FCSCommon.Abstract
{
    internal abstract class FCSController : MonoBehaviour, IProtoEventListener, IConstructable
    {
        public abstract bool IsConstructed { get;}
        public virtual Action OnMonoUpdate { get; set; }
        public abstract bool IsInitialized { get; set; }

        public virtual void OnAddItemEvent(InventoryItem item) { }

        public virtual void OnRemoveItemEvent(InventoryItem item) { }

        public abstract void Initialize();

        public virtual string GetPrefabIDString()
        {
            throw new NotImplementedException();
        }

        public abstract void OnProtoSerialize(ProtobufSerializer serializer);

        public abstract void OnProtoDeserialize(ProtobufSerializer serializer);
        public abstract bool CanDeconstruct(out string reason);

        public abstract void OnConstructedChanged(bool constructed);

        public virtual string GetName()
        {
            return String.Empty;
        }

        public virtual void UpdateScreen(){}
    }
}
