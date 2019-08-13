using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCS_DeepDriller.Helpers
{
    internal static class TechTypeHelper
    {
        private static TechType _batteryModuleTechType;
        private static TechType _solarPanelTechType;
        private static TechType _focusTechType;
        private static TechType _drillerMK1TechType;
        private static TechType _drillerMK2TechType;
        private static TechType _drillerMK3TechType;

        internal static void Initialize()
        {
            var batteryModuleFound = TechTypeHandler.TryGetModdedTechType("BatteryAttachment_DD", out TechType batteryModuleTechType);

            if (!batteryModuleFound)
            {
                QuickLogger.Error("Deep Driller Battery Attachment TechType not found");
            }
            else
            {
                _batteryModuleTechType = batteryModuleTechType;
            }

            var solarPanelFound = TechTypeHandler.TryGetModdedTechType("SolarAttachment_DD", out TechType solarPanelTechType);

            if (!solarPanelFound)
            {
                QuickLogger.Error("Deep Driller Solar Panel TechType not found");
            }
            else
            {
                _solarPanelTechType = solarPanelTechType;
            }

            var focusFound = TechTypeHandler.TryGetModdedTechType("FocusAttachment_DD", out TechType focusTechType);

            if (!focusFound)
            {
                QuickLogger.Error("Deep Driller Focus Attachment TechType not found");
            }
            else
            {
                _focusTechType = focusTechType;
            }

            for (int i = 1; i < 4; i++)
            {
                var mkFound = TechTypeHandler.TryGetModdedTechType($"DrillerMK{i}_DD", out TechType techType);

                if (!mkFound)
                {
                    QuickLogger.Error($"Deep Driller MK{i} TechType not found");
                }
                else
                {
                    switch (i)
                    {
                        case 1:
                            _drillerMK1TechType = techType;
                            break;
                        case 2:
                            _drillerMK2TechType = techType;
                            break;
                        case 3:
                            _drillerMK3TechType = techType;
                            break;
                    }
                }
            }
        }

        internal static DeepDrillModules GetDeepModule(TechType techType)
        {
            var module = DeepDrillModules.None;

            if (techType == _batteryModuleTechType)
            {
                module = DeepDrillModules.Battery;
            }

            if (techType == _solarPanelTechType)
            {
                module = DeepDrillModules.Solar;
            }

            if (techType == _focusTechType)
            {
                module = DeepDrillModules.Focus;
            }

            return module;
        }

        internal static TechType BatteryAttachmentTechType() => _batteryModuleTechType;
        internal static TechType SolarAttachmentTechType() => _solarPanelTechType;
        internal static TechType FocusAttachmentTechType() => _focusTechType;
        internal static TechType DrillerMK1TechType() => _drillerMK1TechType;
        internal static TechType DrillerMK2TechType() => _drillerMK2TechType;
        internal static TechType DrillerMK3TechType() => _drillerMK3TechType;
    }
}
