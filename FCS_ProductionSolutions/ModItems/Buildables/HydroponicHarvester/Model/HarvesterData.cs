using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Model;
internal static class HarvesterData
{
    public static Dictionary<TechType, float> HeightRestrictions { get; set; } = new()
        {
            {TechType.BloodVine,.03f},
            {TechType.Creepvine,.03f},
            {TechType.MelonPlant,.5f},
            {TechType.AcidMushroom,0.3752623f},
            {TechType.WhiteMushroom,0.3752623f},
            {TechType.PurpleTentacle,0.398803f},
            {TechType.BulboTree,0.1638541f},
            {TechType.OrangeMushroom,0.3261985f},
            {TechType.PurpleVasePlant,0.3261985f},
            {TechType.HangingFruitTree,0.1932445f},
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
