using MAC.OxStation.Config;
using MAC.OxStation.Mono;
using Oculus.Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Utilities;
using UnityEngine;

namespace MAC.OxStation.Managers
{
    internal class BaseManager
    {
        internal static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal int InstanceID { get; }
        internal readonly List<OxStationController> BaseUnits = new List<OxStationController>();
        internal readonly SubRoot Habitat;

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
            QuickLogger.Debug($"Creating new manager", true);
            var manager = new BaseManager(habitat);
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            return manager;
        }

        private IEnumerator AutoSystem()
        {
            while (true)
            {


                yield return new WaitForSeconds(0.5f);
            }
        }

        internal BaseManager(SubRoot habitat)
        {
            Habitat = habitat;
            InstanceID = habitat.GetInstanceID();
            var mono = habitat.gameObject.GetComponent<MonoBehaviour>();
            mono.StartCoroutine(AutoSystem());
        }

        internal static BaseManager FindManager(SubRoot subRoot)
        {
            if (subRoot == null || !subRoot.isBase) return null;

            var manager = Managers.Find(x => x.InstanceID == subRoot.GetInstanceID() && x.Habitat == subRoot);

            if (manager == null)
            {
                QuickLogger.Debug("No manager found on base");
            }

            return manager ?? CreateNewManager(subRoot);
        }

        internal void AddBaseUnit(OxStationController unit)
        {
            if (!BaseUnits.Contains(unit))
            {
                BaseUnits.Add(unit);
                QuickLogger.Debug($"Add {Mod.FriendlyName} Unit to Base List : {unit.GetPrefabIDString()}", true);
                QuickLogger.Debug($"{Mod.FriendlyName} has been connected to base list Count {BaseUnits.Count}", true);
            }
        }

        internal static void RemoveBaseUnit(OxStationController unit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.BaseUnits.Contains(unit)) continue;
                manager.BaseUnits.Remove(unit);
                QuickLogger.Debug($"Removed Base Unit : {unit.GetPrefabIDString()}", true);
            }
        }
    }
}
