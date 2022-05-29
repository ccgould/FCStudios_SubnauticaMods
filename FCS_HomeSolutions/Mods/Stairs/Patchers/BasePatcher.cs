using System.Collections.Generic;
using System.Reflection;
using FCS_HomeSolutions.Mods.Stairs.Buildable;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Stairs.Patchers
{
 //   [HarmonyPatch(typeof(Base), nameof(Base.BuildObservatoryGeometry))]
	//class BasePatcher
 //   {


 //       [HarmonyPostfix]
	//	public static void BuildObservatoryGeometry_Postfix(Base __instance, Int3 cell)
	//	{
	//		PrefabIdentifier component = __instance.GetComponent<PrefabIdentifier>();
	//		if (component != null)
	//		{
	//			Transform cellObject = __instance.GetCellObject(cell);

 //               SetupStairCase(cellObject);
	//			//if (BaseGhostPatcher.LBaseConnector != null && BaseGhostPatcher.LBaseConnector.Value.x == cell.x && BaseGhostPatcher.LBaseConnector.Value.y == cell.y && BaseGhostPatcher.LBaseConnector.Value.z == cell.z)
	//			//{
 //   //                BaseGhostPatcher.LBaseConnector = null;
	//			//	if (cellObject != null)
	//			//	{
	//			//		BaseParts.Add(new BasePart(component.Id, cell, index, cellObject.position, 0, null, __instance.GetComponent<BaseRoot>()));
 //   //                    SetupStairCase(cellObject);
 //   //                    return;
	//			//	}
	//			//}
	//			//else if (cellObject != null)
	//			//{
	//			//	foreach (BasePart basePart in BaseParts)
	//			//	{
	//			//		if (basePart.id == component.Id && (int)basePart.position.x == (int)cellObject.position.x && (int)basePart.position.y == (int)cellObject.position.y && (int)basePart.position.z == (int)cellObject.position.z)
	//			//		{
	//			//			if (basePart.root == null)
	//			//			{
	//			//				basePart.root = __instance.GetComponent<BaseRoot>();
	//			//			}
	//			//			SetupStairCase(cellObject);
 //   //                        break;
	//			//		}
	//			//	}
	//			//}
	//		}
	//	}

	//	public static void SetupStairCase(Transform t)
	//	{
	//		GameObject gameObject = InstantiateStairs();

	//		#region Omit

	//		//foreach (object obj in t)
	//		//{
	//		//	Transform transform = (Transform)obj;
	//		//	if (transform.name.StartsWith("BaseConnectorTube"))
	//		//	{
	//		//		SkyApplier[] components = transform.GetComponents<SkyApplier>();
	//		//		if (components != null && components.Length != 0)
	//		//		{
	//		//			foreach (SkyApplier skyApplier in components)
	//		//			{
	//		//				if (skyApplier.renderers != null)
	//		//				{
	//		//					foreach (Renderer renderer in skyApplier.renderers)
	//		//					{
	//		//						if (renderer.name.StartsWith("BaseCorridor"))
	//		//						{
	//		//							using (IEnumerator<Transform> enumerator2 = gameObject.transform.GetEnumerator())
	//		//							{
	//		//								while (enumerator2.MoveNext())
	//		//								{
	//		//									object obj2 = enumerator2.Current;
	//		//									Transform transform2 = (Transform)obj2;
	//		//									if (transform2.name.StartsWith("SmallBaseTube"))
	//		//									{
	//		//										MeshRenderer componentInChildren = transform2.GetComponentInChildren<MeshRenderer>();
	//		//										if (componentInChildren != null)
	//		//										{
	//		//											componentInChildren.material = renderer.material;
	//		//											SkyApplier skyApplier2 = gameObject.AddComponent<SkyApplier>();
	//		//											skyApplier2.renderers = new Renderer[]
	//		//											{
	//		//												componentInChildren
	//		//											};
	//		//											skyApplier2.anchorSky = skyApplier.anchorSky;
	//		//											skyApplier2.customSkyPrefab = skyApplier.customSkyPrefab;
	//		//											skyApplier2.dynamic = skyApplier.dynamic;
	//		//											skyApplier2.emissiveFromPower = skyApplier.emissiveFromPower;
	//		//											skyApplier2.updaterIndex = skyApplier.updaterIndex;
	//		//											break;
	//		//										}
	//		//										break;
	//		//									}
	//		//								}
	//		//								break;
	//		//							}
	//		//						}
	//		//					}
	//		//				}
	//		//			}
	//		//			break;
	//		//		}
	//		//		break;
	//		//	}
	//		//}

	//		#endregion

	//		if (gameObject != null)
	//		{
	//			gameObject.transform.parent = t;
	//			gameObject.transform.localPosition = Vector3.zero;
	//			gameObject.transform.localRotation = Quaternion.identity;
	//			gameObject.transform.localScale = Vector3.one;
	//		}
	//	}

 //       private static GameObject InstantiateStairs()
 //       {
 //           return GameObject.Instantiate(StairsBuildable.Prefab);
 //       }

	//	public static List<BasePart> BaseParts { get; set; }
 //   }
}
