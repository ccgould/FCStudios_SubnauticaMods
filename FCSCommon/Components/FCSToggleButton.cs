using FCSCommon.Enums;
using UnityEngine.EventSystems;

namespace FCSCommon.Components
{
    internal class FCSToggleButton : InterfaceButton
    {
        public override void OnEnable()
        {
            base.OnEnable();

            if (IsSelected)
            {
                Select();
            }
            else
            {
                DeSelect();
            }
        }

        private void ChangeState(bool value)
        {
            if(IsRadial && IsSelected) return;
            if (value)
            {
                DeSelect();
            }
            else
            {
                Select();
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            ChangeState(IsSelected);
            base.OnPointerClick(eventData);
        }

        public void SetVisible(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}