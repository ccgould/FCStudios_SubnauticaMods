using System.Collections.Generic;
using System.Reflection;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.PartitionWalls.Patches
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

        private static string ModelName => "PartitionModel";
        private static string DrillTargetObjectName => "FCS_PartitionWall01_Ext";

        private static readonly Vector3 Placement1 = new Vector3(0.0f, 0f, 1.395f);
        private static readonly Vector3 Placement2 = new Vector3(1.298f, 0f, 0.06999993f);
        private static readonly Vector3 Placement3 = new Vector3(-1.298f, 0f, 0.06999993f);
        private static readonly Vector3 Placement4 = new Vector3(0f, 0f, -1.255f);
        private static readonly Vector3 Placement5 = new Vector3(2.919999f, 0f, 1.395f);
        private static readonly Vector3 Placement6 = new Vector3(-2.919999f, 0f, 1.395f);
        private static readonly Vector3 Placement7 = new Vector3(2.919999f, 0f, -1.255f);
        private static readonly Vector3 Placement8 = new Vector3(-2.919999f, 0f, -1.255f);
        private static readonly Vector3 Placement9 = new Vector3(1.298f, 0f, -2.8f);
        private static readonly Vector3 Placement10 = new Vector3(1.3f, 0f, 2.96f);
        private static readonly Vector3 Placement11 = new Vector3(-1.298f, 0f, -2.8f);
        private static readonly Vector3 Placement12 = new Vector3(-1.298f, 0f, 2.96f);

        private static readonly Dictionary<int, KeyValuePair<Vector3, float>> DrillPlacementPositions = new()
        {
            {1, new KeyValuePair<Vector3, float>(Placement1, 0.0f)},
            {2, new KeyValuePair<Vector3, float>(Placement2, 90.0f)},
            {3, new KeyValuePair<Vector3, float>(Placement3, 90.0f)},
            {4, new KeyValuePair<Vector3, float>(Placement4, 0.0f)},
            {5, new KeyValuePair<Vector3, float>(Placement5, 0.0f)},
            {6, new KeyValuePair<Vector3, float>(Placement6, 0.0f)},
            {7, new KeyValuePair<Vector3, float>(Placement7, 0.0f)},
            {8, new KeyValuePair<Vector3, float>(Placement8, 0.0f)},
            {9, new KeyValuePair<Vector3, float>(Placement9, 90.0f)},
            {10, new KeyValuePair<Vector3, float>(Placement10, 90.0f)},
            {11, new KeyValuePair<Vector3, float>(Placement11, 90.0f)},
            {12, new KeyValuePair<Vector3, float>(Placement12, 90.0f)},
        };


        private static GameObject _objPositioner;

        private static void PositionObject(ref Vector3 position, ref Quaternion rotation, Transform currentObject,
            bool initialInverted = false, int index = 0)
        {
            rotation = currentObject.rotation;
            Vector3 newAngles = rotation.eulerAngles;
            position = Translate(currentObject.position, currentObject.rotation, DrillPlacementPositions[index].Key);
            rotation.eulerAngles =
                new Vector3(newAngles.x, newAngles.y + DrillPlacementPositions[index].Value, newAngles.z);
        }

        private static bool PlaceObject(GameObject targetObj, int index, ref Vector3 position, ref Quaternion rotation)
        {
            if (targetObj != null)
            {
                // Get initial inverted.
                bool initialInverted = targetObj.transform.position.y > Player.main.camRoot.transform.position.y;

                // Set ladder orientation and position.
                PositionObject(ref position, ref rotation, targetObj.transform, initialInverted, index);
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
            if (!QPatch.Configuration.IsWallPartitionsEnabled) return true;
            GameObject ghostModel = (GameObject) _ghostModel.GetValue(null);
            if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
            {
                if (hit.collider?.gameObject?.name != null &&
                    (hit.collider.gameObject.name.StartsWith(DrillTargetObjectName)))
                {
                    QuickLogger.Debug("Is Partition", true);
                    return PlaceObject(hit.collider.gameObject.transform?.parent?.parent?.gameObject,
                        GetIndexOfObject(hit.collider.gameObject.name), ref position, ref rotation);
                }
            }

            // Give back execution to origin function.
            return true;
        }

        private static int GetIndexOfObject(string name)
        {
            var src = name.IndexOf('(') + 1;
            var dst = name.IndexOf(')');
            var result = name.Substring(src, dst - src);

            return int.TryParse(result, out var value) ? value : 0;
        }


        [HarmonyPatch(nameof(Builder.UpdateAllowed))]
        [HarmonyPrefix]
        public static bool UpdateAllowed_Postfix(ref bool __result)
        {
            if (!QPatch.Configuration.IsWallPartitionsEnabled) return true;

            GameObject ghostModel = (GameObject) _ghostModel.GetValue(null);
            if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
            {
                Transform aimTransform = Builder.GetAimTransform();
                float pmd = (float) _placeMaxDistance.GetValue(null);
                LayerMask lm = (LayerMask) _placeLayerMask.GetValue(null);

                if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value,
                        QueryTriggerInteraction.Ignore))
                    if (hit.collider?.gameObject != null &&
                        (hit.collider.gameObject.name.StartsWith(DrillTargetObjectName)))
                    {
                        var result = Builder.CheckAsSubModule(
#if !SUBNAUTICA_STABLE
                            out _
#endif
                        );
                        if (!result)
                        {
                            result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds,
                                Builder.placeLayerMask.value, hit.collider);
                        }

                        __result = result;
                        return false;
                    }
            }

            return true;
        }

        [HarmonyPatch(nameof(Builder.CheckSurfaceType))]
        [HarmonyPostfix]
        public static void CheckSurfaceType_Postfix(ref bool __result, SurfaceType surfaceType)
        {
            if (!QPatch.Configuration.IsWallPartitionsEnabled) return;

            GameObject ghostModel = (GameObject) _ghostModel.GetValue(null);

            if (__result)
            {
                // If there's a hit and object being built is our Outdoor Ladder.
                if (ghostModel?.name != null && ghostModel.name.StartsWith(ModelName))
                {
                    Transform aimTransform = Builder.GetAimTransform();
                    float pmd = (float) _placeMaxDistance.GetValue(null);
                    LayerMask lm = (LayerMask) _placeLayerMask.GetValue(null);
                    bool allowed = false;

                    // If our Outdoor Ladder is being placed on a foundation.
                    if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, pmd, lm.value,
                            QueryTriggerInteraction.Ignore))
                        if (hit.collider?.gameObject != null &&
                            (hit.collider.gameObject.name.StartsWith(DrillTargetObjectName)))
                        {
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