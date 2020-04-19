using System;
using FCSCommon.Interfaces;
using FCSTechFabricator.Components;
using UnityEngine;

namespace FCSTechFabricator.Abstract
{
    public abstract class FCSController : MonoBehaviour, IProtoEventListener, IConstructable, IFCSController
    {

        public virtual Action OnMonoUpdate { get; set; }
        protected string _prefabId;


        public virtual string GetPrefabID()
        {
            if (_prefabId == String.Empty)
            {
                var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponentInChildren<PrefabIdentifier>();
                _prefabId = prefabIdentifier?.Id ?? string.Empty;
            }

            return _prefabId;
        }

        public virtual bool IsConstructed { get; set; }
        public bool IsInitialized { get; set; }
        
        public virtual void UpdateScreen(){}
        public virtual void NotifyMe<T>(T obj) where T : new()
        { }
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
