using System;
using FCS_AlterraHub.Patches;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Helpers
{
    public class FCSHUD : MonoBehaviour
    {
        private void Awake()
        {
            var modeBTN = GameObjectHelpers.FindGameObject(gameObject, "MODEBTN");
            modeBTN.AddComponent<uiButton>();
            var onOffBTN = GameObjectHelpers.FindGameObject(gameObject, "ONOFFBTN");
            onOffBTN.AddComponent<uiButton>();

        }

        public void Move()
        {
            var parent = uGUI_PowerIndicator_Initialize_Patch.IndicatorInstance.text.transform.parent;
            gameObject.transform.SetParent(parent, false);
            gameObject.layer = parent.gameObject.layer;
        }

        public bool IsOpen => gameObject.activeSelf;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }

    internal class uiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private Image _image;
        private static readonly Color HoverColor = new Color(0.956f, 0.796f, 0.258f);
        private static readonly Color CrossColor = new Color(1, 0, 0, 0.5f);
        private bool hover;
        private bool down;
        private static readonly Vector3 DownScale = new Vector3(0.9f, 0.9f, 1);
        private static readonly Vector3 HoverScale = new Vector3(1.2f, 1.2f, 1);
        private Mode mode; 
        public enum Mode { Add, Remove, Cross }
        public Action onClick = delegate { };

        private void Awake()
        {
           _image = gameObject.GetComponent<Image>();
        }

        public void OnDisable()
        {
            hover = false;
            down = false;
            Update();
        }

        private void Update()
        {
            _image.transform.localScale = down ? DownScale : hover ? HoverScale : new Vector3(1, 1, 1);

            switch (mode)
            {
                case Mode.Add:
                    _image.color = hover ? HoverColor : Color.white;
                    break;
                case Mode.Remove:
                    _image.color = HoverColor;
                    break;
                case Mode.Cross:
                    _image.color = CrossColor;
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (mode != Mode.Cross)
            {
                onClick.Invoke();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (mode == Mode.Cross)
            {
                onClick.Invoke();
            }
            down = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            down = false;
        }
	}
}
