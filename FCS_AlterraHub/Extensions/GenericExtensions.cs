using System.Collections.Generic;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using UnityEngine;

namespace FCS_AlterraHub.Extensions
{
    internal static class GenericExtensions
    {
        internal static Quaternion TargetRotationToQuaternion(this TargetRotation targetRotation)
        {
            return new Quaternion(targetRotation.X, targetRotation.Y, targetRotation.Z, targetRotation.W);
        }

        internal static string KiloFormat(this int num)
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

        //internal static List<CartItemSaveData> ToCartItemSaveData(this List<CartItem> data)
        //{
        //    foreach (CartItem cartItem in data)
        //    {
        //        yield return cartItem.Save();
        //    }
        //}
    }
}
