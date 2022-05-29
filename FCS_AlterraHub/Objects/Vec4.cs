using System;
using UnityEngine;

namespace FCS_AlterraHub.Objects
{
    [Serializable]
    public struct Vec4
    {
        public Vec4(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public bool Compare(Vec4 fcs)
        {
            if (!Mathf.Approximately(R, fcs.R))
            {
                return false;
            }

            if (!Mathf.Approximately(B, fcs.B))
            {
                return false;
            }

            if (!Mathf.Approximately(G, fcs.G))
            {
                return false;
            }

            if (!Mathf.Approximately(A, fcs.A))
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

        public static bool operator ==(Vec4 first, Vec4 second)
        {
            return Equals(first, second);
        }
        public static bool operator !=(Vec4 first, Vec4 second)
        {
            // or !Equals(first, second), but we want to reuse the existing comparison 
            return !(first == second);
        }
    }
}
