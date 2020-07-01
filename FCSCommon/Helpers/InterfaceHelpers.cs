using System;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator;
using UnityEngine;
using UnityEngine.UI;

namespace FCSCommon.Helpers
{
    internal class InterfaceHelpers
    {
        internal static readonly Color ColorBlue = new Color(0.07f, 0.38f, 0.7f, 1f);
        
        internal static bool CreateButton(GameObject parent, string childName, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, out InterfaceButton button)
        {
            var result = CreateButton(parent, childName, btnName, btnMode, Color.white, ColorBlue, onButtonClick, out InterfaceButton cusButton);
            button = cusButton;
            return result;
        }

        [Obsolete("This method shall not be used anymore it will be deleted in up-comming releases of the tech fabricator please use another overload")]
        internal static bool CreateButton(GameObject parent, string childName, string btnName, InterfaceButtonMode btnMode, Color startColor, Color hoverColor, Action<string, object> onButtonClick, out InterfaceButton customButton)
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

        internal static InterfaceButton CreateButton(GameObject go, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, Color startColor, Color hoverColor, float maxInteractionRange)
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


        internal static InterfaceButton CreateButton(GameObject go, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, Color startColor, Color hoverColor, float maxInteractionRange, Sprite icon, string lineOne, string lineTwo = "")
        {
            var button = go.AddComponent<InterfaceButton>();
            button.BtnName = btnName;
            button.ButtonMode = btnMode;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
            button.OnButtonClick = onButtonClick;
            button.MaxInteractionRange = maxInteractionRange;
            button.TextLineOne = lineOne;
            button.TextLineTwo = lineTwo; 
            var image = go.EnsureComponent<Image>();
            image.sprite = icon;
            return button;
        }

        internal static InterfaceButton CreateButton(GameObject go, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, Color startColor, Color hoverColor, float maxInteractionRange, string lineOne, string lineTwo = "")
        {
            if(go == null)
            {
                QuickLogger.Error<InterfaceHelpers>($"GameObject for button {btnName} is null");
            }

            var button = go.AddComponent<InterfaceButton>();
            button.BtnName = btnName;
            button.ButtonMode = btnMode;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
            button.OnButtonClick = onButtonClick;
            button.MaxInteractionRange = maxInteractionRange;
            button.TextLineOne = lineOne;
            button.TextLineTwo = lineTwo;
            return button;
        }
        
        internal static GameObject FindGameObject(GameObject gameObject, string name)
        {
            var result =  FindObjectRecursion(gameObject, name);

            if (result == null)
            {
                QuickLogger.Error<InterfaceHelpers>($"Cant find game object {name}");
                throw new MissingComponentException($"A component cant be found.\nMissing Component: {name}");
            }

            return result;
        }
        
        private static GameObject FindObjectRecursion(GameObject gameObject, string name)
        {
            if(gameObject == null)
            {
                QuickLogger.Error<InterfaceHelpers>("GameObject was null in Find Object Recursion");
                return null;
            }


            foreach (Transform obj in gameObject.transform)
            {
                if (obj.name.Trim().StartsWith(name, StringComparison.OrdinalIgnoreCase))
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
        
        internal static bool CreatePaginator(GameObject parent, string childName, int amountToChangeBy, Action<int> ChangePageBy, out PaginatorButton customButton)
        {
            var result = CreatePaginator(parent, childName, amountToChangeBy, ChangePageBy, Color.white, ColorBlue,
                out var paginatorButton);
            customButton = paginatorButton;
            return result;
        }

        internal static bool CreatePaginator(GameObject parent, string childName, int amountToChangeBy, Action<int> ChangePageBy, Color startColor, Color hoverColor, out PaginatorButton customButton)
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

        internal static void CreatePaginator(GameObject parent, int amountToChangeBy, Action<int> ChangePageBy, Color startColor, Color hoverColor)
        {
            var button = parent.AddComponent<PaginatorButton>();
            button.ChangePageBy = ChangePageBy;
            button.AmountToChangePageBy = amountToChangeBy;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
        }

        internal static bool FindGameObject(GameObject parent, string childName, out GameObject gameObject)
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

        internal static Sprite GetBundleIcon(string name)
        {
            return FcAssetBundlesService.PublicAPI.GetIconByName(name);
        }
    }
}
