using UnityEngine.EventSystems;

namespace FCS_AlterraHub.Mono
{
    public class FCSToggleButton : InterfaceButton
    {
        public bool IgnoreDoubleClicking = false;

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
            if(IsRadial && IsSelected || IgnoreDoubleClicking) return;

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