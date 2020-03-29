using System;
using FCSCommon.Interfaces;
using UnityEngine;

namespace FCSTechFabricator.Abstract
{
    public abstract class FCSController : MonoBehaviour, IProtoEventListener, IConstructable, IFCSController
    {

        public virtual Action OnMonoUpdate { get; set; }

        public virtual bool IsConstructed { get; set; }
        public bool IsInitialized { get; set; }

        public virtual void OnAddItemEvent(InventoryItem item) { }

        public virtual void OnRemoveItemEvent(InventoryItem item) { }
        
        public virtual string GetPrefabIDString()
        {
            throw new NotImplementedException();
        }

        public virtual void OnDestroy()
        {
            throw new NotImplementedException();
        }

        public virtual string GetName()
        {
            return String.Empty;
        }

        public virtual void UpdateScreen(){}
        public virtual void OnProtoSerialize(ProtobufSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public virtual void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanDeconstruct(out string reason)
        {
            throw new NotImplementedException();
        }

        public virtual void OnConstructedChanged(bool constructed)
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
