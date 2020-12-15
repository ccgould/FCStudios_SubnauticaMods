using System;
using System.Collections.Generic;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Extensions;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Patches
{
    internal class Builder_Pathes
    {
		public static List<TechType> acceptableColliders = new List<TechType>() { TechType.BaseWindow, TechType.BaseRoom, TechType.BaseMoonpool, TechType.BaseReinforcement, TechType.BaseConnector, TechType.BaseCorridor,  TechType.BaseCorridorI, TechType.BaseCorridorT };

		[HarmonyPatch(typeof(Builder), nameof(Builder.CheckSpace), new Type[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(int), typeof(Collider), })]
		internal class Builder_CheckSpace_Patch
		{
			[HarmonyPostfix]
			public static void Postfix(Vector3 extents, Collider allowedCollider, ref bool __result)
			{

				if (__result || Builder.constructableTechType != Mod.CurtainTechType || extents.x <= 0f || extents.y <= 0f || extents.z <= 0f)
					return;

				if (acceptableColliders.Contains(Builder.sColliders[0]?.gameObject?.GetComponentInParent<BaseDeconstructable>()?.recipe ?? TechType.None) && acceptableColliders.Contains(allowedCollider?.gameObject?.GetComponentInParent<BaseDeconstructable>()?.recipe ?? TechType.None))
					__result = true;
            }
		}

		[HarmonyPatch(typeof(Builder), nameof(Builder.CheckTag))]
		internal class Builder_CheckTag_Patch
		{
			[HarmonyPostfix]
			public static void Postfix(Collider c, ref bool __result)
			{
				if (Builder.constructableTechType == Mod.CurtainTechType && (c?.gameObject?.GetComponentInParent<BaseDeconstructable>()?.recipe ?? TechType.None) == TechType.BaseWindow)
					__result = true;
			}
		}
	}
}
