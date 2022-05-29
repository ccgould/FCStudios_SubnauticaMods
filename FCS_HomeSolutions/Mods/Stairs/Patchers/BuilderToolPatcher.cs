using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FCS_HomeSolutions.Mods.Stairs.Buildable;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Stairs.Patchers
{

 //   [HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.OnHover))]

 //   internal class BuilderToolPatcher
 //   {
	//	public static class BuilderToolFixer
	//	{
 //   		public static bool ConstructOnHover_Prefix(BuilderTool __instance, Constructable constructable)
	//		{
	//			if (constructable.techType == TechType.BaseCorridor)
	//			{
	//				Transform transform = constructable.gameObject.transform;
	//				if (!(transform != null))
	//				{
	//					return true;
	//				}
	//				using (IEnumerator enumerator = transform.GetEnumerator())
	//				{
	//					while (enumerator.MoveNext())
	//					{
	//						object obj = enumerator.Current;
	//						Transform transform2 = (Transform)obj;
	//						if (transform2.name.StartsWith("BaseGhost"))
	//						{
	//							using (IEnumerator enumerator2 = transform2.GetEnumerator())
	//							{
	//								while (enumerator2.MoveNext())
	//								{
	//									object obj2 = enumerator2.Current;
	//									Transform transform3 = (Transform)obj2;
	//									if (transform3.name.StartsWith("BaseConnectorLc") || transform3.name.StartsWith("CyclopsDockingHatchClean"))
	//									{
	//										HandReticle main = HandReticle.main;
	//										if (constructable.constructed)
	//										{
	//											main.SetInteractText(StairsBuildable.StairsFriendly, (string)BuilderToolFixer._deconstructText.GetValue(__instance), false, false, false);
	//											return false;
	//										}
	//										StringBuilder stringBuilder = new StringBuilder();
	//										stringBuilder.AppendLine((string)BuilderToolFixer._constructText.GetValue(__instance));
	//										foreach (KeyValuePair<TechType, int> keyValuePair in constructable.GetRemainingResources())
	//										{
	//											TechType key = keyValuePair.Key;
	//											string text = Language.main.Get(key);
	//											int value = keyValuePair.Value;
	//											if (value > 1)
	//											{
	//												stringBuilder.AppendLine(Language.main.GetFormat<string, int>("RequireMultipleFormat", text, value));
	//											}
	//											else
	//											{
	//												stringBuilder.AppendLine(text);
	//											}
	//										}
	//										main.SetInteractText(StairsBuildable.StairsFriendly, stringBuilder.ToString(), false, false, false);
	//										main.SetProgress(constructable.amount);
	//										main.SetIcon(HandReticle.IconType.Progress, 1.5f);
	//										return false;
	//									}
	//								}
	//								break;
	//							}
	//						}
	//					}
	//					return true;
	//				}
	//			}
	//			if (constructable.techType == TechType.Sign && constructable.gameObject.transform.GetComponent<LoopRefreshEnergyController>() != null)
	//			{
	//				HandReticle.main.SetInteractText(CyclopsHatchConnector.CyclopsHatchConnectorName, false, HandReticle.Hand.None);
	//				return false;
	//			}
	//			return true;
	//		}
	//		public static bool DeconstructOnHover_Prefix(BuilderTool __instance, BaseDeconstructable deconstructable)
	//		{
	//			if ((TechType)BuilderToolFixer._recipe.GetValue(deconstructable) == TechType.BaseConnector)
	//			{
	//				Transform transform = deconstructable.gameObject.transform;
	//				if (transform != null)
	//				{
	//					using (List<BasePart>.Enumerator enumerator = BaseFixer.BaseParts.GetEnumerator())
	//					{
	//						while (enumerator.MoveNext())
	//						{
	//							if (FastHelper.IsNear(enumerator.Current.position, transform.position))
	//							{
	//								HandReticle.main.SetInteractInfo(CyclopsHatchConnector.CyclopsHatchConnectorName, (string)BuilderToolFixer._deconstructText.GetValue(__instance));
	//								return false;
	//							}
	//						}
	//					}
	//					return true;
	//				}
	//			}
	//			return true;
	//		}

	//		private static readonly FieldInfo _deconstructText = typeof(BuilderTool).GetField("deconstructText", BindingFlags.Instance | BindingFlags.NonPublic);
 //           private static readonly FieldInfo _constructText = typeof(BuilderTool).GetField("constructText", BindingFlags.Instance | BindingFlags.NonPublic);
 //           private static readonly FieldInfo _recipe = typeof(BaseDeconstructable).GetField("recipe", BindingFlags.Instance | BindingFlags.NonPublic);
	//	}
	//}
}
