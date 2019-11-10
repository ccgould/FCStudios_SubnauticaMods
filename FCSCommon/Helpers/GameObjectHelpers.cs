using UnityEngine;

namespace FCSCommon.Helpers
{
    public class GameObjectHelpers : MonoBehaviour
    {
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
