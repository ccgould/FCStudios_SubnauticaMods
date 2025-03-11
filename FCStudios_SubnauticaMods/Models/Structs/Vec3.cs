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

    public Vec3(Vector3 vector3)
    {
        X = vector3.x;
        Y = vector3.y;
        Z = vector3.z;
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

public struct Vec2
{
    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vec2(Vector2 vector3)
    {
        X = vector3.x;
        Y = vector3.y;  
    }


    public bool Compare(Vec2 fcs)
    {
        if (!Mathf.Approximately(X, fcs.X))
        {
            return false;
        }

        if (!Mathf.Approximately(Y, fcs.Y))
        {
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        return $"X:{X}||Y:{Y}";
    }

    public override bool Equals(object obj)
    {
        return obj is Vec3 vec &&
               X == vec.X &&
               Y == vec.Y;
    }

    public override int GetHashCode()
    {
        int hashCode = -307843816;
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(Vec2 first, Vec2 second)
    {
        return Equals(first, second);
    }
    public static bool operator !=(Vec2 first, Vec2 second)
    {
        // or !Equals(first, second), but we want to reuse the existing comparison 
        return !(first == second);
    }

    public static bool operator ==(Vec2 first, Vector2 second)
    {
        return Equals(first.X, second.x) && Equals(first.Y, second.y);
    }
    public static bool operator !=(Vec2 first, Vector2 second)
    {
        // or !Equals(first, second), but we want to reuse the existing comparison 
        return !(first == second);
    }


    public float X { get; set; }
    public float Y { get; set; }
}
