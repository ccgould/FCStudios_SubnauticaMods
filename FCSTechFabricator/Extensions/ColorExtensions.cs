using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Objects;
using UnityEngine;

namespace FCSTechFabricator.Extensions
{
    /// <summary>
    /// Extensions that handle color conversion
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Color"/> to a <see cref="ColorVec4"/>
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <returns></returns>
        public static ColorVec4 ColorToVector4(this Color color)
        {
            return new ColorVec4(color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// Converts a <see cref="ColorVec4"/> to a <see cref="Color"/>
        /// </summary>
        /// <param name="vec4"></param>
        /// <returns></returns>
        public static Color Vector4ToColor(this ColorVec4 vec4)
        {
            return new Color(vec4.R, vec4.G, vec4.B, vec4.A);
        }

    }
}
