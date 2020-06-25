using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using UnityEngine;


namespace DataStorageSolutions.Patches
{
    internal class EasyCraft_Patch
    {
        private static float _cacheExpired;
        private const float CacheDuration = 1f;
        private static BaseManager[] _cached = new BaseManager[0];

        private static BaseManager[] Find()
        {
            var usageStorage = (int)QPatch.UseStorage.GetValue(QPatch.EasyCraftSettingsInstance);

            HashSet<BaseManager> list = new HashSet<BaseManager>();

            if (usageStorage != 0)
            {
                if (usageStorage == 1 && Player.main.IsInside())
                {
                    if (Player.main.IsInSub())
                    {
                        list.Add(BaseManager.FindManager(Player.main.currentSub));
                    }
                }
                else if (usageStorage == 2)
                {
                    foreach (SubRoot subRoot in GameObject.FindObjectsOfType<SubRoot>())
                    {
                        Vector3 position = Player.main.transform.position;
                        BaseRoot baseRoot;
                        if ((subRoot.isCyclops && (position - subRoot.GetWorldCenterOfMass()).sqrMagnitude < 10000f) || (subRoot.isBase && (baseRoot = (subRoot as BaseRoot)) != null && baseRoot.GetDistanceToPlayer() < 100f))
                        {
                            list.Add(BaseManager.FindManager(subRoot));
                        }
                    }
                }

                return (from x in list.Distinct<BaseManager>()
                    orderby (Player.main.transform.position - x.Habitat.transform.position).sqrMagnitude
                    select x).ToArray<BaseManager>();
            }
            return list.ToArray();
        }

        private static BaseManager[] Containers
        {
            get
            {
                if (_cacheExpired < Time.unscaledTime || _cacheExpired > Time.unscaledTime + CacheDuration)
                {
                    _cached = Find();
                    _cacheExpired = Time.unscaledTime + CacheDuration;
                }
                return _cached;
            }
        }
        
        public static void GetPickupCount(TechType techType, ref int __result)
        {
            var bases = Containers;

            QuickLogger.Debug($"Bases Count: {bases.Length}");

            if (bases.Length == 0)
            {
                __result += 0;
            }
            else
            {
                for (int i = 0; i < bases.Length; i++)
                {
                    var baseManager = bases[i];
                    if (baseManager != null)
                    {
                        if (baseManager.IsOperational)
                        {
                            __result += baseManager.GetItemCount(techType);
                        }
                    }
                    else
                    {
                        __result += 0;
                    }
                }
                
            }
            
        }

        public static void DestroyItem(TechType techType, ref bool __result, ref int count)
        {
            QuickLogger.Debug("Attempting to take items");

            if (!__result)
            {
                int num = 0;

                var baseManager = Containers;
                
                for (var index = 0; index < Containers.Length; index++)
                {
                    var currentBase = Containers[index];

                    QuickLogger.Debug($"Attempting to take items from base {currentBase?.GetBaseName()}");


                    for (int i = 0; i < count; i++)
                    {
                        var server = currentBase?.GetServerWithItem(techType);

                        if (server != null)
                        {
                            server.Remove(techType);
                            num++;
                        }
                    }

                    if (num == count)
                    {
                        __result = true;
                        QuickLogger.Info($"[EasyCraft] removed {count} {techType} from base {currentBase.GetBaseName()}");
                    }
                }


                QuickLogger.Debug($"Count: {count}");

                if (num >= count) return;
                QuickLogger.Info($"[EasyCraft] Unable to remove {count} {techType}");
                __result = false;
            }
        }
    }
}