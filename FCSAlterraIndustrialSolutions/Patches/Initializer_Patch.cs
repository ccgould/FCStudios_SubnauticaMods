using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Data;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSAlterraIndustrialSolutions.Utilities;
using Harmony;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Patches
{
    class Initializer_Patch
    {

        private static bool initialized;

        [HarmonyPatch(typeof(uGUI_PowerIndicator))]
        [HarmonyPatch("Initialize")]
        class Initialize_Patch : MonoBehaviour
        {
            private static void Postfix(uGUI_PowerIndicator __instance)
            {
                if (initialized)
                {
                    if (Player.main == null)
                    {
                        Log.Info("Deinitialize from no player");
                        initialized = false;
                    }
                    return;
                }

                if (Inventory.main == null)
                {
                    return;
                }

                Log.Info("Initialized Works");
          
                initialized = true;
            }

           
        }


        [HarmonyPatch(typeof(uGUI_PowerIndicator))]
        [HarmonyPatch("LateUpdate")]
        class InitializeUpdate_Patch : MonoBehaviour
        {
            private static void Postfix()
            {
                AISolutionsData.UpdateTime();
            }
        }

        [HarmonyPatch(typeof(uGUI_PowerIndicator))]
        [HarmonyPatch("Deinitialize")]
        class uGUI_PowerIndicator_Deinitialize_Patch
        {
            private static void PostFix()
            {
                Log.Info("Deinitialize");
                initialized = false;
            }
        }
    }
}
