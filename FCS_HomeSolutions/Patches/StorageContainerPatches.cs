namespace FCS_HomeSolutions.Patches
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
    //        if (pid != null && __instance.gameObject.transform.parent != null && __instance.gameObject.transform.parent.gameObject != null)
    //        {
    //            GameObject parentGO = __instance.gameObject.transform.parent.gameObject;
    //            PrefabIdentifier pid2 = parentGO.GetComponent<PrefabIdentifier>();
    //            if (pid2 != null && (parentGO.name.StartsWith("CabinetWide", true, CultureInfo.InvariantCulture) ||
    //                                 parentGO.name.StartsWith("CabinetTall", true, CultureInfo.InvariantCulture)||
    //                                 parentGO.name.StartsWith("CabinetMediumTall", true, CultureInfo.InvariantCulture)))
    //            {

    //                if (_storages.ContainsKey(pid2.Id))
    //                {
    //                    if (_storages[pid2.Id].Item2)
    //                    {
    //                        _storages[pid2.Id] = new Tuple<StorageContainer, bool>(__instance, false);
    //                    }
    //                    else
    //                    {
    //                        _storages[pid2.Id] = new Tuple<StorageContainer, bool>(_storages[pid2.Id].Item1, true);
    //                        StorageHelper.TransferItems(__instance.storageRoot.gameObject, _storages[pid2.Id].Item1.container);
    //                        GameObject.Destroy(__instance.gameObject);
    //                    }
    //                }
    //                else
    //                {
    //                    _storages.Add(pid2.Id, new Tuple<StorageContainer, bool>(__instance, false));
    //                }
    //            }
    //        }
    //    }
    //}
}
