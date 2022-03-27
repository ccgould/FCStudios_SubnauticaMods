using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.LedLights.Buildable
{
    internal class ShortLEDLight : LedLightPatch
    {
        public ShortLEDLight() : base(new LedLightData
        {
            classId = "LedLightStickShort",
            description = "A short LED light stick, suitable for interior and exterior use. (Change the color with the Paint Tool)",
            friendlyName = "Short LED Light Stick",
            allowedInBase = true,
            allowedInSub = true,
            allowedOnGround = true,
            allowedOnWall = false,
            allowedOutside = true,
            categoryForPDA = TechCategory.Misc,
            groupForPda = TechGroup.Miscellaneous,
            size = Vector3.zero,
            center = Vector3.zero,
            prefab = null
        })
        {
        }
    }
}
