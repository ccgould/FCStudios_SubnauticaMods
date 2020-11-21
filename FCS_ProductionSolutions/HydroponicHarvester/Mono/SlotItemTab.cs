using System;
using FCSCommon.Components;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class SlotItemTab : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private TechType _iconTechType;
        private InterfaceButton _button;
        private bool Initialized => _icon != null;

        private void Initialize(Action<string,object> onButtonClicked)
        {
            if (Initialized) return;
            _icon = gameObject.FindChild("Icon").AddComponent<uGUI_Icon>();
           _button =  gameObject.AddComponent<InterfaceButton>();
           _button.HOVER_COLOR = Color.white;
           _button.STARTING_COLOR = new Color(.5f,.5f,.5f);
           _button.Tag = 
           _button.OnButtonClick += onButtonClicked;
        }

        internal void SetIcon(TechType techType, Action<string, object> onButtonClicked)
        {
            Initialize(onButtonClicked);
            _iconTechType = techType;
            _icon.sprite = SpriteManager.Get(techType);
            _button.Tag = techType;
        }

        internal void Clear()
        {
            _icon.sprite = SpriteManager.Get(TechType.None);
            _iconTechType = TechType.None;
            SetVisibility(false);
        }

        internal void SetVisibility(bool isVisible)
        {
            if(_iconTechType == TechType.None && isVisible) return;
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}