using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Mods.Common.DroneSystem.StatesMachine;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Mods.Common.DroneSystem.Factory
{
    internal static class StateFactory
    {
        private static Dictionary<string, Type> statesByName;
        private static bool IsInitialized => statesByName != null;

        private static void InitializeFactory()
        {
            if (IsInitialized) return;

            var statesTypes = Assembly.GetAssembly(typeof(BaseState)).GetTypes().
                    Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BaseState)));

            statesByName = new Dictionary<string, Type>();

            foreach (var type in statesTypes)
            {
                var tempState = Activator.CreateInstance(type) as BaseState;
                statesByName.Add(tempState.Name,type);
            }
        }

        internal static BaseState GetState(string stateType)
        {
            InitializeFactory();

            if (statesByName.ContainsKey(stateType))
            {
                Type type = statesByName[stateType];
                var state = Activator.CreateInstance(type) as BaseState;
                return state;
            }

            return null;
        }

        internal static IEnumerable<string> GetStateNames()
        {
            QuickLogger.Debug("Getting State Names");
            InitializeFactory();
            return statesByName.Keys;
        }
    }
}
