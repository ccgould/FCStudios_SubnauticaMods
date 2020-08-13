using System.Collections.Generic;

namespace FCSTechFabricator.Managers
{
    public static class EggHandler
    {
        private static Dictionary<TechType, TechType> EggTypes { get; } = new Dictionary<TechType, TechType>
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

        public static bool GetDiscoveredEgg(TechType unDiscovered, out TechType value)
        {
            foreach (KeyValuePair<TechType, TechType> eggType in EggTypes)
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
