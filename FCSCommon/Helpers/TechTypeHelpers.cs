using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCSCommon.Helpers
{
    public static class TechTypeHelpers
    {
        public static TechType GetTechType(string techTypeString)
        {
            bool otherModIsInstalled = TechTypeHandler.TryGetModdedTechType(techTypeString, out TechType techtype);

            if (otherModIsInstalled) return techtype;
            QuickLogger.Error($"Couldn't find {techTypeString} Tech Type");
            return TechType.None;
        }
    }
}
