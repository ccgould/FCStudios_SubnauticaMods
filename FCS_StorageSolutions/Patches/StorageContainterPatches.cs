using System;
using System.Collections.Generic;
using System.Globalization;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_StorageSolutions.Patches
{
    //[HarmonyPatch(typeof(StorageContainer))]
    //[HarmonyPatch("OnProtoDeserializeObjectTree")]
    //public class StorageContainerFixer
    //{
    //    public static Dictionary<string, Tuple<StorageContainer, bool>> _storages = new Dictionary<string, Tuple<StorageContainer, bool>>();

    //    [HarmonyPostfix]
    //    public static void OnProtoDeserializeObjectTree_Postfix(StorageContainer __instance, ProtobufSerializer serializer)
    //    {
    //        PrefabIdentifier pid = __instance.gameObject.GetComponent<PrefabIdentifier>();
    //        if (pid != null && __instance.gameObject != null)
    //        {
    //            GameObject parentGO = __instance.gameObject;
    //            QuickLogger.Debug($"parent name :{parentGO.name}");
    //            PrefabIdentifier pid2 = parentGO.GetComponent<PrefabIdentifier>();
    //            if (pid2 != null && (parentGO.name.StartsWith(Mod.AlterraStorageClassName, true, CultureInfo.InvariantCulture)))
    //            {
    //                QuickLogger.Debug($"Found AlterraStorage: {pid2.Id}");

    //                if (_storages.ContainsKey(pid2.Id))
    //                {
    //                    QuickLogger.Debug($"Found AlterraStorage: {pid2.Id}");

    //                    if (_storages[pid2.Id].Item2)
    //                    {
    //                        QuickLogger.Debug($"Storages contains Item2 : {pid2.Id}");
    //                        _storages[pid2.Id] = new Tuple<StorageContainer, bool>(__instance, false);
    //                    }
    //                    else
    //                    {
    //                        QuickLogger.Debug($"Transferring Storage: {pid2.Id}");
    //                        _storages[pid2.Id] = new Tuple<StorageContainer, bool>(_storages[pid2.Id].Item1, true);
    //                        StorageHelper.TransferItems(__instance.storageRoot.gameObject, _storages[pid2.Id].Item1.container);
    //                        GameObject.Destroy(__instance.gameObject);
    //                    }
    //                }
    //                else
    //                {
    //                    QuickLogger.Debug($"Add new Storage: {pid2.Id}");
    //                    _storages.Add(pid2.Id, new Tuple<StorageContainer, bool>(__instance, false));
    //                }
    //            }
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(UniqueIdentifier))]
    //[HarmonyPatch("Register")]
    //public class UniqueIdentifier_Register_Patch
    //{
    //    [HarmonyPrefix]
    //    public static void UniqueIdentifierRegister_Prefix(UniqueIdentifier __instance)
    //    {
    //        string text = __instance.id;
    //        if (string.IsNullOrEmpty(text))
    //        {
    //            return;
    //        }

    //        var d = __instance.identifiers.TryGetValue(text, out var uniqueIdentifier);
    //        if (uniqueIdentifier)
    //        {
    //            QuickLogger.Debug($"UPosition: {uniqueIdentifier.transform}");
    //            QuickLogger.Debug($"Position: {__instance.transform}");

    //        }
    //    }
    //}
}
