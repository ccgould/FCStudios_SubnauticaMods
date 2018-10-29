using System.Collections.Generic;
using UnityEngine;

namespace FCSTerminal.Fixers
{
    public class StorageContainerFixer
    {
        public static Dictionary<string, StorageContainer> _storages = new Dictionary<string, StorageContainer>();

        public static void OnProtoDeserializeObjectTree_Postfix(StorageContainer __instance, ProtobufSerializer serializer)
        {
#if DEBUG_CARGO_CRATES
            Logger.Log("DEBUG: OnProtoDeserializeObjectTree()");
#endif
            PrefabIdentifier pid = __instance.gameObject.GetComponent<PrefabIdentifier>();
            if (pid != null && __instance.gameObject.transform.parent != null && __instance.gameObject.transform.parent.gameObject != null)
            {
                GameObject parentGO = __instance.gameObject.transform.parent.gameObject;
                PrefabIdentifier pid2 = parentGO.GetComponent<PrefabIdentifier>();
                if (pid2 != null && (parentGO.name.StartsWith("TestObject")))
                {
#if DEBUG_CARGO_CRATES
                    Logger.Log("DEBUG: OnProtoDeserializeObjectTree() storageConteiner Id=[" + pid2.Id + "] objName=[" + parentGO.name + "] nbItems=[" + (__instance.container != null ? Convert.ToString(__instance.container.count) : "null") + "]");
#endif
                    if (_storages.ContainsKey(pid2.Id))
                    {
#if DEBUG_CARGO_CRATES
                        Logger.Log("DEBUG: OnProtoDeserializeObjectTree() Transfering items...");
#endif
                        StorageHelper.TransferItems(__instance.storageRoot.gameObject, _storages[pid2.Id].container);
                        GameObject.Destroy(__instance.gameObject);
                    }
                    else
                    {
#if DEBUG_CARGO_CRATES
                        Logger.Log("DEBUG: OnProtoDeserializeObjectTree() Registering items...");
#endif
                        _storages.Add(pid2.Id, __instance);
                    }
                }
            }
        }
    }
}
