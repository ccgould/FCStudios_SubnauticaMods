using System;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class InterfaceHelpers
    {
        public static InterfaceButton CreateButton(GameObject go, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, Color startColor, Color hoverColor, float maxInteractionRange)
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

        public static InterfaceButton CreateButton(GameObject go, string btnName, InterfaceButtonMode btnMode, Action<string, object> onButtonClick, Color startColor, Color hoverColor, float maxInteractionRange, string lineOne, string lineTwo = "")
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
        
        public static GameObject FindGameObject(GameObject gameObject, string name, bool throwException = true)
        {
            var result = gameObject.transform.FirstOrDefault(x => x.name == name);
            
            if (result == null)
            {
                QuickLogger.Debug($"[InterfaceHelpers] Cant find game object {name} this maybe intentional. throwException {throwException}");
                if (throwException)
                {
                    throw new MissingComponentException($"A component cant be found.\nMissing Component: {name}");
                }
            }

            return result?.gameObject;
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
                QuickLogger.Debug($"Current Object in search: ({obj.name.Trim()}) || Name Looking for: ({name}) || {obj.name.Trim().StartsWith(name.Trim(), StringComparison.OrdinalIgnoreCase)}");
                if (obj.name.Trim().StartsWith(name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    QuickLogger.Debug($"Found: {name}");
                    return obj.gameObject;
                }

                if (obj.transform.childCount > 0)
                {
                    var result = FindObjectRecursion(obj.gameObject, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            QuickLogger.Debug("Returning null");
            return null;
        }

        public static void CreatePaginator(GameObject parent, int amountToChangeBy, Action<int> ChangePageBy, Color startColor, Color hoverColor)
        {
            var button = parent.AddComponent<PaginatorButton>();
            button.ChangePageBy = ChangePageBy;
            button.AmountToChangePageBy = amountToChangeBy;
            button.STARTING_COLOR = startColor;
            button.HOVER_COLOR = hoverColor;
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
