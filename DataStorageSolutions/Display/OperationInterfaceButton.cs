using FCSCommon.Components;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DataStorageSolutions.Display
{
    internal class OperationInterfaceButton : InterfaceButton
    {
        private GameObject _checkMark;

        public override void OnEnable()
        {
            base.OnEnable();
            _checkMark = GameObjectHelpers.FindGameObject(gameObject, "CheckMark");
            if (_checkMark == null)
            {
                QuickLogger.Debug("Cannot find the CheckMark");
                return;
            }

            _checkMark.SetActive(false);

        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            _checkMark?.SetActive(!_checkMark.activeSelf);
            base.OnPointerClick(eventData);
        }

        internal void SetCheck(bool value)
        {
            _checkMark?.SetActive(value);
        }

        public bool IsChecked()
        {
            return _checkMark.activeSelf;
        }
    }
}
