using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using HarmonyLib;
using QModManager.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_AlterraHub.API
{
	public static class EasyCraft_API
	{
		public enum ReturnSurplus
		{
			Inventory,
			Lockers
		}

		public enum NeighboringStorage
		{
			Off,
			Inside,
			Range100
		}

		private static IQMod EasyCraft;
		private static Type SettingsType;
		private static Type EasyCraftMain;
		private static Type ClosestItemContainers;
		private static PropertyInfo SettingsProperty;
		private static object SettingsObject => SettingsProperty?.GetValue(null);

		public static bool GetAutoCraft() => (bool)SettingsType?.GetField("autoCraft")?.GetValue(SettingsObject);
		public static NeighboringStorage GetUseStorage() => (NeighboringStorage)SettingsType?.GetField("useStorage")?.GetValue(SettingsObject);
		public static ReturnSurplus GetReturnSurplus() => (ReturnSurplus)SettingsType?.GetField("returnSurplus")?.GetValue(SettingsObject);

		public static void Init(Harmony harmony)
		{
			EasyCraft = QModServices.Main.FindModById("EasyCraft");
			Assembly assembly = EasyCraft.LoadedAssembly;
			SettingsType = assembly.GetType("EasyCraft.Settings", true);
			EasyCraftMain = assembly.GetType("EasyCraft.Main", true);
			SettingsProperty = EasyCraftMain.GetProperty("Settings", SettingsType);


			ClosestItemContainers = assembly.GetType("EasyCraft.ClosestItemContainers", true);
			MethodInfo Find = AccessTools.Method(ClosestItemContainers, "Find");

			harmony.Patch(Find, postfix: new HarmonyMethod(AccessTools.Method(typeof(EasyCraft_API), nameof(EasyCraft_API.FindPostfix))));

		}

		public static void FindPostfix(ref ItemsContainer[] __result)
		{
			NeighboringStorage useStorage = GetUseStorage();

			if (useStorage != NeighboringStorage.Off)
			{
				List<ItemsContainer> list = new List<ItemsContainer>();
				FcsDevice[] array = new FcsDevice[0];

				if (useStorage == NeighboringStorage.Inside && Player.main.IsInside())
				{
					if (Player.main.currentEscapePod != null)
					{
						array = Player.main.currentEscapePod.GetComponentsInChildren<FcsDevice>();
					}
					else if (Player.main.IsInSub())
					{
						array = Player.main.currentSub.GetComponentsInChildren<FcsDevice>();
					}
				}
				else if (useStorage == NeighboringStorage.Range100)
				{
					if ((Player.main.transform.position - EscapePod.main.transform.position).sqrMagnitude < 10000f)
					{
						array = EscapePod.main.GetComponentsInChildren<FcsDevice>();
					}
					foreach (SubRoot subRoot in UnityEngine.Object.FindObjectsOfType<SubRoot>())
					{
						Vector3 position = Player.main.transform.position;
						BaseRoot baseRoot;
						if ((subRoot.isCyclops && (position - subRoot.GetWorldCenterOfMass()).sqrMagnitude < 10000f) || (subRoot.isBase && (baseRoot = (subRoot as BaseRoot)) != null && GetDistanceToPlayer(baseRoot) < 100f))
						{
							foreach (FcsDevice fcsDevice in subRoot.GetComponentsInChildren<FcsDevice>())
							{
								var storage = fcsDevice.GetStorage();
								if (storage != null && storage.ItemsContainer.containerType == ItemsContainerType.Default && fcsDevice.IsConstructed)
								{
									list.Add(storage.ItemsContainer);
								}
							}
						}
					}
					foreach (FcsDevice fcsDevice in UnityEngine.Object.FindObjectsOfType<FcsDevice>())
					{
						FCSStorage storage = fcsDevice.GetStorage();
						if (storage != null && storage.ItemsContainer.containerType == ItemsContainerType.Default && (Player.main.transform.position - fcsDevice.GetPosition()).sqrMagnitude < 10000f)
						{
							list.Add(storage.ItemsContainer);
						}
					}
				}

				foreach (FcsDevice fcsDevice in array)
				{
					FCSStorage storage = fcsDevice.GetStorage();
					if (storage != null && storage.ItemsContainer.containerType == ItemsContainerType.Default && fcsDevice.IsConstructed)
					{
						list.Add(storage.ItemsContainer);
					}
				}

				__result = (from x in list.Distinct<ItemsContainer>()
							orderby (Player.main.transform.position - x.tr.position).sqrMagnitude
							select x).Concat(__result).ToArray<ItemsContainer>();

				list.Clear();
			}
		}
		private static float GetDistanceToPlayer(BaseRoot baseRoot)
		{
			Int3 u = baseRoot.baseComp.WorldToGrid(Player.main.transform.position);
			int num = int.MaxValue;
			Int3 cell = new Int3(int.MaxValue);
			foreach (Int3 @int in baseRoot.baseComp.AllCells)
			{
				int num2 = (u - @int).SquareMagnitude();
				if (num2 < num)
				{
					cell = @int;
					num = num2;
				}
			}
			Vector3 b;
			baseRoot.baseComp.GridToWorld(cell, UWE.Utils.half3, out b);
			return (Player.main.transform.position - b).magnitude;
		}
	}
}
