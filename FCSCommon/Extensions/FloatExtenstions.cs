using FCSCommon.Objects;
using UnityEngine;

namespace FCSCommon.Extensions
{
    public static class FloatExtenstions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> to a <see cref="Vec3"/>
        /// </summary>
        /// <param name="vector">The vector to convert</param>
        /// <returns></returns>
        public static Vec3 ColorToVector4(this Vector3 vector)
        {
            return new Vec3(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Converts a <see cref="Vec3"/> to a <see cref="Vector3"/>
        /// </summary>
        /// <param name="vec3"></param>
        /// <returns></returns>
        public static Color Vec3ToVector3(this Vec3 vec3)
        {
            return new Color(vec3.X, vec3.Y, vec3.Z);
        }
    }
}
