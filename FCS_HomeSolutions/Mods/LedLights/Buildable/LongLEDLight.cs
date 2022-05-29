using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.LedLights.Buildable
{
    internal class LongLEDLight : LedLightPatch
    {
        public LongLEDLight() : base(new LedLightData
        {
            classId = "LedLightStickLong",
            description = "A long LED light stick, suitable for interior and exterior use. (Change the color with the Paint Tool)",
            friendlyName = "Long LED Light Stick",
            allowedInBase = true,
            allowedInSub = false,
            allowedOnGround = true,
            allowedOnWall = false,
            allowedOutside = true,
            categoryForPDA = TechCategory.Misc,
            groupForPda = TechGroup.Miscellaneous,
            size = Vector3.zero,
            center = Vector3.zero,
            prefab = ModelPrefab.GetPrefabFromGlobal("FCS_LedLightStick_03")
        })
        {
            
        }
    }
}
