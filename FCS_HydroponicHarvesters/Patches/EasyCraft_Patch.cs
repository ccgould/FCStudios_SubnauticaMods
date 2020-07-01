using System.Collections.Generic;
using System.Linq;
using FCS_HydroponicHarvesters.Extensions;
using FCS_HydroponicHarvesters.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Patches
{
    internal class EasyCraft_Patch
    {
        private static float _cacheExpired;
        private static HydroHarvController[] _cached;
        private const float CacheDuration = 1f;


        private static HydroHarvController[] Find()
        {
            var usageStorage = (int)QPatch.UseStorage.GetValue(QPatch.EasyCraftSettingsInstance);

            HashSet<HydroHarvController> list = new HashSet<HydroHarvController>();

            if (usageStorage != 0)
            {
                if (usageStorage == 1 && Player.main.IsInside())
                {
                    if (Player.main.IsInSub())
                    {
                        list = new HashSet<HydroHarvController>(Player.main.currentSub.GetComponentsInChildren<HydroHarvController>());
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
                            foreach (HydroHarvController hydroHarvController in subRoot.GetComponentsInChildren<HydroHarvController>())
                            {
                                if (hydroHarvController.IsConstructed)
                                {
                                    list.Add(hydroHarvController);
                                }
                            }
                        }
                    }
                }

                return (from x in list.Distinct<HydroHarvController>()
                    orderby (Player.main.transform.position - x.transform.position).sqrMagnitude
                    select x).ToArray<HydroHarvController>();
            }
            return list.ToArray();
        }

        private static HydroHarvController[] Containers
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
            var hydroHarvControllers = Containers;

            //QuickLogger.Debug($"Hydroponic Harvesters Count: {hydroHarvControllers.Length}");

            if (hydroHarvControllers.Length == 0)
            {
                __result += 0;
            }
            else
            {
                for (int i = 0; i < hydroHarvControllers.Length; i++)
                {
                    var hydroHarvController = hydroHarvControllers[i];
                    if (hydroHarvController != null)
                    {
                        if (hydroHarvController.IsOperational)
                        {
                            __result += hydroHarvController.HydroHarvContainer.GetItemCount(techType);
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

                for (var index = 0; index < Containers.Length; index++)
                {
                    var harvController = Containers[index];
                    
                    for (int i = 0; i < count; i++)
                    {

                        if(harvController.HydroHarvContainer.HasItems() && harvController.HydroHarvContainer.ContainsItem(techType))
                        {
                            harvController.HydroHarvContainer.RemoveItemFromContainerOnly(techType);
                            num++;
                        }
                    }

                    if (num == count)
                    {
                        __result = true;
                        QuickLogger.Info($"[EasyCraft] removed {count} {techType} from a Hydroponic Harvester");
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