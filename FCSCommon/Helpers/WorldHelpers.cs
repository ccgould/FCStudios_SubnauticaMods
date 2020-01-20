using UnityEngine;

namespace FCSCommon.Helpers
{
    public static class WorldHelpers
    {
#if SUBNAUTICA
        public static float GetDepth(GameObject gameObject)
        {
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
        }
#elif BELOWZERO
        public static float GetDepth(GameObject gameObject)
        {
            return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
        }
#endif
    }
}
