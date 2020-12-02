using UnityEngine.EventSystems;

namespace FCSCommon.Components
{
    internal class FCSToggleButton : InterfaceButton
    {
        public override void OnEnable()
        {
            base.OnEnable();
            ChangeState(!IsSelected);
        }

        private void ChangeState(bool value)
        {
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