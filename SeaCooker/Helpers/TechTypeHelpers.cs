using AE.SeaCooker.Enumerators;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace AE.SeaCooker.Helpers
{
    internal static class TechTypeHelpers
    {
        private static TechType _gasTankTechType;
        private static TechType _alienFecesTechType;

        internal static void Initialize()
        {
            var gasTank = TechTypeHandler.TryGetModdedTechType("SeaGasTank_SC", out TechType gasTankTechType);

            if (!gasTank)
            {
                QuickLogger.Error("Gas tank TechType not found");
            }
            else
            {
                _gasTankTechType = gasTankTechType;
            }

            var alienFeces = TechTypeHandler.TryGetModdedTechType("SeaAlienGasTank_SC", out TechType alienFecesTechType);

            if (!alienFeces)
            {
                QuickLogger.Error("Alien Feces Tank TechType not found");
            }
            else
            {
                _alienFecesTechType = alienFecesTechType;
            }
        }

        internal static TechType GasTankTechType() => _gasTankTechType;

        public static TechType AlienFecesTechType() => _alienFecesTechType;

        public static TechType GetTechType(FuelType currentFuel)
        {
            switch (currentFuel)
            {
                case FuelType.Gas:
                    return _gasTankTechType;

                case FuelType.AlienFeces:
                    return _gasTankTechType;
            }

            return TechType.None;
        }
    }
}
