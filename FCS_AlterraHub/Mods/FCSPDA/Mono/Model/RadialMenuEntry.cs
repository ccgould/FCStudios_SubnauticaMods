﻿using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSPDA.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Model
{
    public class RadialMenuEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Text _label;
        private string _buttonName;
        private GameObject Hover;
        private uGUI_Icon Icon;
        private PDAPages _page;
        private FCSPDAController _controller;


        public void Initialize(FCSPDAController controller, Text pLabel, Sprite pIcon, Text pageLabel, string buttonName,PDAPages page)
        {
            Hover = GameObjectHelpers.FindGameObject(gameObject, "Backer");
            Icon = gameObject.FindChild("Icon").AddComponent<uGUI_Icon>();
            Icon.sprite = pIcon;
            _controller = controller;

            _label = pageLabel;
            _label = pLabel;
            _buttonName = buttonName;
            _page = page;
        }

        private void SetLabel(string pText)
        {
            _label.text = pText;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetLabel(_buttonName);
            Hover.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetLabel(string.Empty); 
            Hover.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _controller.GoToPage(_page);
            Hover.SetActive(false);
            _label.text = string.Empty;
        }
    }
}
