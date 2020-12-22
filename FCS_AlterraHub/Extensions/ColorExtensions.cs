using FCS_AlterraHub.Objects;
using UnityEngine;

namespace FCS_AlterraHub.Extensions
{
    /// <summary>
    /// Extensions that handle color conversion
    /// </summary>
    public static class VecExtensions
    {
        /// <summary>
        /// Converts a <see cref="Color"/> to a <see cref="Vec4"/>
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <returns></returns>
        public static Vec4 ColorToVector4(this Color color)
        {
            return new Vec4(color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// Converts a <see cref="Vec4"/> to a <see cref="Color"/>
        /// </summary>
        /// <param name="vec4"></param>
        /// <returns></returns>
        public static Color Vector4ToColor(this Vec4 vec4)
        {
            return new Color(vec4.R, vec4.G, vec4.B, vec4.A);
        }

        /// <summary>
        /// Converts a <see cref="Vec3"/> to a <see cref="Vector3"/>
        /// </summary>
        /// <param name="vec3"></param>
        /// <returns></returns>
        public static Color Vec3ToColor(this Vec3 vec3)
        {
            return new Color(vec3.X, vec3.Y, vec3.Z);
        }

        /// <summary>
        /// Converts a <see cref="Quaternion"/> to a <see cref="Vec4"/>
        /// </summary>
        /// <returns></returns>
        public static Vec4 QuaternionToVec4(this Quaternion quaternion)
        {
            return new Vec4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        /// <summary>
        /// Converts a <see cref="Vec4"/> to a <see cref="Quaternion"/>
        /// </summary>
        /// <returns></returns>
        public static Quaternion Vec4ToQuaternion(this Vec4 vec4)
        {
            return new Quaternion(vec4.R, vec4.G, vec4.B, vec4.A);
        }

    }
}
