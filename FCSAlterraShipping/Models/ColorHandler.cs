using FCSCommon.Helpers;
using UnityEngine;

namespace FCSAlterraShipping.Models
{
    internal class ColorHandler
    {
        internal static void ChangeBodyColor(string matNameColor, Color color, GameObject gameObject)
        {
            MaterialHelpers.ChangeMaterialColor(matNameColor, gameObject, color);
        }
    }
}
