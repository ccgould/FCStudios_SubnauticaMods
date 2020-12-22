using System;
using System.Collections.Generic;
using FCSCommon.Components;
using FCSCommon.Enums;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSToggleGroup : MonoBehaviour
    {
        private List<FCSToggleButton> _buttons;
        public Action<string> OnToggleButtonAction;

        private void Awake()
        {
            int i = 0;
            if (_buttons == null)
            {
                _buttons = new List<FCSToggleButton>();
            }

            foreach (Transform child in transform)
            {
                var toggleBtn = child.gameObject.EnsureComponent<FCSToggleButton>();
                toggleBtn.ButtonMode = InterfaceButtonMode.HoverImage;
                toggleBtn.BtnName = $"ToggleButton_{i++}";
                _buttons.Add(toggleBtn);
                toggleBtn.OnButtonClick += Refresh;
            }
        }

        private void Refresh(string name, object buttonTag)
        {
            OnToggleButtonAction?.Invoke(name);
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i].BtnName == name) continue;
                _buttons[i].DeSelect();
            }
        }

        public void Select(string buttonName)
        {
            foreach (FCSToggleButton button in _buttons)
            {
                if (button.BtnName.Equals(buttonName))
                {
                    button.Select();
                    break;
                }
            }
        }
    }
}
