using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;


namespace DataStorageSolutions.Patches
{
    internal class EasyCraft_Patch
    {
        public static ItemsContainer[] Items { get; set; }

        public static void Find(ref ItemsContainer[] __result)
        {
            var useStorage = (int) QPatch.UseStorage.GetValue(QPatch.EasyCraftSettingsInstance);
            List<ItemsContainer> list = new List<ItemsContainer>();
            list.Add(Inventory.main.container);
            if (useStorage != 0)
            {
                if (useStorage == 1 && Player.main.IsInside())
                {
                    if (Player.main.IsInSub())
                    {
                        var baseConnectables = Player.main.currentSub.GetComponentsInChildren<BaseConnectable>();

                        foreach (BaseConnectable connectable in baseConnectables)
                        {
                            list.AddRange(connectable.GetServers());
                        }
                    }
                }
                else if (useStorage == 2)
                {
                    foreach (SubRoot subRoot in Object.FindObjectsOfType<SubRoot>())
                    {
                        Vector3 position = Player.main.transform.position;
                        BaseRoot baseRoot;
                        if ((subRoot.isCyclops && (position - subRoot.GetWorldCenterOfMass()).sqrMagnitude < 10000f) ||
                            (subRoot.isBase && (baseRoot = (subRoot as BaseRoot)) != null &&
                             baseRoot.GetDistanceToPlayer() < 100f))
                        {
                            foreach (BaseConnectable baseConnectable in
                                subRoot.GetComponentsInChildren<BaseConnectable>())
                            {
                                list.AddRange(baseConnectable.GetServers());
                            }
                        }
                    }
                }

                __result.AddRangeToArray((from x in list.Distinct<ItemsContainer>()
                    orderby (Player.main.transform.position - x.tr.position).sqrMagnitude
                    select x).ToArray<ItemsContainer>()) ;


                Items.AddRangeToArray(__result);
                QuickLogger.Debug($"Item Container count: {__result.Length}");
            }
        }
    }
}