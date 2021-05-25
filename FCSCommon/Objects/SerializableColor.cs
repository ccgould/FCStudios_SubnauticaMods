using System;
using UnityEngine;

namespace FCSCommon.Objects
{


    [Serializable]
    internal class SerializableColor
    {
        public float r = 1;
        public float g = 1;
        public float b = 1;
        public float a = 1;

        internal SerializableColor(Color c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
            a = c.a;
        }

        public static implicit operator SerializableColor(Color c)
        {
            return new SerializableColor(c);
        }

        internal Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        internal static SerializableColor Create(Color c)
        {
            return new SerializableColor(c);
        }
    }
}
