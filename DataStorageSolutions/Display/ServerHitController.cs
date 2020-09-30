using System;
using DataStorageSolutions.Mono;
using FCSCommon.Components;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DataStorageSolutions.Display
{
    internal class ServerHitController: InterfaceButton
    {
        public DSSServerController Controller { get; set; }
        public Func<bool> IsClickable { get; set; }

        public override void OnPointerClick(PointerEventData eventData)
        {

            if (!EventSystem.current.IsPointerOverGameObject()) return;

            if (this.IsHovered && IsClickable.Invoke())
            {
                QuickLogger.Debug($"Clicked Button: {this.BtnName}", true);
                OnButtonClick?.Invoke(this.BtnName, this.Tag);
            }
        }
    }
}
