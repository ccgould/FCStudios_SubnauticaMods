using System;
using UnityEngine;

namespace FCSCommon.Objects
{


    [Serializable]
    public class SerializableColor
    {
        public float r = 1;
        public float g = 1;
        public float b = 1;
        public float a = 1;

        public SerializableColor(Color c)
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

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public static SerializableColor Create(Color c)
        {
            return new SerializableColor(c);
        }
    }
}
