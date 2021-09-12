using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Mods.Stairs.Buildable;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Stairs.Patchers
{
 //   [HarmonyPatch(typeof(Constructable))]
	//public class ConstructablePatcher
 //   {
 //       private static bool IsBaseConnector(Transform root)
	//	{
	//		foreach (object obj in root)
	//		{
	//			Transform transform = (Transform)obj;
	//			if (transform.name.StartsWith("BaseGhost"))
	//			{
 //                   foreach (Transform transform1 in transform)
 //                   {
	//					if (transform1.name.StartsWith("BaseCorridorHatch"))
 //                       {
 //                           return true;
 //                       }
	//				}
 //               }
	//		}
	//		return false;
	//	}

	//	private static void SetBaseRoot(Constructable constructable, BasePart basePart)
	//	{
	//		GameObject gameObject = constructable.gameObject;
	//		basePart.root = gameObject.GetComponent<BaseRoot>();
	//		if (basePart.root == null)
	//		{
	//			foreach (object obj in gameObject.transform)
	//			{
	//				Transform transform = (Transform)obj;
	//				if (transform.name.StartsWith("BaseGhost"))
	//				{
	//					GameObject gameObject2 = transform.gameObject;
	//					basePart.root = ((gameObject2 != null) ? gameObject2.GetComponent<BaseRoot>() : null);
	//					break;
	//				}
	//			}
	//		}
	//	}

	//	//private static KeyValuePair<List<BasePart>, List<Base>> BaseSelect(Constructable construct, Vector3 pos)
	//	//{
	//	//	List<BasePart> list = new List<BasePart>();
	//	//	List<Base> list2 = new List<Base>();
	//	//	foreach (BasePart basePart in BaseFixer.BaseParts)
	//	//	{
	//	//		if (WorldHelpers.IsNear(basePart.position, pos))
	//	//		{
	//	//			list.Add(basePart);
	//	//			if (basePart.root == null)
	//	//			{
	//	//				SetBaseRoot(construct, basePart);
	//	//			}
	//	//			BaseRoot root = basePart.root;
	//	//			Base @base = (root != null) ? root.GetComponent<Base>() : null;
	//	//			if (@base != null)
	//	//			{
	//	//				list2.Add(@base);
	//	//			}
	//	//		}
	//	//	}
	//	//	return new KeyValuePair<List<BasePart>, List<Base>>(list, list2);
	//	//}

 //       [HarmonyPatch(nameof(Constructable.Construct))]
 //       [HarmonyPrefix]
	//	public static bool Construct_Prefix(Constructable __instance, ref bool __result)
	//	{
	//		if (!__instance.constructed && __instance.techType == TechType.BaseCorridor)
	//		{
	//			GameObject gameObject = __instance.gameObject;
	//			if (((gameObject != null) ? gameObject.transform : null) != null && IsBaseConnector(__instance.gameObject.transform))
	//			{
	//				_resourceMapField.SetValue(__instance, ResourceMap);
	//				int count = ResourceMap.Count;
	//				int num = Mathf.CeilToInt(__instance.constructedAmount * (float)count);
	//				__instance.constructedAmount += Time.deltaTime / ((float)count * (float)_GetConstructInterval.Invoke(null, null));
	//				__instance.constructedAmount = Mathf.Clamp01(__instance.constructedAmount);
	//				int num2 = Mathf.CeilToInt(__instance.constructedAmount * (float)count);
	//				if (num2 != num)
	//				{
	//					TechType destroyTechType = ResourceMap[num2 - 1];
	//					if (!Inventory.main.DestroyItem(destroyTechType, false) && GameModeUtils.RequiresIngredients())
	//					{
	//						__instance.constructedAmount = (float)num / (float)count;
	//						__result = false;
	//						return false;
	//					}
	//				}
	//				_UpdateMaterial.Invoke(__instance, null);
	//				if (__instance.constructedAmount >= 1f)
	//				{
	//					__instance.SetState(true, true);
	//				}
	//				__result = true;
	//				return false;
	//			}
	//		}
	//		return true;
	//	}

 //       public static List<TechType> ResourceMap { get; set; } = new List<TechType>
 //       {
 //           TechType.Copper
 //       };

	//	//      public static bool Deconstruct_Prefix(Constructable __instance, ref bool __result)
	//	//{
	//	//	if (!__instance.constructed && __instance.techType == TechType.BaseConnector && CyclopsDockingMod.CyclopsHatchConnector != TechType.None)
	//	//	{
	//	//		GameObject gameObject = __instance.gameObject;
	//	//		Transform transform = (gameObject != null) ? gameObject.transform : null;
	//	//		if (transform != null && IsBaseConnector(transform))
	//	//		{
	//	//			_resourceMapField.SetValue(__instance, CyclopsHatchConnector.ResourceMap);
	//	//			int count = CyclopsHatchConnector.ResourceMap.Count;
	//	//			int num = Mathf.CeilToInt(__instance.constructedAmount * (float)CyclopsHatchConnector.ResourceMap.Count);
	//	//			__instance.constructedAmount -= Time.deltaTime / ((float)count * (float)_GetConstructInterval.Invoke(null, null));
	//	//			__instance.constructedAmount = Mathf.Clamp01(__instance.constructedAmount);
	//	//			int num2 = Mathf.CeilToInt(__instance.constructedAmount * (float)CyclopsHatchConnector.ResourceMap.Count);
	//	//			if (num2 != num && GameModeUtils.RequiresIngredients())
	//	//			{
	//	//				Pickupable component = CraftData.InstantiateFromPrefab(CyclopsHatchConnector.ResourceMap[num2], false).GetComponent<Pickupable>();
	//	//				if (!Inventory.main.Pickup(component, false))
	//	//				{
	//	//					ErrorMessage.AddError(Language.main.Get("InventoryFull"));
	//	//					UnityEngine.Object.Destroy(component.gameObject);
	//	//					__instance.constructedAmount = ((float)num2 + 0.001f) / (float)count;
	//	//					__result = false;
	//	//					return false;
	//	//				}
	//	//			}
	//	//			_UpdateMaterial.Invoke(__instance, null);
	//	//			if (__instance.constructedAmount <= 0f)
	//	//			{
	//	//				KeyValuePair<List<BasePart>, List<Base>> keyValuePair = BaseSelect(__instance, transform.position);
	//	//				if (keyValuePair.Key.Count > 0)
	//	//				{
	//	//					foreach (BasePart item in keyValuePair.Key)
	//	//					{
	//	//						List<string> list = new List<string>();
	//	//						foreach (KeyValuePair<string, BasePart> keyValuePair2 in SubControlFixer.DockedSubs)
	//	//						{
	//	//							if (keyValuePair2.Value != null && FastHelper.IsNear(keyValuePair2.Value.position, transform.position))
	//	//							{
	//	//								list.Add(keyValuePair2.Key);
	//	//							}
	//	//						}
	//	//						foreach (string key in list)
	//	//						{
	//	//							SubControlFixer.DockedSubs.Remove(key);
	//	//						}
	//	//						BaseFixer.BaseParts.Remove(item);
	//	//					}
	//	//				}
	//	//				UnityEngine.Object.Destroy(__instance.gameObject);
	//	//				if (keyValuePair.Value.Count > 0)
	//	//				{
	//	//					foreach (Base @base in keyValuePair.Value)
	//	//					{
	//	//						@base.RebuildGeometry();
	//	//					}
	//	//				}
	//	//			}
	//	//			__result = true;
	//	//			return false;
	//	//		}
	//	//	}
	//	//	return true;
	//	//}

 //       [HarmonyPatch(nameof(Constructable.InitializeModelCopy))]
	//	[HarmonyPrefix]
	//	public static bool InitializeModelCopy_Prefix(ConstructableBase __instance, ref bool __result)
	//	{
	//		foreach (BasePart basePart in BasePatcher.BaseParts)
	//		{
	//			GameObject gameObject = __instance.gameObject;
	//			if (!(((gameObject != null) ? gameObject.transform : null) != null) || !WorldHelpers.IsNear(__instance.gameObject.transform.position, basePart.position))
	//			{
	//				GameObject model = __instance.model;
	//				if (!(((model != null) ? model.transform : null) != null) || !WorldHelpers.IsNear(__instance.model.transform.position, basePart.position))
	//				{
	//					continue;
	//				}
	//			}
	//			_ReplaceMaterials.Invoke(__instance, new object[]
	//			{
	//				__instance.model
	//			});
	//			GameObject model2 = __instance.model;
	//			GameObject gameObject2 = new GameObject("BaseConnectorHatch");
	//			gameObject2.transform.parent = model2.transform;
	//			gameObject2.transform.localPosition = Vector3.zero;
	//			gameObject2.transform.localRotation = Quaternion.identity;
	//			gameObject2.transform.localScale = Vector3.one;
 //               BasePatcher.SetupStairCase(model2.transform);
	//			_modelCopy.SetValue(__instance, model2);
	//			__result = true;
	//			return false;
	//		}
	//		return true;
	//	}


	//	private static readonly MethodInfo _GetConstructInterval = typeof(Constructable).GetMethod("GetConstructInterval", BindingFlags.Static | BindingFlags.NonPublic);
 //       private static readonly MethodInfo _UpdateMaterial = typeof(Constructable).GetMethod("UpdateMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
 //       private static readonly MethodInfo _ReplaceMaterials = typeof(Constructable).GetMethod("ReplaceMaterials", BindingFlags.Instance | BindingFlags.NonPublic);
 //       private static readonly FieldInfo _resourceMapField = typeof(Constructable).GetField("resourceMap", BindingFlags.Instance | BindingFlags.NonPublic);

	//	private static readonly FieldInfo _modelCopy = typeof(Constructable).GetField("modelCopy", BindingFlags.Instance | BindingFlags.NonPublic);
	//}

 //   internal class BasePart
 //   {
	//	public BasePart()
	//	{
	//		this.id = null;
	//		this.cell = Int3.zero;
	//		this.index = -1;
	//		this.position = Vector3.zero;
	//		this.type = -1;
	//		this.root = null;
	//		this.dock = null;
	//		this.sub = null;
 //       }

	//	// Token: 0x06000016 RID: 22 RVA: 0x000021C0 File Offset: 0x000003C0
	//	public BasePart(string id, Int3 cell, int index, Vector3 position, int type, string dock, BaseRoot root)
	//	{
	//		this.id = id;
	//		this.cell = cell;
	//		this.index = index;
	//		this.position = position;
	//		this.type = type;
	//		this.root = root;
	//		this.dock = dock;
 //       }

	//	// Token: 0x06000017 RID: 23 RVA: 0x00002240 File Offset: 0x00000440
	//	public Transform GetDockingHatch()
	//	{
	//		if (this.root != null)
	//		{
	//			Base component = this.root.GetComponent<Base>();
	//			if (component != null)
	//			{
	//				foreach (Int3 @int in component.AllCells)
	//				{
	//					Transform cellObject;
	//					if ((cellObject = component.GetCellObject(@int)) != null && WorldHelpers.IsNear(cellObject.position, this.position))
	//					{
 //                           foreach (Transform transform in cellObject)
 //                           {
 //                               if (transform.name.StartsWith("CyclopsDockingHatchClean"))
 //                               {
 //                                   return transform;
 //                               }
	//						}
 //                       }
	//				}
	//			}
	//		}
	//		return null;
	//	}


 //       // Token: 0x0400000A RID: 10
 //       public string id;

 //       // Token: 0x0400000B RID: 11
 //       public Int3 cell;

 //       // Token: 0x0400000C RID: 12
 //       public int index;

 //       // Token: 0x0400000D RID: 13
 //       public Vector3 position;

 //       // Token: 0x0400000E RID: 14
 //       public int type;

 //       // Token: 0x0400000F RID: 15
 //       public BaseRoot root;
 //       // Token: 0x04000010 RID: 16
 //       public string dock;

 //       // Token: 0x04000011 RID: 17
 //       public SubRoot sub;

	//}
}
