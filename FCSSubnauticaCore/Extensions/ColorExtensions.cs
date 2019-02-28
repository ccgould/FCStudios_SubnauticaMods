using FCSSubnauticaCore.Objects;
using UnityEngine;
namespace FCSSubnauticaCore.Extensions
{
    public static class ColorExtensions
    {
        public static ColorVec4 ColorToVector4(this Color color)
        {
            return new ColorVec4(color.r, color.g, color.b, color.a);
        }

        public static Color Vector4ToColor(this ColorVec4 vec4)
        {
            return new Color(vec4.R, vec4.G, vec4.B, vec4.A);
        }
    }
}
