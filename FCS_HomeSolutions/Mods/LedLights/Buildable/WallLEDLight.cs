using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.LedLights.Buildable
{
    internal class WallLEDLight : LedLightPatch
    {
        public WallLEDLight() : base(new LedLightData
        {
            classId = "LedLightStickWall",
            description =
                "A wall mountable LED light strip. (Change the color with the Paint Tool) (Interior use only)",
            friendlyName = "Wall Mountable LED Light Strip",
            allowedInBase = true,
            allowedInSub = true,
            allowedOnGround = false,
            allowedOnWall = true,
            allowedOutside = false,
            categoryForPDA = TechCategory.InteriorModule,
            groupForPda = TechGroup.InteriorModules,
            size = Vector3.zero,
            center = Vector3.zero,
            prefab = ModelPrefab.GetPrefabFromGlobal("FCS_LedLightStick_02")
        })
        {
        }
    }
}