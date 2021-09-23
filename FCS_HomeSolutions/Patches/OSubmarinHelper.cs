using System.Collections.Generic;
using System.Reflection;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Patches
{
    [HarmonyPatch(typeof(Builder))]
    class OSubmarinHelper
    {
		//private static GameObject ghostModel;
		private static readonly FieldInfo _ghostModel = typeof(Builder).GetField("ghostModel", BindingFlags.NonPublic | BindingFlags.Static);
		//private static LayerMask placeLayerMask;
		private static readonly FieldInfo _placeLayerMask = typeof(Builder).GetField("placeLayerMask", BindingFlags.NonPublic | BindingFlags.Static);
		//private static float placeMaxDistance;
		private static readonly FieldInfo _placeMaxDistance = typeof(Builder).GetField("placeMaxDistance", BindingFlags.NonPublic | BindingFlags.Static);

		private static readonly Vector3 NorthLadder = new Vector3(0.0f, -2.02f, 3.5f);
        private static readonly Vector3 SouthLadder = new Vector3(0.0f, 3.26f, 4.62f);
        private static readonly Vector3 EastLadder = new Vector3(-4.618f, 3.26f, 0.0f);
        private static readonly Vector3 WestLadder = new Vector3(4.618f, 3.26f, 0.0f);

        private static int CurrentDirection = 0;

        private static readonly Dictionary<int, KeyValuePair<Vector3, float>> LadderPositions = new Dictionary<int, KeyValuePair<Vector3, float>>()
        {
            { 0, new KeyValuePair<Vector3, float>(NorthLadder, 0.0f) },
            { 1, new KeyValuePair<Vector3, float>(EastLadder, 90.0f) },
            { 2, new KeyValuePair<Vector3, float>(SouthLadder, 180.0f) },
            { 3, new KeyValuePair<Vector3, float>(WestLadder, 270.0f) }
        };


		public static readonly Dictionary<Vector3, KeyValuePair<int, bool>> TempLadderDirections = new Dictionary<Vector3, KeyValuePair<int, bool>>();
        private static GameObject _objPositioner;

        private static void Rotate()
        {
            bool scrollUp = Input.GetAxis("Mouse ScrollWheel") > 0.0f;
            bool scrollDown = Input.GetAxis("Mouse ScrollWheel") < 0.0f;
            if (scrollDown || scrollUp)
            {
                if (scrollDown)
                {
                    CurrentDirection--;
                    if (CurrentDirection < 0)
                        CurrentDirection = 3;
                }
                else
                {
                    CurrentDirection++;
                    if (CurrentDirection > 3)
                        CurrentDirection = 0;
                }
            }
        }

		private static void PositionLadder(ref Vector3 position, ref Quaternion rotation, Transform foundation, bool initialInverted = false)
		{
			rotation = foundation.rotation;
			Vector3 newAngles = rotation.eulerAngles;
            int direction = CurrentDirection;


            position = Translate(foundation.position, foundation.rotation, LadderPositions[direction].Key);
            rotation.eulerAngles = new Vector3(newAngles.x, newAngles.y, newAngles.z);
			//TempLadderDirections[position] = new KeyValuePair<int, bool>(direction, false);
		}
		
		private static bool PlaceOutdoorLadder(GameObject targetObj, Vector3 hitPoint, ref Vector3 position, ref Quaternion rotation)
		{
			if (targetObj != null)
			{
				// Get initial inverted.
				bool initialInverted = targetObj.transform.position.y > Player.main.camRoot.transform.position.y;

                // Apply rotation on mouse wheel.
                //Rotate();

                // Set ladder orientation and position.
                PositionLadder(ref position, ref rotation, targetObj.transform, initialInverted);
				// Return false to prevent origin function call.
				return false;
			}
			// Give back execution to origin function.
			return true;
		}


        [HarmonyPatch(nameof(Builder.SetPlaceOnSurface))]
        [HarmonyPrefix]
        public static bool SetPlaceOnSurface_Prefix(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
        {
            if (!QPatch.Configuration.IsHatchStairwayEnabled) return true;

			GameObject ghostModel = (GameObject)_ghostModel.GetValue(null);
            QuickLogger.Debug($"SetPlaceOnbSurface : {hit.collider?.gameObject?.name} | {ghostModel.name}", true);
			// If object being built is our Outdoor Ladder.
			if (ghostModel?.name != null && ghostModel.name.StartsWith("StairsModel"))
			{
                QuickLogger.Debug("Is Stairs SETP", true);
				// If our Outdoor Ladder is being placed on a Foundation.
                if (hit.collider?.gameObject?.name != null && hit.collider.gameObject.name.StartsWith("hatch_end_anim") && !hit.collider.gameObject.name.StartsWith("hatch_side_anim"))
                {
                    QuickLogger.Debug("Is Hatch", true);
					return PlaceOutdoorLadder(hit.collider.gameObject.transform?.parent?.parent?.gameObject, hit.point, ref position, ref rotation);
                }
			}
			// Give back execution to origin function.
			return true;
		}

        [HarmonyPatch(nameof(Builder.CheckSurfaceType))]
        [HarmonyPostfix]
        public static void CheckSurfaceType_Postfix(ref bool __result, SurfaceType surfaceType)
		{
            if (!QPatch.Configuration.IsHatchStairwayEnabled) return ;

            QuickLogger.Debug($"CheckSurfaceType_Postfix : {__result}", true);
            GameObject ghostModel = (GameObject)_ghostModel.GetValue(null);
			if (!__result)
			{
				// If there's a hit and object being built is our Outdoor Ladder.
				if (ghostModel?.name != null && ghostModel.name.StartsWith("StairsModel"))
				{
					QuickLogger.Debug("Is Stairs",true);
					Transform aimTransform = Builder.GetAimTransform();
					float pmd = (float)_placeMaxDistance.GetValue(null);
					LayerMask lm = (LayerMask)_placeLayerMask.GetValue(null);
					bool allowed = false;

					// If our Outdoor Ladder is being placed on a foundation.
					if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value, QueryTriggerInteraction.Ignore))
                        if (hit.collider?.gameObject != null && hit.collider.gameObject.name.StartsWith("hatch_end_anim") && !hit.collider.gameObject.name.StartsWith("hatch_side_anim"))
                        {
                            QuickLogger.Debug("Allowed to build", true);
							allowed = true;
                            __result = true;
                        }
					if (!allowed)
						__result = false;
				}
			}
            else
            {
                if (ghostModel?.name != null && ghostModel.name.StartsWith("StairsModel"))
                {
                    QuickLogger.Debug("Is Stairs", true);
                    Transform aimTransform = Builder.GetAimTransform();
                    float pmd = (float)_placeMaxDistance.GetValue(null);
                    LayerMask lm = (LayerMask)_placeLayerMask.GetValue(null);
                    // If our Outdoor Ladder is being placed on a foundation.
                    if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value, QueryTriggerInteraction.Ignore))
                        if (hit.collider?.gameObject != null && !hit.collider.gameObject.name.StartsWith("hatch_end_anim") && !hit.collider.gameObject.name.StartsWith("hatch_side_anim"))
                        {
                            __result = false;
                        }
                }
            }
        }

        public static Vector3 Translate(Vector3 pos, Quaternion rot, Vector3 to, Space space = Space.Self)
        {
            Vector3 result = pos;
            if (_objPositioner == null)
                _objPositioner = new GameObject("dummyPositioner");
            if (_objPositioner != null)
            {
                _objPositioner.transform.position = pos;
                _objPositioner.transform.rotation = rot;
                _objPositioner.transform.Translate(to, space);
                result = _objPositioner.transform.position;
            }
            return result;
        }
	}
}