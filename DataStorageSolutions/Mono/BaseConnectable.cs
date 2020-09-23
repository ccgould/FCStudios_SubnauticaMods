using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class BaseConnectable : MonoBehaviour, IProtoTreeEventListener
    {
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving Data Storage Solutions");
                Mod.Save();
                QuickLogger.Info($"Saved Data Storage Solutions");
            }
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            
        }
    }
}
