using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSAlterraShipping.Mono
{
    internal class AlterraShippingNameController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hover;

        public RectTransform rectTransform;

        [SerializeField]
        private AlterraShippingTarget target;
        [SerializeField]
        private Text ShippingContainerName;

        public Action OnLabelChanged;


        private void Awake()
        {

        }

        private void Initialize(AlterraShippingTarget shippingContainer, GameObject textPrefab)
        {
            target = shippingContainer;
            ShippingContainerName = textPrefab.GetComponent<Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            uGUI.main.userInput.RequestString("Shipping Container Name", "Submit", target.Name, 25, new uGUI_UserInput.UserInputCallback(SetLabel));
        }

        public void SetLabel(string newLabel)
        {
            target.Name = newLabel;
            ShippingContainerName.text = newLabel;
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
            if (hover)
            {
                HandReticle.main.SetIcon(HandReticle.IconType.Rename);
                HandReticle.main.SetInteractTextRaw("Set Shipping Container Name", "");
            }
        }

        public static AlterraShippingNameController Create(AlterraShippingTarget shippingContainer, GameObject textPrefab)
        {
            //var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
            //var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
            //textPrefab.fontSize = 12;
            //textPrefab.color = new Color32(188, 254, 254, 255);

            var alterraShippingNameController = textPrefab.GetComponent<AlterraShippingNameController>();

            //var alterraShippingNameController = new GameObject("AlterraShippingNameController", typeof(RectTransform)).AddComponent<AlterraShippingNameController>();
            //var rt = alterraShippingNameController.gameObject.transform as RectTransform;
            //RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
            alterraShippingNameController.Initialize(shippingContainer, textPrefab);

            return alterraShippingNameController;
        }
    }
}
