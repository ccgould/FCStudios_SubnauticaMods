using UnityEngine;

namespace FCS_AlterraHub.Models.Structs;

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

    public override bool Equals(object obj)
    {
        return obj is Vec3 vec &&
               X == vec.X &&
               Y == vec.Y &&
               Z == vec.Z;
    }

    public override int GetHashCode()
    {
        int hashCode = -307843816;
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        hashCode = hashCode * -1521134295 + Z.GetHashCode();
        return hashCode;
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
