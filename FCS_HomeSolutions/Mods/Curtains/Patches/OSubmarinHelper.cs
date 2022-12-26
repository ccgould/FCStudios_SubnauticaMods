//using System.Collections.Generic;
//using System.Reflection;
//using FCS_AlterraHub.Helpers;
//using FCSCommon.Utilities;
//using HarmonyLib;
//using UnityEngine;

//namespace FCS_HomeSolutions.Mods.Curtains.Patches
//{
//    [HarmonyPatch(typeof(Builder))]
//    class OSubmarinHelper
//    {
//		//private static GameObject ghostModel;
//		private static readonly FieldInfo _ghostModel = typeof(Builder).GetField("ghostModel", BindingFlags.NonPublic | BindingFlags.Static);
//		//private static LayerMask placeLayerMask;
//		private static readonly FieldInfo _placeLayerMask = typeof(Builder).GetField("placeLayerMask", BindingFlags.NonPublic | BindingFlags.Static);
//		//private static float placeMaxDistance;
//		private static readonly FieldInfo _placeMaxDistance = typeof(Builder).GetField("placeMaxDistance", BindingFlags.NonPublic | BindingFlags.Static);
//        private static string ModelName => "Curtain";
//        private static string DrillTargetObjectName => "BaseRoomWindowSide";

//        private static readonly Vector3 Placement1 = new Vector3(0.0f, 0f, 0f);

//        private static readonly Dictionary<int, KeyValuePair<Vector3, float>> DrillPlacementPositions = new()
//        {
//            { 1, new KeyValuePair<Vector3, float>(Placement1, 0.0f) },
//        };


//        private static GameObject _objPositioner;

//		private static void PositionObject(ref Vector3 position, ref Quaternion rotation, Transform currentObject, bool initialInverted = false, int index= 0)
//		{
//			rotation = currentObject.rotation;
//			Vector3 newAngles = rotation.eulerAngles;
//            position = Translate(currentObject.position, currentObject.rotation, DrillPlacementPositions[index].Key);
//            rotation.eulerAngles = new Vector3(newAngles.x, newAngles.y + DrillPlacementPositions[index].Value, newAngles.z);
//        }
		
//		private static bool PlaceObject(GameObject targetObj, int index, ref Vector3 position, ref Quaternion rotation)
//		{
//			if (targetObj != null)
//			{
//				// Get initial inverted.
//				bool initialInverted = targetObj.transform.position.y > Player.main.camRoot.transform.position.y;

//                // Set ladder orientation and position.
//                PositionObject(ref position, ref rotation, targetObj.transform, initialInverted,index);
//				// Return false to prevent origin function call.
//				return false;
//			}

//			// Give back execution to origin function.
//			return true;
//		}


//        [HarmonyPatch(nameof(Builder.SetPlaceOnSurface))]
//        [HarmonyPrefix]
//        public static bool SetPlaceOnSurface_Prefix( RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
//        {
//            if (!Main.Configuration.IsCurtainEnabled) return true;
//			GameObject ghostModel = (GameObject)_ghostModel.GetValue(null);
//            QuickLogger.Debug($"SetPlaceOnSurface {ghostModel.name}", true);
//            if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
//            {
//                var root = GameObjectHelpers.FindParentWithName(hit.collider.gameObject, DrillTargetObjectName);

//                QuickLogger.Debug($"Found Root: {root?.gameObject?.name}", true);

//                if (!string.IsNullOrWhiteSpace(root?.name) && (root.name.StartsWith(DrillTargetObjectName)))
//                { 
//                    QuickLogger.Debug("Is Curtain", true);
//					return PlaceObject(root, 1, ref position, ref rotation);
//                }
//            }
//			// Give back execution to origin function.
//			return true;
//		}

//        [HarmonyPatch(nameof(Builder.UpdateAllowed))]
//        [HarmonyPrefix]
//        public static bool UpdateAllowed_Postfix(ref bool __result)
//        {
//            if (!Main.Configuration.IsCurtainEnabled) return true;

//            GameObject ghostModel = (GameObject)_ghostModel.GetValue(null);
//            if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
//            {
//                Transform aimTransform = Builder.GetAimTransform();
//                float pmd = (float)_placeMaxDistance.GetValue(null);
//                LayerMask lm = (LayerMask)_placeLayerMask.GetValue(null);

//                    if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value,
//                        QueryTriggerInteraction.Ignore))
//                    {
//                        var root = GameObjectHelpers.FindParentWithName(hit.collider.gameObject, DrillTargetObjectName);


//                    if (!string.IsNullOrWhiteSpace(root?.name) && (root.name.StartsWith(DrillTargetObjectName)))
//                    {
//                            var result = Builder.CheckAsSubModule();
//                            if (!result)
//                            {
//                                result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, hit.collider);
//                            }

//                            __result = result;
//                            return false;
//                        }
//                    }
//            }
//            return true;
//        }
        
//        [HarmonyPatch(nameof(Builder.CheckSurfaceType))]
//        [HarmonyPostfix]
//        public static void CheckSurfaceType_Postfix(ref bool __result, SurfaceType surfaceType)
//		{
//            if (!Main.Configuration.IsCurtainEnabled) return;

//            GameObject ghostModel = (GameObject)_ghostModel.GetValue(null);

//			if (__result)
//			{
//				// If there's a hit and object being built is our Outdoor Ladder.
//				if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
//				{
//                    Transform aimTransform = Builder.GetAimTransform();
//					float pmd = (float)_placeMaxDistance.GetValue(null);
//					LayerMask lm = (LayerMask)_placeLayerMask.GetValue(null);
//					bool allowed = false;

//					// If our Outdoor Ladder is being placed on a foundation.
//                    if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value,
//                        QueryTriggerInteraction.Ignore))
//                    {
//                        var root = GameObjectHelpers.FindParentWithName(hit.collider.gameObject, DrillTargetObjectName);


//                        if (!string.IsNullOrWhiteSpace(root?.name) && (root.name.StartsWith(DrillTargetObjectName)))
//                        {
//                            QuickLogger.Debug("Allowed");
//                            allowed = true;
//                                __result = true;
//                            }
//                        if (!allowed)
//                            __result = false;
//                    }
//                }
//			}
//        }

//        public static Vector3 Translate(Vector3 pos, Quaternion rot, Vector3 to, Space space = Space.Self)
//        {
//            Vector3 result = pos;
//            if (_objPositioner == null)
//                _objPositioner = new GameObject("dummyPositioner");

//            if (_objPositioner != null)
//            {
//                _objPositioner.transform.position = pos;
//                _objPositioner.transform.rotation = rot;
//                _objPositioner.transform.Translate(to, space);
//                result = _objPositioner.transform.position;
//            }
//            return result;
//        }
//	}
//}