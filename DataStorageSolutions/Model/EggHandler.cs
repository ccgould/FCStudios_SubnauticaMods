using System.Collections.Generic;


namespace DataStorageSolutions.Model
{
    internal static class EggHandler
    {
        private static Dictionary<TechType, TechType> _eggTypes { get; } = new Dictionary<TechType, TechType>
        {
            {
                TechType.ShockerEggUndiscovered,
                TechType.ShockerEgg
            },
            {
                TechType.BonesharkEggUndiscovered,
                TechType.BonesharkEgg
            },
            {
                TechType.CrabsnakeEggUndiscovered,
                TechType.CrabsnakeEgg
            },
            {
                TechType.CrabsquidEggUndiscovered,
                TechType.CrabsquidEgg
            },
            {
                TechType.CrashEggUndiscovered,
                TechType.CrashEgg
            },
            {
                TechType.CutefishEggUndiscovered,
                TechType.CutefishEgg
            },
            {
                TechType.GasopodEggUndiscovered,
                TechType.GasopodEgg
            },
            {
                TechType.JellyrayEggUndiscovered,
                TechType.JellyrayEgg
            },
            {
                TechType.LavaLizardEggUndiscovered,
                TechType.LavaLizardEgg
            },
            {
                TechType.MesmerEggUndiscovered,
                TechType.MesmerEgg
            },
            {
                TechType.RabbitrayEggUndiscovered,
                TechType.RabbitrayEgg
            },
            {
                TechType.SandsharkEggUndiscovered,
                TechType.SandsharkEgg
            },
            {
                TechType.SpadefishEggUndiscovered,
                TechType.SpadefishEgg
            },
            {
                TechType.StalkerEggUndiscovered,
                TechType.StalkerEgg
            }
        };

        internal static bool GetDiscoveredEgg(TechType unDiscovered, out TechType value)
        {
            foreach (KeyValuePair<TechType, TechType> eggType in _eggTypes)
            {
                if (eggType.Key != unDiscovered) continue;
                value = eggType.Value;
                return true;
            }

            value = unDiscovered;
            return false;
        }
    }
}
