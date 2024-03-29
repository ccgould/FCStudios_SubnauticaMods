﻿using System;
using UnityEngine;

namespace FCS_AlterraHub.Objects
{
    [Serializable]
    public struct Vec3
    {
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        public bool Compare(Vec3 fcs)
        {
            if (!Mathf.Approximately(X, fcs.X))
            {
                return false;
            }

            if (!Mathf.Approximately(Y, fcs.Y))
            {
                return false;
            }

            if (!Mathf.Approximately(Z, fcs.Z))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"X:{X}||Y:{Y}||Z:{Z}";
        }
        
        public static bool operator ==(Vec3 first, Vec3 second)
        {
            return Equals(first, second);
        }
        public static bool operator !=(Vec3 first, Vec3 second)
        {
            // or !Equals(first, second), but we want to reuse the existing comparison 
            return !(first == second);
        }

        public static bool operator ==(Vec3 first, Vector3 second)
        {
            return Equals(first.X, second.x) && Equals(first.Y, second.z) && Equals(first.Z, second.z);
        }
        public static bool operator !=(Vec3 first, Vector3 second)
        {
            // or !Equals(first, second), but we want to reuse the existing comparison 
            return !(first == second);
        }


        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
