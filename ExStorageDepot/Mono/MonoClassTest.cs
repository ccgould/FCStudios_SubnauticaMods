using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Utilities;
using UnityEngine;

namespace ExStorageDepot.Mono
{
    internal class MonoClassTest : MonoBehaviour, IProtoEventListener,IConstructable
    {
        private void Awake()
        {
            QuickLogger.Debug("Awake");            
        }

        private void OnEnable()
        {
            QuickLogger.Debug("OnEnable");
        }

        private void Start()
        {
            QuickLogger.Debug("Start");
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("OnProtoSerialize");
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("OnProtoDeserialize");
        }

        public bool CanDeconstruct(out string reason)
        {
            QuickLogger.Debug("CanDeconstruct");
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Debug("OnConstructedChanged");
        }

        private void OnDrawGizmos()
        {
            QuickLogger.Debug("OnDrawGizmos");
        }

        private void OnDestroy()
        {
            QuickLogger.Debug("OnDestroy");
        }
    }
}
