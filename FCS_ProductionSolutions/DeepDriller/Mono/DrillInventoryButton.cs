using FCSCommon.Components;
using FCSCommon.Helpers;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class DrillInventoryButton : InterfaceButton
    {
        private Text _amount;
        private uGUI_Icon _icon;
        private bool _isInitialized;

        private void Start()
        {
            if (!_isInitialized)
            {
                _icon = InterfaceHelpers.FindGameObject(gameObject, "Icon").EnsureComponent<uGUI_Icon>();
                _amount = gameObject.GetComponentInChildren<Text>();
                InvokeRepeating(nameof(TryRemove), 1, 1);
                _isInitialized = true;
            }
            
            UpdateAmount();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            UpdateAmount();
            
        }

        internal void UpdateAmount()
        {
            var amount = DrillStorage.GetItemCount((TechType) Tag);
            if (amount <= 0)
            { 
                gameObject.SetActive(false);
                return;
            }

            if (_amount == null)
            {
                _amount = gameObject.GetComponentInChildren<Text>();
            }

            _amount.text = amount.ToString();
        }


        private void TryRemove()
        {
            var amount = DrillStorage.GetItemCount((TechType)Tag);
            if (amount <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        public FCSDeepDrillerContainer DrillStorage { get; set; }

        public void RefreshIcon()
        {
            if (_icon == null)
            {
                _icon = InterfaceHelpers.FindGameObject(gameObject, "Icon").EnsureComponent<uGUI_Icon>();

            }
            _icon.sprite = SpriteManager.Get((TechType)Tag);
        }

        public bool IsValidAndActive(TechType techType)
        {
            return techType == (TechType) Tag && gameObject.activeSelf;
        }
    }
}