using System.Collections.Generic;
using System.Reflection;
using FCS_AlterraHub.API;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
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
                    Size = new Vector3(1.104156f, 1.028278f, 1433573f),
                    Center = new Vector3(0f, 0.6523819f,0f)
                }),
            new DecorationEntryPatch("ahssmallrailglass", "Small Railing With Glass", "A railing to create a barrior", ModelPrefab.GetPrefab("Small_Rail_wGlass_01"),
                new Settings
                {
                    KitClassID = "ahssmallrailingglass_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1.104156f, 1.028278f, 1433573f),
                    Center = new Vector3(0f, 0.6523819f,0f)
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
            new DecorationEntryPatch("ahsSweetWaterBar", "Sweet Water Bar", "All drinks on the house.", ModelPrefab.GetPrefab("SweetWaterBar"),
                new Settings
                {
                    KitClassID = "ahsSweetWaterBar_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(3.075155f, 2.793444f, 1.905103f),
                    Center = new Vector3(-0.1215184f, 1.516885f,-0.008380651f)
                }),

    };

            [QModPatch]
        public void Patch()
        {

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
#endif

            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            foreach (var decoration in _decorations)
            {
                decoration.Patch();
            }
        }
    }

    
}
