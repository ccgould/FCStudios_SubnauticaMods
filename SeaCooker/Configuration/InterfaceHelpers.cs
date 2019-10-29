using AE.SeaCooker.Display;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace AE.SeaCooker.Configuration
{
    internal static class InterfaceHelpers
    {
        private static readonly Color ColorBlue = new Color(0.07f, 0.38f, 0.7f, 1f);
        public static bool CreateButton(GameObject parent, string childName, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, out InterfaceButton button)
        {
            var result = CreateButton(parent, childName, btnName, btnMode, Color.white, ColorBlue, onButtonClick, out InterfaceButton cusButton);
            button = cusButton;
            return result;
        }

        public static bool CreateButton(GameObject parent, string childName, string btnName, InterfaceButtonMode btnMode, Color startColor, Color hoverColor, Action<string, object> onButtonClick, out InterfaceButton customButton)
        {
            customButton = null;
            var go = parent.FindChild(childName)?.gameObject;
            if (go == null)
            {
                QuickLogger.Error($"{childName} cannot be found");
                return false;
            }

            var button = go.AddComponent<InterfaceButton>();
            button.BtnName = btnName;
            button.ButtonMode = btnMode;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
            button.OnButtonClick = onButtonClick;
            customButton = button;
            return true;
        }

        public static bool CreatePaginator(GameObject parent, string childName, int amountToChangeBy, Action<int> ChangePageBy, out PaginatorButton customButton)
        {
            var result = CreatePaginator(parent, childName, amountToChangeBy, ChangePageBy, Color.white, ColorBlue,
                out var paginatorButton);
            customButton = paginatorButton;
            return result;
        }

        public static bool CreatePaginator(GameObject parent, string childName, int amountToChangeBy, Action<int> ChangePageBy, Color startColor, Color hoverColor, out PaginatorButton customButton)
        {
            customButton = null;
            var go = parent.FindChild(childName)?.gameObject;
            if (go == null)
            {
                QuickLogger.Error($"{childName} cannot be found");
                return false;
            }

            var button = go.AddComponent<PaginatorButton>();
            button.ChangePageBy = ChangePageBy;
            button.AmountToChangePageBy = amountToChangeBy;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
            customButton = button;
            return true;
        }

        public static bool FindGameObject(GameObject parent, string childName, out GameObject gameObject)
        {
            gameObject = null;
            var result = parent.FindChild(childName)?.gameObject;
            if (result == null)
            {
                QuickLogger.Error($"{childName} cannot be found");
                return false;
            }

            gameObject = result;
            return true;
        }
    }
}
