using System;
using System.Collections.Generic;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class GameObjectHelpers
    {
        public static void AddConstructableBounds(GameObject prefab, Vector3 size, Vector3 center)
        {
            try
            {
                if (prefab == null) return;
                var bounds = prefab.EnsureComponent<ConstructableBounds>();
                bounds.bounds.size = size;
                bounds.bounds.position = center;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}");
            }
        }
        
        public static int GetObjectCount<T>() where T : MonoBehaviour
        {
            var length = GameObject.FindObjectsOfType<T>()?.Length;
            if (length != null)
                return (int)length;
            return 0;
        }

        public static GameObject FindGameObject(GameObject go, string name,SearchOption searchOption = SearchOption.Full)
        {
            try
            {
                var renders = FindGameObject(go?.transform);
                
                foreach (GameObject mesh in renders)
                {
                    //QuickLogger.Debug($"[FindGameObject: {name} | Current Object: {mesh?.name}]");
                    switch (searchOption)
                    {
                        case SearchOption.Full:
                            if (mesh.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                            {
                                return mesh.gameObject;
                            }

                            break;
                        case SearchOption.StartsWith:
                            if (mesh.name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                            {
                                return mesh.gameObject;
                            }

                            break;
                        case SearchOption.EndsWith:
                            if (mesh.name.EndsWith(name, StringComparison.OrdinalIgnoreCase))
                            {
                                return mesh.gameObject;
                            }

                            break;
                        case SearchOption.Contains:
                            if (mesh.name.Contains(name))
                            {
                                return mesh.gameObject;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(searchOption), searchOption, null);
                    }
                }
            }
            catch (Exception e)
            {

                QuickLogger.Error($"Message: {e.Message} StackTrace: {e.StackTrace} while trying to find: {name}");
                QuickLogger.Error($"Failed to find GameObject with the name: {name}");
            }
            return null;
        }


        public static IEnumerable<GameObject> FindGameObjects(GameObject go, string name,
    SearchOption searchOption = SearchOption.Full)
        {
            var renders = FindGameObject(go?.transform);

                foreach (GameObject mesh in renders)
                {
                    switch (searchOption)
                    {
                        case SearchOption.Full:
                            if (mesh.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return mesh.gameObject;
                            }

                            break;
                        case SearchOption.StartsWith:
                            if (mesh.name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return mesh.gameObject;
                            }

                            break;
                        case SearchOption.EndsWith:
                            if (mesh.name.EndsWith(name, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return mesh.gameObject;
                            }

                            break;
                        case SearchOption.Contains:
                            if (mesh.name.Contains(name))
                            {
                                yield return mesh.gameObject;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(searchOption), searchOption, null);
                    }
                }
        }


        private static IEnumerable<GameObject> FindGameObject(Transform tr)
        {
            foreach (Transform child in tr)
            {
                if (child == null) continue;
                yield return child.gameObject;
                if (child.childCount <= 0) continue;
                foreach (var o in FindGameObject(child))
                {
                    yield return o.gameObject;
                }
            }
        }


        public static Transform FirstOrDefault(this Transform transform, Func<Transform, bool> query)
        {
            if (query(transform))
            {
                return transform;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                var result = FirstOrDefault(transform.GetChild(i), query);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static void SetConstructableBounds(GameObject go, Vector3 size, Vector3 center)
        {
            try
            {
                if (go == null) return;
                var bounds = go.EnsureComponent<ConstructableBounds>();
                bounds.bounds.size = size;
                bounds.bounds.position = center;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}");
            }
        }
    }

    public enum SearchOption
    {
        Full,
        StartsWith,
        EndsWith,
        Contains
    }
}
