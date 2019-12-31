using System;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSCommon.Helpers
{
    public class GameObjectHelpers : MonoBehaviour
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
                QuickLogger.Error<GameObjectHelpers>($"{e.Message}");
            }
        }
        public static void DestroyComponent(GameObject obj)
        {
            var list = obj.GetComponents(typeof(Component));

            for (int i = 0; i < list.Length; i++)
            {
                if (!list[i].name.StartsWith("Transform"))
                    Destroy(list[i]);
            }
        }

        public static int GetObjectCount<T>() where T : MonoBehaviour
        {
            var length = GameObject.FindObjectsOfType<T>()?.Length;
            if (length != null)
                return (int) length;
            return 0;
        }
    }
}
