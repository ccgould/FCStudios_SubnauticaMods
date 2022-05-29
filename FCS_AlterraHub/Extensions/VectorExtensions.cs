using FCS_AlterraHub.Objects;
using UnityEngine;

namespace FCS_AlterraHub.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Vector3"/> to a <see cref="Vec3"/>
        /// </summary>
        /// <param name="vector">The vector to convert</param>
        /// <returns></returns>
        public static Vec3 ToVec3(this Vector3 vector)
        {
            return new Vec3(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Converts a <see cref="Vec3"/> to a <see cref="Vector3"/>
        /// </summary>
        /// <param name="vec3"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this Vec3 vec3)
        {
            return new Vector3(vec3.X, vec3.Y, vec3.Z);
        }
    }
}
