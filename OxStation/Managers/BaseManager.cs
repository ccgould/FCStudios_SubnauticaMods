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

        public static List<BaseManager> Managers { get; } = new List<BaseManager>();
        public int InstanceID { get; }

        internal readonly List<OxStationController> Units = new List<OxStationController>();
        internal readonly List<OxStationController> BaseUnits = new List<OxStationController>();
        public readonly SubRoot Habitat;

        /// <summary>
        /// Saves all the bases settings
        /// </summary>
        internal static void SaveBases()
        {
            QuickLogger.Debug("Save Bases");
            var saveDirectory = Mod.GetSaveFileDirectory();
            var SaveFile = Path.Combine(saveDirectory, "Bases.json");

            QuickLogger.Debug($"SD {saveDirectory} || SF {SaveFile}");

            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);


            var output = JsonConvert.SerializeObject(Managers, Formatting.Indented);
            File.WriteAllText(SaveFile, output);
        }

        public BaseManager(SubRoot habitat)
        {
            Habitat = habitat;
            InstanceID = habitat.GetInstanceID();
            var mono = habitat.gameObject.GetComponent<MonoBehaviour>();
            mono.StartCoroutine(AutoSystem());
        }

        public static BaseManager FindManager(SubRoot subRoot)
        {
            if (subRoot == null || !subRoot.isBase) return null;

            var manager = Managers.Find(x => x.InstanceID == subRoot.GetInstanceID() && x.Habitat == subRoot);

            if (manager == null)
            {
                QuickLogger.Debug("No manager found on base");
            }

            return manager ?? CreateNewManager(subRoot);
        }

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

        internal static void RemoveUnit(OxStationController unit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.Units.Contains(unit)) continue;
                manager.Units.Remove(unit);
                QuickLogger.Debug($"Removed Power Storage: {unit.GetPrefabIDString()}", true);
            }
        }

        internal void AddUnit(OxStationController unit)
        {
            if (!Units.Contains(unit) && unit.IsConstructed)
            {
                Units.Add(unit);
                QuickLogger.Debug($"Add Power Storage Unit : {unit.GetPrefabIDString()}", true);
            }
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
