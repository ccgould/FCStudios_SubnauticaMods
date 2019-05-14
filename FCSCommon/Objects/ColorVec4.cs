using System;

namespace FCSCommon.Objects
{
    [Serializable]
    public class ColorVec4
    {
        public ColorVec4(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
    }
}
