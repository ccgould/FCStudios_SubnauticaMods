using FCSAlterraShipping.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;

namespace FCSAlterraShipping.Models
{
    internal class ShippingTargetManager
    {
        internal static readonly List<ShippingTargetManager> Managers = new List<ShippingTargetManager>();
        internal readonly List<AlterraShippingTarget> ShippingTargets = new List<AlterraShippingTarget>();
        internal static readonly List<AlterraShippingTarget> GlobalShippingTargets = new List<AlterraShippingTarget>();
        public readonly int InstanceID;
        public readonly SubRoot Habitat;
        public static Action GlobalChanged;

        public ShippingTargetManager(SubRoot habitat)
        {
            Habitat = habitat;
            InstanceID = habitat.GetInstanceID();
        }

        public static int MaxShippingContainers { get; } = 1;

        public static ShippingTargetManager FindManager(SubRoot subRoot)
        {
            if (subRoot.isBase || subRoot.isCyclops) //Changed To allow in cyclops
            {
                QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()}");

                var manager = Managers.Find(x => x.InstanceID == subRoot.GetInstanceID() && x.Habitat == subRoot);

                return manager ?? CreateNewManager(subRoot);
            }

            return null;
        }

        internal int GetShippingTargetCount()
        {
            return GlobalShippingTargets.Count;
        }

        private static ShippingTargetManager CreateNewManager(SubRoot habitat)
        {
            var manager = new ShippingTargetManager(habitat);
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            return manager;
        }

        internal static void RemoveShippingTarget(AlterraShippingTarget shippingTarget)
        {
            foreach (ShippingTargetManager manager in Managers)
            {
                if (!manager.ShippingTargets.Contains(shippingTarget)) continue;
                manager.ShippingTargets.Remove(shippingTarget);
                manager.UpdateGlobalTargets();
                QuickLogger.Debug($"Removed Shipping Target : {shippingTarget.ID}", true);
            }
        }

        public void AddShippingTarget(AlterraShippingTarget shippingTarget)
        {
            if (!ShippingTargets.Contains(shippingTarget) && shippingTarget.IsConstructed)
            {
                ShippingTargets.Add(shippingTarget);
                QuickLogger.Debug($"Add Shipping Target : {shippingTarget.ID}", true);
                UpdateGlobalTargets();
            }
        }

        public void UpdateGlobalTargets()
        {
            //var targets = Resources.FindObjectsOfTypeAll<AlterraShippingTarget>();

            GlobalShippingTargets.Clear();

            foreach (ShippingTargetManager manager in Managers)
            {
                foreach (var target in manager.ShippingTargets)
                {
                    if (!GlobalShippingTargets.Contains(target))
                    {
                        GlobalShippingTargets.Add(target);
                    }
                }
            }

            GlobalChanged?.Invoke();
            QuickLogger.Debug($"Updated Global Targets Count : {GlobalShippingTargets.Count}", true);
        }
    }
}
