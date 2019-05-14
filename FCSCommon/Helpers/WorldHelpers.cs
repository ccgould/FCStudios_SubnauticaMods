using UnityEngine;

namespace FCSCommon.Helpers
{
    public static class WorldHelpers
    {
        public static float GetDepth(GameObject gameObject)
        {
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
        }
    }
}
