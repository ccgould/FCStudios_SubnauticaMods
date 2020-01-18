using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FCSTechFabricator.Objects
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

        public bool Compare(ColorVec4 color)
        {
            if (!Mathf.Approximately(R, color.R))
            {
                return false;
            }

            if (!Mathf.Approximately(B, color.B))
            {
                return false;
            }

            if (!Mathf.Approximately(G, color.G))
            {
                return false;
            }

            if (!Mathf.Approximately(A, color.A))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"R:{R}||G:{G}||B:{B}||A:{A}";
        }

        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
    }
}
