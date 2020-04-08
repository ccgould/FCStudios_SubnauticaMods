using System;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSCommon.Helpers
{
    internal class GameObjectHelpers : MonoBehaviour
    {
        internal static void AddConstructableBounds(GameObject prefab, Vector3 size, Vector3 center)
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
                QuickLogger.Error<GameObjectHelpers>($"{e.Message}");
            }
        }
        internal static void DestroyComponent(GameObject obj)
        {
            var list = obj.GetComponents(typeof(Component));

            for (int i = 0; i < list.Length; i++)
            {
                if (!list[i].name.StartsWith("Transform"))
                    Destroy(list[i]);
            }
        }

        internal static int GetObjectCount<T>() where T : MonoBehaviour
        {
            var length = GameObject.FindObjectsOfType<T>()?.Length;
            if (length != null)
                return (int) length;
            return 0;
        }

        internal static GameObject FindGameObject(GameObject go,string name, SearchOption searchOption = SearchOption.Full)
        {
            try
            {
                var renders = go.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer mesh in renders)
                {
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
               QuickLogger.Error($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }

            return null;
        }
    }

    internal enum SearchOption
    {
        Full,
        StartsWith,
        EndsWith,
        Contains
    }
}
