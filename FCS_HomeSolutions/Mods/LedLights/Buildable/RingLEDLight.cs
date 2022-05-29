using FCS_AlterraHub.Extensions;
using FCS_HomeSolutions.Buildables;
using SMLHelper.V2.Crafting;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
#endif

namespace FCS_HomeSolutions.Mods.LedLights.Buildable
{
    internal class RingLEDLight : LedLightPatch
    {
        public RingLEDLight() : base(new LedLightData
        {
            classId = "RingLight",
            description = "Elegant interior light, suitable for interior tables, shelves, or stands. (Change the color with the Paint Tool)",
            friendlyName = "Ring Light",
            allowedInBase = true,
            allowedInSub = true,
            allowedOnGround = true,
            allowedOnWall = false,
            allowedOutside = false,
            allowedOnContructables = true,
            categoryForPDA = TechCategory.InteriorModule,
            groupForPda = TechGroup.InteriorModules,
            center = new Vector3(0f, 0.2182536f, 0f),
            size = new Vector3(0.3452354f, 0.3483996f, 0.1863832f),
            prefab = ModelPrefab.GetPrefabFromGlobal("FCS_RingLamp")
        }) 
        {
        }

    }
}
