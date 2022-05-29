using System.Collections.Generic;
using System.Reflection;
using FCS_AlterraHub.Extensions;
using FCS_HomeSolutions.Mods.Stairs.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Stairs.Patchers
{

 //   [HarmonyPatch(typeof(uGUI_BuilderMenu), nameof(uGUI_BuilderMenu.OnPointerClick))]
 //   class uGUI_BuilderMenuPatcherOnClickPatch
 //   {
 //       [HarmonyPrefix]
 //       public static bool Prefix(uGUI_BuilderMenu __instance, string id, int button)
 //       {
 //           QuickLogger.Debug("1", true);
 //           if (button != 0)
 //           {
 //               return true;
 //           }
 //           QuickLogger.Debug("1", true);

 //           Dictionary<string, TechType> dictionary = (Dictionary<string, TechType>)_items.GetValue(__instance);
 //           TechType techType;
 //           QuickLogger.Debug("1", true);

 //           if (dictionary == null || !dictionary.TryGetValue(id, out techType))
 //           {
 //               return true;
 //           }
 //           QuickLogger.Debug("1", true);

 //           if (techType == StairsBuildable.StairsClassID.ToTechType() && KnownTech.Contains(techType))
 //           {
 //           QuickLogger.Debug("Is Stairs",true);

 //           SelectedStairs = true;
 //               GameObject buildPrefab = CraftData.GetBuildPrefab(TechType.BaseHatch);
 //               _SetState.Invoke(__instance, new object[]
 //               {
 //                   false
 //               });
 //               Builder.Begin(buildPrefab);
 //               return false;
 //           }
 //           return true;
 //       }

 //       private static readonly FieldInfo _items = typeof(uGUI_BuilderMenu).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
 //       private static readonly MethodInfo _SetState = typeof(uGUI_BuilderMenu).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic);
 //       public static bool SelectedStairs { get; set; }
	//}
}
