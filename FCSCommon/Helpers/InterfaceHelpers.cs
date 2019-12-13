using System;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSCommon.Helpers
{
    public class InterfaceHelpers
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

        public static GameObject FindGameObject(GameObject gameObject, string name)
        {
            var result =  FindObjectRecursion(gameObject, name);

            if (result == null)
            {
                QuickLogger.Error<InterfaceHelpers>($"Cant find game object {name}");
            }

            return result;
        }

        public static InterfaceButton CreateButton(GameObject go, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, Color startColor, Color hoverColor,float maxInteractionRange)
        {
            var button = go.AddComponent<InterfaceButton>();
            button.BtnName = btnName;
            button.ButtonMode = btnMode;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
            button.OnButtonClick = onButtonClick;
            button.MaxInteractionRange = maxInteractionRange;
            return button;
        }

        private static GameObject FindObjectRecursion(GameObject gameObject, string name)
        {
            foreach (Transform obj in gameObject.transform)
            {
                if (obj.name.StartsWith(name))
                {
                    return obj.gameObject;
                }

                if (obj.transform.childCount > 0)
                {
                    FindObjectRecursion(obj.gameObject, name);
                }
            }

            return null;
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

        public static void CreatePaginator(GameObject go, int amountToChangeBy, Action<int> ChangePageBy, Color startColor, Color hoverColor)
        {
            var button = go.AddComponent<PaginatorButton>();
            button.ChangePageBy = ChangePageBy;
            button.AmountToChangePageBy = amountToChangeBy;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
        }
    }
}
