using System;
using System.Collections.Generic;
using AE.BaseTeleporter.Configuration;
using FCSCommon.Abstract;
using FCSCommon.Utilities;

namespace AE.BaseTeleporter.Managers
{
    internal class BaseManager
    {
        public static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal int InstanceID { get; }

        internal static readonly List<FCSController> GlobalUnits = new List<FCSController>();
        internal readonly List<FCSController> BaseUnits = new List<FCSController>();
        internal readonly SubRoot Habitat;
        internal Action OnBaseUnitsChanged;
        internal Action OnGlobalChanged;
        public BaseManager(SubRoot habitat)
        {
            Habitat = habitat;
            InstanceID = habitat.GetInstanceID();
        }

        internal static BaseManager FindManager(SubRoot subRoot)
        {
            QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()} || Name {subRoot.GetSubName()}");

            var pre = subRoot.gameObject.GetComponent<PrefabIdentifier>();

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
        
        internal void AddBaseUnit(FCSController baseUnit)
        {
            if (!BaseUnits.Contains(baseUnit))
            {
                BaseUnits.Add(baseUnit);
                UpdateGlobalTargets();
                QuickLogger.Debug($"Add {Mod.FriendlyName} Unit to Base List : {baseUnit.GetPrefabIDString()}", true);
                QuickLogger.Debug($"{Mod.FriendlyName} has been connected to base list Count {BaseUnits.Count}", true);
            }

            OnBaseUnitsChanged?.Invoke();
        }

        internal void RemoveBaseUnit(FCSController baseUnit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.BaseUnits.Contains(baseUnit)) continue;
                manager.BaseUnits.Remove(baseUnit);
                QuickLogger.Debug($"Removed Base {Mod.FriendlyName} : {baseUnit.GetPrefabIDString()}", true);
            }
            OnBaseUnitsChanged?.Invoke();
        }

        internal void UpdateGlobalTargets()
        {
            GlobalUnits.Clear();

            foreach (var manager in Managers)
            {
                foreach (var target in manager.BaseUnits)
                {
                    if (!GlobalUnits.Contains(target))
                    {
                        GlobalUnits.Add(target);
                    }
                }
            }

            OnGlobalChanged?.Invoke();
            QuickLogger.Debug($"Updated Global Targets Count : {GlobalUnits.Count}", true);
        }

        public void UpdateUnits()
        {
            foreach (FCSController baseUnit in BaseUnits)
            {
                baseUnit.UpdateScreen();
            }
        }
    }
}
