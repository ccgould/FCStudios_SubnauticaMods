using System;
using FCSCommon.Helpers;
using FCSDemo.Buildables;
using FCSTechFabricator.Abstract;
using Model;
using UnityEngine;

namespace Mono
{
    internal class FCSDemoController : FCSController,IHandTarget
    {
        private bool _runStartUpOnEnable;

        internal void ChangeColor(Vector3 vec3)
        {
            MaterialHelpers.ChangeMaterialColor(FCSDemoModel.BodyMaterial,gameObject,Color.white,new Color(vec3.x,vec3.y,vec3.z),Color.white);
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();

                }
                _runStartUpOnEnable = false;
            }

            IsInitialized = true;
        }


        public override void Initialize()
        {
            GetPrefabID();
        }

        public void Save(SaveData newSaveData)
        {

        }

        public override string GetPrefabID()
        {
            if (string.IsNullOrEmpty(_prefabId))
            {
                var prefabIdentifier = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                _prefabId = prefabIdentifier?.Id ?? string.Empty;
            }

            return _prefabId;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }

                IsInitialized = true;
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            
        }

        public override bool CanDeconstruct(out string reason)
        {
           reason = String.Empty;
           return true;
        }

        public string Name => gameObject.name;
        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractTextRaw($"Item PrefabID: {GetPrefabID()}","");
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}
