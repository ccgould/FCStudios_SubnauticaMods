using System;
using System.Collections.Generic;
using System.Globalization;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Patches
{
    //[HarmonyPatch(typeof(StorageContainer))]
    //[HarmonyPatch("OnProtoDeserializeObjectTree")]
    //public class Storage_Patcher
    //{
    //    public static Dictionary<string, Tuple<StorageContainer, bool>> _storages = new Dictionary<string, Tuple<StorageContainer, bool>>();

    //    [HarmonyPostfix]
    //    public static void OnProtoDeserializeObjectTree_Postfix(StorageContainer __instance, ProtobufSerializer serializer)
    //    {

    //        QuickLogger.Debug("OnProtoDeserializeObjectTree Patch");

    //        PrefabIdentifier pid = __instance.gameObject.GetComponent<PrefabIdentifier>();
    //        if (pid != null && __instance.gameObject.transform.parent != null && __instance.gameObject.transform.parent.gameObject != null)
    //        {

    //            QuickLogger.Debug("OnProtoDeserializeObjectTree() storageContainer Id=[" + pid.Id + "] objName=[" + __instance.gameObject.name + "] nbItems=[" + (__instance.container != null ? Convert.ToString(__instance.container.count) : "null") + "]");

    //            GameObject parentGO = __instance.gameObject;
    //            QuickLogger.Debug($"OnProtoDeserializeObjectTree() ParentGO: [{parentGO?.name}]");

    //            PrefabIdentifier pid2 = parentGO.GetComponent<PrefabIdentifier>();
    //            QuickLogger.Debug($"OnProtoDeserializeObjectTree() ParentID2: [{pid2?.Id}] Name Match Case: [{parentGO.name.StartsWith(Mod.SmartPlanterPotClassID, true, CultureInfo.InvariantCulture)}]");

    //            if (pid2 != null && (parentGO.name.StartsWith(Mod.SmartPlanterPotClassID, true, CultureInfo.InvariantCulture)))
    //            {
    //                QuickLogger.Debug("OnProtoDeserializeObjectTree() parent storageContainer Id=[" + pid2.Id + "] objName=[" + parentGO.name + "] nbItems=[" + (__instance.container != null ? Convert.ToString(__instance.container.count) : "null") + "]");
    //                if (_storages.ContainsKey(pid2.Id))
    //                {
    //                    if (_storages[pid2.Id].Item2)
    //                    {
    //                        QuickLogger.Debug("OnProtoDeserializeObjectTree() Setup A"); // Resetting

    //                        _storages[pid2.Id] = new Tuple<StorageContainer, bool>(__instance, false);
    //                    }
    //                    else
    //                    {
    //                        QuickLogger.Debug("OnProtoDeserializeObjectTree() Setup B"); // Transfering
    //                        _storages[pid2.Id] = new Tuple<StorageContainer, bool>(_storages[pid2.Id].Item1, true);
    //                        StorageHelper.TransferItems(__instance.storageRoot.gameObject, _storages[pid2.Id].Item1.container);
    //                        GameObject.Destroy(__instance.gameObject);
    //                    }
    //                }
    //                else
    //                {
    //                    QuickLogger.Debug("OnProtoDeserializeObjectTree() Setup C"); // Registering

    //                    _storages.Add(pid2.Id, new Tuple<StorageContainer, bool>(__instance, false));
    //                }
    //            }
    //        }
    //    }
    //}
}
