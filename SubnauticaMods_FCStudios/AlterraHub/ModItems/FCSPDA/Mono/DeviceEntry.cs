using FCS_AlterraHub.Models.Abstract;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static HandReticle;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono
{
    internal class DeviceEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private TechType _techType;
        private FCSDevice _device;

        public void OnPointerClick(PointerEventData eventData)
        {
            FCSPDAController.Main.OpenDeviceUI(_techType, _device);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }

        internal void Initialize(FCSDevice device)
        {
            _techType = device.GetTechType();
            _device = device;
            var icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            icon.sprite = SpriteManager.Get(device.GetTechType());

            var text = gameObject.GetComponentInChildren<TMP_Text>();
            text.text = device.GetDeviceName();
        }
    }
}