using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using UnityEngine;

namespace FCSCommon.Extensions
{
    /// <summary>
    /// Extensions that handle color conversion
    /// </summary>
    internal static class ColorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Color"/> to a <see cref="ColorVec4"/>
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <returns></returns>
        internal static ColorVec4 ColorToVector4(this Color color)
        {
            return new ColorVec4(color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// Converts a <see cref="ColorVec4"/> to a <see cref="Color"/>
        /// </summary>
        /// <param name="vec4"></param>
        /// <returns></returns>
        internal static Color Vector4ToColor(this ColorVec4 vec4)
        {
            return new Color(vec4.R, vec4.G, vec4.B, vec4.A);
        }

    }
}
