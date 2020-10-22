using System.Collections.Generic;
using System.Reflection;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Spawnables;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;
using UnityEngine;

namespace FCS_HomeSolutions
{
    /*
     * Alterra Home Solutions mod pack adds objects to subnautica that deals with bases and decorations
    */

    [QModCore]
    public class QPatch
    {
        private static List<DecorationEntryPatch> _decorations = new List<DecorationEntryPatch>
        {
            new DecorationEntryPatch("ahsrailing", "Large Railing", "A railing to create a barrior", ModelPrefab.GetPrefab("Large_Rail_01"),
                new Settings
                {
                    KitClassID = "ahsrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f,0f)
                }),
            new DecorationEntryPatch("ahsrailingglass", "Large Railing With Glass", "A railing to create a barrior", ModelPrefab.GetPrefab("Large_Rail_wGlass_01"),
                new Settings
                {
                    KitClassID = "ahsrailingglass_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f,0f)
                }),
            new DecorationEntryPatch("ahssmallrail", "Small Railing", "A railing to create a barrior", ModelPrefab.GetPrefab("Small_Rail_01"),
                new Settings
                {
                    KitClassID = "ahssmallrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f,-0.004088677f)
                }),
            new DecorationEntryPatch("ahssmallrailglass", "Small Railing With Glass", "A railing to create a barrior", ModelPrefab.GetPrefab("Small_Rail_wGlass_01"),
                new Settings
                {
                    KitClassID = "ahssmallrailingglass_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f,-0.004088677f)
                }),
            new DecorationEntryPatch("ahssmallstairplatform", "Small Stair Platform", "A stairs for your personal needs.", ModelPrefab.GetPrefab("Small_PlatformDoorStairs"),
                new Settings
                {
                    KitClassID = "ahssmallstairplatform_kit",
                    AllowedInBase = false,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(4.033218f, 2.194448f, 2.34824f),
                    Center = new Vector3(4.449882e-24f, 1.225415f,0.8919346f)
                }),
            new DecorationEntryPatch("ahssmallrailmesh", "Small Railing With Mesh", "A railing to create a barrior", ModelPrefab.GetPrefab("Small_Rail_wNeat_01"),
                new Settings
                {
                    KitClassID = "ahsSmallRailMesh_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f,-0.004088677f)
                }),
            new DecorationEntryPatch("ahslargerailmesh", "Large Railing With Mesh", "A railing to create a barrior", ModelPrefab.GetPrefab("Large_Rail_wNeat_01"),
                new Settings
                {
                    KitClassID = "ahsSmallRailMesh_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f,0f)
                }),

    };

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            ModelPrefab.LoadSelfLoadingPrefab();

            var ahsSweetWaterBar = new SweetWaterBarPatch("ahsSweetWaterBar", "Sweet Water Bar", "All drinks on the house.", ModelPrefab.GetPrefab("SweetWaterBar"), new Settings
            {
                KitClassID = "ahsSweetWaterBar_kit",
                AllowedInBase = true,
                AllowedOutside = true,
                AllowedOnGround = true,
                RotationEnabled = true,
                Size = new Vector3(3.075155f, 2.793444f, 1.905103f),
                Center = new Vector3(-0.1215184f, 1.516885f, -0.008380651f)
            });
            ahsSweetWaterBar.Patch();

            foreach (var decoration in _decorations)
            {
                decoration.Patch();
            }


            //Patch Paint Tool
            var paintToolSpawnable = new PaintToolSpawnable();
            paintToolSpawnable.Patch();
        }
    }


}
