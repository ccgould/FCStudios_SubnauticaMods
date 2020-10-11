using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Patches;
using FCSCommon.Utilities;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class BaseConnectable : MonoBehaviour, IProtoTreeEventListener
    {
        public ItemsContainer[] ItemsContainers => EasyCraft_Patch.Items;
        public BaseManager BaseManager { get; set; }
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving Data Storage Solutions");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved Data Storage Solutions");
            }
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            
        }

        public IEnumerable<ItemsContainer> GetServers()
        {
            foreach (ItemsContainer itemContiner in BaseManager.StorageManager.GetItemContiners())
            {
                yield return itemContiner;
            }
        }
    }
}
