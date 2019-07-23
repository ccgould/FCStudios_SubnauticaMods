using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCS_DeepDriller.Helpers
{
    internal static class TechTypeHelpers
    {
        private static TechType _batteryModuleTechType;
        private static TechType _solarPanelTechType;
        private static TechType _focusTechType;

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

            var solarPanelFound = TechTypeHandler.TryGetModdedTechType("SolarPanelAttachment_DD", out TechType solarPanelTechType);

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
    }
}
