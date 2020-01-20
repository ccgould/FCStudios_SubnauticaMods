using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSAlterraShipping.Mono
{
    internal class AlterraShippingNameController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hover;
        [SerializeField]
        private AlterraShippingTarget target;
        [SerializeField]
        private Text _shippingContainerName;
        public Action OnLabelChanged;

        private void Initialize(AlterraShippingTarget shippingContainer, GameObject textPrefab)
        {
            target = shippingContainer;
            _shippingContainerName = textPrefab.GetComponent<Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            uGUI.main.userInput.RequestString("Shipping Container Name", "Submit", target.Name, 25, new uGUI_UserInput.UserInputCallback(SetLabel));
        }

        internal void SetLabel(string newLabel)
        {
            target.Name = newLabel;
            _shippingContainerName.text = newLabel;
            OnLabelChanged?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }

        private void Update()
        {
            if (!hover) return;
            HandReticle.main.SetIcon(HandReticle.IconType.Rename);
#if SUBNAUTICA
            HandReticle.main.SetInteractTextRaw("Set Shipping Container Name", "");
#elif BELOWZERO
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Set Shipping Container Name");
#endif
        }

        public static AlterraShippingNameController Create(AlterraShippingTarget shippingContainer, GameObject textPrefab)
        {
            var alterraShippingNameController = textPrefab.GetComponent<AlterraShippingNameController>();
            alterraShippingNameController.Initialize(shippingContainer, textPrefab);
            return alterraShippingNameController;
        }
    }
}
