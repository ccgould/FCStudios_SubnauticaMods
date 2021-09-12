using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Stairs.Patchers
{

  //  [HarmonyPatch(typeof(Builder), nameof(Builder.CreateGhost))]
  //  internal class BuilderCreateGhost_Patch
  //  {
  //      private static readonly FieldInfo _ghostModel = typeof(Builder).GetField("ghostModel", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _placeMaxDistance = typeof(Builder).GetField("placeMaxDistance", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _renderers = typeof(Builder).GetField("renderers", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _ghostStructureMaterial = typeof(Builder).GetField("ghostStructureMaterial", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _prefab = typeof(Builder).GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _constructableTechType = typeof(Builder).GetField("constructableTechType", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _placeMinDistance = typeof(Builder).GetField("placeMinDistance", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _placeDefaultDistance = typeof(Builder).GetField("placeDefaultDistance", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _placementTarget = typeof(Builder).GetField("placementTarget", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _allowedSurfaceTypes = typeof(Builder).GetField("allowedSurfaceTypes", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _forceUpright = typeof(Builder).GetField("forceUpright", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _allowedInSub = typeof(Builder).GetField("allowedInSub", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _allowedInBase = typeof(Builder).GetField("allowedInBase", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _allowedOutside = typeof(Builder).GetField("allowedOutside", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _allowedOnConstructables = typeof(Builder).GetField("allowedOnConstructables", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _rotationEnabled = typeof(Builder).GetField("rotationEnabled", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _ghostModelPosition = typeof(Builder).GetField("ghostModelPosition", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _ghostModelRotation = typeof(Builder).GetField("ghostModelRotation", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly FieldInfo _ghostModelScale = typeof(Builder).GetField("ghostModelScale", BindingFlags.Static | BindingFlags.NonPublic);
  //      private static readonly MethodInfo _InitBounds = typeof(Builder).GetMethod("InitBounds", BindingFlags.Static | BindingFlags.NonPublic);
  //      public const string BaseConnectorHatch = "BaseCorridorHatch";
  //      public static bool BaseConnector;


  //      [HarmonyPrefix]
  //      public static bool Prefix(ref bool __result)
		//{
			
		//	GameObject gameObject = (GameObject)_prefab.GetValue(null);

		//	if ((GameObject)_ghostModel.GetValue(null) != null || ((gameObject != null) ? gameObject.GetComponent<ConstructableBase>() : null) == null)
		//	{
		//		return true;
		//	}
		//	Constructable component = gameObject.GetComponent<Constructable>();

		//	if (!uGUI_BuilderMenuPatcherOnClickPatch.SelectedStairs || component.techType != TechType.BaseHatch)
		//	{
		//		return true;
		//	}

		//	BaseConnector = true;
		//	_constructableTechType.SetValue(null, component.techType);
		//	_placeMinDistance.SetValue(null, component.placeMinDistance);
		//	_placeMaxDistance.SetValue(null, component.placeMaxDistance);
		//	_placeDefaultDistance.SetValue(null, component.placeDefaultDistance);
		//	_allowedSurfaceTypes.SetValue(null, component.allowedSurfaceTypes);
		//	_forceUpright.SetValue(null, component.forceUpright);
		//	_allowedInSub.SetValue(null, component.allowedInSub);
		//	_allowedInBase.SetValue(null, component.allowedInBase);
		//	_allowedOutside.SetValue(null, component.allowedOutside);
		//	_allowedOnConstructables.SetValue(null, component.allowedOnConstructables);
		//	_rotationEnabled.SetValue(null, component.rotationEnabled);
		//	if ((bool)_rotationEnabled.GetValue(null))
		//	{
		//		Builder.ShowRotationControlsHint();
		//	}
		//	GameObject model = UnityEngine.Object.Instantiate(gameObject).GetComponent<ConstructableBase>().model;
  //          uGUI_BuilderMenuPatcherOnClickPatch.SelectedStairs = false;
		//	GameObject gameObject2 = new GameObject(BaseConnectorHatch);
		//	gameObject2.transform.parent = model.transform;
		//	gameObject2.transform.localPosition = Vector3.zero;
		//	gameObject2.transform.localRotation = Quaternion.identity;
		//	gameObject2.transform.localScale = Vector3.one;
		//	BasePatcher.SetupStairCase(model.transform);
		//	_ghostModel.SetValue(null, model);
		//	((GameObject)_ghostModel.GetValue(null)).GetComponent<BaseGhost>().SetupGhost();
		//	_ghostModelPosition.SetValue(null, Vector3.zero);
		//	_ghostModelRotation.SetValue(null, Quaternion.identity);
		//	_ghostModelScale.SetValue(null, Vector3.one);
		//	_renderers.SetValue(null, MaterialExtensions.AssignMaterial((GameObject)_ghostModel.GetValue(null), (Material)_ghostStructureMaterial.GetValue(null)));
		//	_InitBounds.Invoke(null, new object[]
		//	{
		//		(GameObject)_ghostModel.GetValue(null)
		//	});
		//	__result = true;
		//	return false;
		//}

  //      [HarmonyPrefix]
  //      public static void End_Prefix()
  //      {
  //          if (BaseConnector)
  //          {
  //              BaseConnector = false;
  //          }

  //          //var trans = ((GameObject)_ghostModel.GetValue(null))?.transform;
  //          //if (trans != null)
  //          //{
  //          //    BasePatcher.SetupStairCase(trans);
  //          //}
		//}

    //}
}
