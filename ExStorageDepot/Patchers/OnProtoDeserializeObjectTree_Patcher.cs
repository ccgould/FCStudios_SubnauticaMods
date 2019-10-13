using FCSCommon.Utilities;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExStorageDepot.Patchers
{
    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch("OnProtoDeserializeObjectTree")]
    internal class OnProtoDeserializeObjectTree_Patcher
    {
        public static Dictionary<string, StorageContainer> _storages = new Dictionary<string, StorageContainer>();

        [HarmonyPostfix]
        public static void Postfix(ref StorageContainer __instance, ProtobufSerializer serializer)
        {
            QuickLogger.Debug("OnProtoDeserializeObjectTree()");
            PrefabIdentifier pid = __instance.gameObject.GetComponent<PrefabIdentifier>();


            if (pid != null && __instance.gameObject.transform.parent != null &&
                __instance.gameObject.transform.parent.gameObject != null)
            {
                QuickLogger.Debug($"PID {pid?.Id} || Parent {__instance?.gameObject.transform.parent} || PGO {__instance.gameObject.transform.parent.gameObject.name}");

                GameObject parentGO = __instance.gameObject.transform.parent.gameObject;
                PrefabIdentifier pid2 = parentGO.GetComponent<PrefabIdentifier>();

                if (pid2 != null && parentGO.name.StartsWith("ExStorageDepot"))
                {
                    QuickLogger.Debug("OnProtoDeserializeObjectTree() storageContainer Id=[" + pid2.Id + "] objName=[" + parentGO.name + "] nbItems=[" + (__instance.container != null ? Convert.ToString(__instance.container.count) : "null") + "]");
                    if (_storages.ContainsKey(pid2.Id))
                    {
                        QuickLogger.Debug($"Transferring items and destroying {pid2.Id}");

                        StorageHelper.TransferItems(__instance.storageRoot.gameObject, _storages[pid2.Id].container);
                        GameObject.Destroy(__instance.gameObject);
                    }
                    else
                    {
                        QuickLogger.Debug("OnProtoDeserializeObjectTree() Registering items...");
                        _storages.Add(pid2.Id, __instance);
                    }
                }
            }
        }
    }
}