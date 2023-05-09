using System.Collections.Generic;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Structs;
using UnityEngine;

namespace FCS_AlterraHub.Core.Extensions
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
        /// Converts a <see cref="Vec3"/> to a <see cref="Color"/>
        /// </summary>
        /// <param name="vec3"></param>
        /// <returns></returns>
        public static Color Vec3ToColor(this Vec3 vec3)
        {
            return new Color(vec3.X, vec3.Y, vec3.Z);
        }

        /// <summary>
        /// Converts a <see cref="Vec3"/> to a <see cref="Vector3"/>
        /// </summary>
        /// <param name="vec3"></param>
        /// <returns></returns>
        public static Vector3 Vec3ToVector3(this Vec3 vec3)
        {
            return new Vector3(vec3.X, vec3.Y, vec3.Z);
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

        /// <summary>
        /// Converts a <see cref="ColorTemplateSave"/> to a <see cref="ColorTemplate"/>
        /// </summary>
        /// <returns></returns>
        public static ColorTemplate ToColorTemplate(this ColorTemplateSave template)
        {
            return new ColorTemplate
            {
                PrimaryColor = template.PrimaryColor.Vector4ToColor(),
                SecondaryColor = template.SecondaryColor.Vector4ToColor(),
                EmissionColor = template.EmissionColor.Vector4ToColor(),
            };
        }


        /// <summary>
        /// Converts a <see cref="ColorTemplate"/> to a <see cref="ColorTemplateSave"/>
        /// </summary>
        /// <returns></returns>
        public static ColorTemplateSave ToColorTemplate(this ColorTemplate template)
        {
            return new ColorTemplateSave
            {
                PrimaryColor = template?.PrimaryColor.ColorToVector4() ?? Color.white.ColorToVector4(),
                SecondaryColor = template?.SecondaryColor.ColorToVector4() ?? Color.white.ColorToVector4(),
                EmissionColor = template?.EmissionColor.ColorToVector4() ?? Color.cyan.ColorToVector4(),
            };
        }

        /// <summary>
        /// Converts a <see cref="List&lt;ColorTemplateSave&gt;"/> to a <see cref="List&lt;ColorTemplate&gt;"/>
        /// </summary>
        /// <returns></returns>
        public static List<ColorTemplate> ToListOfColorTemplates(this List<ColorTemplateSave> templates)
        {
            var list = new List<ColorTemplate>();

            foreach (ColorTemplateSave save in templates)
            {
                list.Add(new ColorTemplate
                {
                    PrimaryColor = save.PrimaryColor.Vector4ToColor(),
                    SecondaryColor = save.SecondaryColor.Vector4ToColor(),
                    EmissionColor = save.EmissionColor.Vector4ToColor(),
                });
            }

            return list;
        }

        /// <summary>
        /// Converts a <see cref="List&lt;ColorTemplate&gt;"/> to a <see cref="List&lt;ColorTemplateSave&gt;"/>
        /// </summary>
        /// <returns></returns>
        public static List<ColorTemplateSave> ToListOfColorTemplatesSaves(this List<ColorTemplate> templates)
        {
            var list = new List<ColorTemplateSave>();

            foreach (ColorTemplate save in templates)
            {
                list.Add(new ColorTemplateSave
                {
                    PrimaryColor = save.PrimaryColor.ColorToVector4(),
                    SecondaryColor = save.SecondaryColor.ColorToVector4(),
                    EmissionColor = save.EmissionColor.ColorToVector4(),
                });
            }

            return list;
        }

    }
}
