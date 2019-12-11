using System;
using System.Collections.Generic;
using System.Text;
using FCSCommon.Utilities;

namespace FCSCommon.Components
{
    public class Category
    {
        private List<InterfaceButton> _buttons = new List<InterfaceButton>(); 
        
        public void AddButton(InterfaceButton button)
        {
            _buttons.Add(button);
            button.OnButtonClick += OnButtonClick;
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            QuickLogger.Debug($"OnButtonClick {arg1}",true);

            if (arg2 == null) return;

            var selectedButton = (InterfaceButton) arg2;
            
            foreach (InterfaceButton button in _buttons)
            {
                if (button == selectedButton)
                {
                    button.Select();
                }

                button.DeSelect();
            }
        }

        public void RemoveButton(InterfaceButton button)
        {
            _buttons.Remove(button);
        }

        public void Initialize()
        {
            var button = _buttons[0];
            
            if (button == null)
            {
                QuickLogger.Error<Category>("No button to select");
                return;
            }

            OnButtonClick(button.BtnName, button);
        }
    }
}
