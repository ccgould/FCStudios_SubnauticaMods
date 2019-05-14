using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
