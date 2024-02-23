using FCS_AlterraHub.Core.Managers;
using FCS_AlterraHub.Models.Mono;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Patcher;
[HarmonyPatch(typeof(GrowingPlant))]
[HarmonyPatch("Start")]
internal class GrowningPlantPatches
{
    [HarmonyPostfix]
    public static void Postfix(ref GrowingPlant __instance)
    {
        if (__instance.GetComponentInParent<HydroponicHarvesterController>() is null) return;
        
        var growthWidth = __instance.growthWidth;

        var growthHeight = __instance.growthHeight;

        var growthWidthIndoor = __instance.growthWidthIndoor;


        var growthHeightIndoor = __instance.growthHeightIndoor;

        QuickLogger.Debug($"Growing Plant: {__instance.plantTechType}");

        growthWidth = new AnimationCurve(growthWidth.keys[0], new Keyframe(1,GetValue(__instance.plantTechType)));
        growthWidth.SmoothTangents(0, 0.8f);
        growthWidth.SmoothTangents(1, 0.8f);

        __instance.growthWidth = growthWidth;

        growthHeight = new AnimationCurve(growthHeight.keys[0], new Keyframe(1, GetValue(__instance.plantTechType)));
        growthHeight.SmoothTangents(0, 0.8f);
        growthHeight.SmoothTangents(1, 0.8f);

        __instance.growthHeight = growthHeight;


        growthWidthIndoor = new AnimationCurve(growthWidthIndoor.keys[0], new Keyframe(1, GetValue(__instance.plantTechType)));
        growthWidthIndoor.SmoothTangents(0, 0.8f);
        growthWidthIndoor.SmoothTangents(1, 0.8f);

        __instance.growthWidthIndoor = growthWidthIndoor;



        growthHeightIndoor = new AnimationCurve(growthHeightIndoor.keys[0], new Keyframe(1, GetValue(__instance.plantTechType)));
        growthHeightIndoor.SmoothTangents(0, 0.8f);
        growthHeightIndoor.SmoothTangents(1, 0.8f);

        __instance.growthHeightIndoor = growthHeightIndoor;
    }

    private static float GetValue(TechType plantTechType)
    {
       if( HeightRestrictions.TryGetValue(plantTechType, out var heightRestrictions))
       {
            return heightRestrictions;
       }

       return 0.2f;
    }

    public static Dictionary<TechType, float> HeightRestrictions { get; set; } = new()
        {
            {TechType.BloodVine,.03f},
            {TechType.Creepvine,.03f},
            {TechType.MelonPlant,.3f},
            {TechType.AcidMushroom,0.3752623f},
            {TechType.WhiteMushroom,0.3752623f},
            {TechType.PurpleTentacle,0.398803f},
            {TechType.BulboTree,0.1638541f},
            {TechType.OrangeMushroom,0.3261985f},
            {TechType.PurpleVasePlant,0.3261985f},
            {TechType.HangingFruitTree,0.1f},
            {TechType.PurpleVegetablePlant,0.3793474f},
            {TechType.FernPalm,0.3770215f},
            {TechType.OrangePetalsPlant,0.2895765f},
            {TechType.PinkMushroom,0.5093553f},
            {TechType.PurpleRattle,0.5077053f},
            {TechType.PinkFlower,0.3104943f},
            //{TechType.PurpleTentacle,0.2173283f},
            {TechType.SmallKoosh,0.3104943f},
            {TechType.GabeSFeather,0.2198986f},
            {TechType.MembrainTree,0.1574817f},
            {TechType.BluePalm,0.4339138f},
            {TechType.EyesPlant,0.2179814f},
            {TechType.RedBush,0.1909147f},
            {TechType.RedGreenTentacle,0.2514144f},
            {TechType.RedConePlant,0.1909147f},
            {TechType.SpikePlant,0.2668017f},
            {TechType.SeaCrown,0.2668017f},
            {TechType.PurpleStalk,0.2668017f},
            {TechType.RedBasketPlant,0.2455478f},
            {TechType.ShellGrass,0.2455478f},
            {TechType.SpottedLeavesPlant,0.2455478f},
            {TechType.RedRollPlant,0.2082229f},
            {TechType.PurpleBranches,0.3902903f},
            {TechType.SnakeMushroom,0.3902903f},
            {TechType.PurpleFan,0.2761627f},
            {TechType.SmallFan,0.2761627f},
            {TechType.JellyPlant,0.2761627f},
            {TechType.PurpleBrainCoral, 0.3731633f}
        };
}