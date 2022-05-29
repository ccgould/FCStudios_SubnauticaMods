using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSToggleGroup : MonoBehaviour
    {
        private List<FCSToggleButton> _buttons;
        public Action<string> OnToggleButtonAction;
        public Action<string,object> OnToggleButtonActionObj;
        private InterfaceButtonMode _toggleButtonMode = InterfaceButtonMode.HoverImage;
        private Color _color = Color.gray;
        private Color _hoverColor = Color.white;
        private object Tag;

        public void Initialize()
        {
            if (_buttons == null)
            {
                _buttons = new List<FCSToggleButton>();
            }

            RefreshList();
        }

        public void RefreshList()
        {
            int i = 0;
            foreach (Transform child in transform)
            {
                var toggleBtn = child.gameObject.EnsureComponent<FCSToggleButton>();
                toggleBtn.ButtonMode = _toggleButtonMode;
                toggleBtn.STARTING_COLOR = _color;
                toggleBtn.HOVER_COLOR = _hoverColor;
                toggleBtn.BtnName = $"ToggleButton_{i++}";
                _buttons.Add(toggleBtn);
                toggleBtn.OnButtonClick += Refresh;
            }
        }

        private void Refresh(string name, object buttonTag)
        {
            OnToggleButtonActionObj?.Invoke(name,buttonTag);
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

        public void SetMode(InterfaceButtonMode mode)
        {
            _toggleButtonMode = mode;
        }

        public void SetColor(Color color,Color hoverColor)
        {
            _color = color;
            _hoverColor = hoverColor;
        }
    }
}
