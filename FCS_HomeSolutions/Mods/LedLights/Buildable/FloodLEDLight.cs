﻿using FCS_AlterraHub.Extensions;
using FCS_HomeSolutions.Buildables;
using SMLHelper.Crafting;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
#endif

namespace FCS_HomeSolutions.Mods.LedLights.Buildable
{
    internal class FloodLEDLight : LedLightPatch
    {
        public FloodLEDLight() : base(new LedLightData
        {
            classId = "FloodLEDLight",
            description = "A Flood Light for wide area illumination, suitable for exterior use. (Change the color with the Paint Tool)",
            friendlyName = "LED Flood Light",
            allowedInBase = false,
            allowedInSub = false,
            allowedOnGround = true,
            allowedOnWall = false,
            allowedOutside = true,
            categoryForPDA = TechCategory.ExteriorModule,
            groupForPda = TechGroup.ExteriorModules,
            size = Vector3.zero,
            center = Vector3.zero,
            prefab = ModelPrefab.GetPrefabFromGlobal("FCS_FloodLight")
        })
        {
        }
    }
}
