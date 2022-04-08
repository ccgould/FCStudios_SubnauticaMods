using System.Collections.Generic;
using System.Reflection;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Patchers
{
    [HarmonyPatch(typeof(Builder))]
    class OSubmarinHelper
    {
        //private static GameObject ghostModel;
        private static readonly FieldInfo _ghostModel =
            typeof(Builder).GetField("ghostModel", BindingFlags.NonPublic | BindingFlags.Static);

        //private static LayerMask placeLayerMask;
        private static readonly FieldInfo _placeLayerMask =
            typeof(Builder).GetField("placeLayerMask", BindingFlags.NonPublic | BindingFlags.Static);

        //private static float placeMaxDistance;
        private static readonly FieldInfo _placeMaxDistance =
            typeof(Builder).GetField("placeMaxDistance", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly FieldInfo _isRotationEnabled =
            typeof(Builder).GetField("rotationEnabled", BindingFlags.NonPublic | BindingFlags.Static);

        private static string ModelName => "DrillModel";
        private static string DrillTargetObjectName => "Unity_Collison_Object_0";
        private static string PlatformTargetObjectName => "BaseFoundationPlatform";

        private static readonly Vector3 NorthDrillPlacement = new Vector3(0.0f, 0.0f, 5.518f);
        private static readonly Vector3 SouthDrillPlacement = new Vector3(0.0f, 0.0f, -5.518f);
        private static readonly Vector3 EastDrillPlacement = new Vector3(5.446f, 0.0f, 0.0f);
        private static readonly Vector3 WestDrillPlacement = new Vector3(-5.452f, 0.0f, 0.0f);

        private static readonly Vector3 NorthPlatformPlacement = new Vector3(0.0f, 1.0f, 7.75f);
        private static readonly Vector3 SouthPlatformPlacement = new Vector3(0.0f, 1.0f, -7.75f);
        private static readonly Vector3 EastPlatformPlacement = new Vector3(7.75f, 1.0f, 0.0f);
        private static readonly Vector3 WestPlatformPlacement = new Vector3(-7.75f, 1.0f, 0.0f);
        private static readonly Vector3 CenterPlatformPlacement = new Vector3(0.0f, 1.0f, 0.0f);

        private static int CurrentDirection = 0;

        private static readonly Dictionary<int, KeyValuePair<Vector3, float>> DrillPlacementPositions =
            new Dictionary<int, KeyValuePair<Vector3, float>>()
            {
                {0, new KeyValuePair<Vector3, float>(NorthDrillPlacement, -180.0f)},
                {1, new KeyValuePair<Vector3, float>(EastDrillPlacement, 0.0f)},
                {2, new KeyValuePair<Vector3, float>(SouthDrillPlacement, 180.0f)},
                {3, new KeyValuePair<Vector3, float>(WestDrillPlacement, 0.0f)}
            };

        private static readonly Dictionary<int, KeyValuePair<Vector3, float>> PlatformPlacementPositions =
            new Dictionary<int, KeyValuePair<Vector3, float>>()
            {
                {0, new KeyValuePair<Vector3, float>(CenterPlatformPlacement, 90.0f)},
                {1, new KeyValuePair<Vector3, float>(NorthPlatformPlacement, 180.0f)},
                {2, new KeyValuePair<Vector3, float>(EastPlatformPlacement, -90.0f)},
                {3, new KeyValuePair<Vector3, float>(SouthPlatformPlacement, 0.0f)},
                {4, new KeyValuePair<Vector3, float>(WestPlatformPlacement, 90.0f)},
            };

        private static GameObject _objPositioner;

        private static void Rotate(bool isPlatform)
        {
            bool scrollUp = Input.GetAxis("Mouse ScrollWheel") > 0.0f;
            bool scrollDown = Input.GetAxis("Mouse ScrollWheel") < 0.0f;
            if (scrollDown || scrollUp)
            {
                if (isPlatform)
                {
                    if (scrollDown)
                    {
                        CurrentDirection--;
                        if (CurrentDirection < 0)
                            CurrentDirection = 4;
                    }
                    else
                    {
                        CurrentDirection++;
                        if (CurrentDirection > 4)
                            CurrentDirection = 0;
                    }
                }
                else
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
        }

        private static void PositionObject(ref Vector3 position, ref Quaternion rotation, Transform currentObject,
            bool initialInverted = false, bool isPlatform = false)
        {
            rotation = currentObject.rotation;
            Vector3 newAngles = rotation.eulerAngles;
            int direction = CurrentDirection;


            if (isPlatform)
            {
                position = Translate(currentObject.position, currentObject.rotation,
                    PlatformPlacementPositions[direction].Key);
                rotation.eulerAngles = new Vector3(newAngles.x,
                    newAngles.y + PlatformPlacementPositions[direction].Value, newAngles.z);
            }
            else
            {
                position = Translate(currentObject.position, currentObject.rotation,
                    DrillPlacementPositions[direction].Key);
                rotation.eulerAngles = new Vector3(newAngles.x, newAngles.y + DrillPlacementPositions[direction].Value,
                    newAngles.z);
            }
        }

        private static bool PlaceObject(GameObject targetObj, Vector3 hitPoint, ref Vector3 position,
            ref Quaternion rotation, bool isPlatform)
        {
            if (targetObj != null)
            {
                // Get initial inverted.
                bool initialInverted = targetObj.transform.position.y > Player.main.camRoot.transform.position.y;

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    _isRotationEnabled.SetValue(null, true);
                }
                else
                {
                    // Apply rotation on mouse wheel.
                    Rotate(isPlatform);
                    _isRotationEnabled.SetValue(null, false);
                }

                // Set ladder orientation and position.
                PositionObject(ref position, ref rotation, targetObj.transform, initialInverted, isPlatform);
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
            if (!QPatch.Configuration.IsDeepDrillerEnabled) return true;

            GameObject ghostModel = (GameObject) _ghostModel.GetValue(null);
            QuickLogger.Debug($"SetPlaceOnbSurface : {hit.collider?.gameObject?.name} | {ghostModel.name}", true);
            // If object being built is our drill.
            if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
            {
                QuickLogger.Debug("Is Drill SETP", true);
                // If our drill is being placed on a Foundation.
                if (hit.collider?.gameObject?.name != null &&
                    (hit.collider.gameObject.name.StartsWith(DrillTargetObjectName)) ||
                    hit.collider.gameObject.name.StartsWith(PlatformTargetObjectName))
                {
                    QuickLogger.Debug("Is Drill", true);
                    return PlaceObject(hit.collider.gameObject.transform?.parent?.parent?.gameObject, hit.point,
                        ref position, ref rotation, hit.collider.gameObject.name.StartsWith(PlatformTargetObjectName));
                }
                else
                {
                    if (!(bool) _isRotationEnabled.GetValue(null))
                    {
                        _isRotationEnabled.SetValue(null, true);
                    }
                }
            }

            // Give back execution to origin function.
            return true;
        }


        [HarmonyPatch(nameof(Builder.UpdateAllowed))]
        [HarmonyPrefix]
        public static bool UpdateAllowed_Postfix(ref bool __result)
        {
            QuickLogger.Debug($"CheckAsSubModule Result = {__result}", true);
            GameObject ghostModel = (GameObject) _ghostModel.GetValue(null);
            if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
            {
                Transform aimTransform = Builder.GetAimTransform();
                float pmd = (float) _placeMaxDistance.GetValue(null);
                LayerMask lm = (LayerMask) _placeLayerMask.GetValue(null);

                if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value,
                        QueryTriggerInteraction.Ignore))
                    if (hit.collider?.gameObject != null &&
                        (hit.collider.gameObject.name.StartsWith(DrillTargetObjectName) ||
                         hit.collider.gameObject.name.StartsWith(PlatformTargetObjectName)))
                    {
                        var result = Builder.CheckAsSubModule(
#if !SUBNAUTICA_STABLE
                            out _
#endif
                        );
                        if (!result)
                        {
                            //List<GameObject> list = new List<GameObject>();
                            //Builder.GetObstacles(Builder.placePosition, Builder.placeRotation, Builder.bounds, list);
                            //result = (list.Count == 0);

                            //QuickLogger.Debug($"Is Colliding: {result}",true);

                            //list.Clear();
                            result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds,
                                Builder.placeLayerMask.value, hit.collider);
                        }

                        __result = result;
                        return false;
                    }
            }

            QuickLogger.Debug($"CheckAsSubModule Result = {__result}", true);
            return true;
        }

        [HarmonyPatch(nameof(Builder.Update))]
        [HarmonyPostfix]
        public static void Update_Postfix()
        {
            GameObject ghostModel = (GameObject) _ghostModel.GetValue(null);
            if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
            {
                Transform aimTransform = Builder.GetAimTransform();
                float pmd = (float) _placeMaxDistance.GetValue(null);
                LayerMask lm = (LayerMask) _placeLayerMask.GetValue(null);

                if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value,
                        QueryTriggerInteraction.Ignore))
                    if (hit.collider?.gameObject != null &&
                        (!hit.collider.gameObject.name.StartsWith(DrillTargetObjectName) ||
                         hit.collider.gameObject.name.StartsWith(PlatformTargetObjectName)))
                    {
                        _isRotationEnabled.SetValue(null, true);
                        return;
                    }

                QuickLogger.Debug("Drill Builder Update", true);
            }
        }

        [HarmonyPatch(nameof(Builder.CheckSurfaceType))]
        [HarmonyPostfix]
        public static void CheckSurfaceType_Postfix(ref bool __result, SurfaceType surfaceType)
        {
            if (!QPatch.Configuration.IsDeepDrillerEnabled) return;

            QuickLogger.Debug($"CheckSurfaceType_Postfix : {__result}", true);
            GameObject ghostModel = (GameObject) _ghostModel.GetValue(null);
            if (__result)
            {
                // If there's a hit and object being built is our Outdoor Ladder.
                if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
                {
                    QuickLogger.Debug("Is drill", true);
                    Transform aimTransform = Builder.GetAimTransform();
                    float pmd = (float) _placeMaxDistance.GetValue(null);
                    LayerMask lm = (LayerMask) _placeLayerMask.GetValue(null);
                    bool allowed = false;

                    // If our Outdoor Ladder is being placed on a foundation.
                    if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value,
                            QueryTriggerInteraction.Ignore))
                        if (hit.collider?.gameObject != null &&
                            (hit.collider.gameObject.name.StartsWith(DrillTargetObjectName) ||
                             hit.collider.gameObject.name.StartsWith(PlatformTargetObjectName)))
                        {
                            QuickLogger.Debug("Allowed to build", true);
                            allowed = true;
                            __result = true;
                        }

                    if (!allowed)
                        __result = false;
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