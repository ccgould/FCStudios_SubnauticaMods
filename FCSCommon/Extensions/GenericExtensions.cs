using FCSCommon.Objects;
using UnityEngine;

namespace FCSCommon.Extensions
{
    public static class GenericExtensions
    {
        public static Quaternion TargetRotationToQuaternion(this TargetRotation targetRotation)
        {
            return new Quaternion(targetRotation.X, targetRotation.Y, targetRotation.Z, targetRotation.W);
        }

        public static string KiloFormat(this int num)
        {
            if (num >= 100000000)
                return (num / 1000000).ToString("#,0M");

            if (num >= 10000000)
                return (num / 1000000).ToString("0.#") + "M";

            if (num >= 100000)
                return (num / 1000).ToString("#,0K");

            if (num >= 10000)
                return (num / 1000).ToString("0.#") + "K";

            return num.ToString("#,0");
        }
    }
}
