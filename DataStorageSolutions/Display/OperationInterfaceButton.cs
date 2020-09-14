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
        private bool _initialized;

        private  void Initialize()
        {
            if(_initialized) return;
            _checkMark = GameObjectHelpers.FindGameObject(gameObject, "CheckMark");
            if (_checkMark == null)
            {
                QuickLogger.Debug("Cannot find the CheckMark");
            }

            _initialized = true;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            Initialize();
            _checkMark?.SetActive(!_checkMark.activeSelf);
            base.OnPointerClick(eventData);
        }

        internal void SetCheck(bool value)
        {
            Initialize();
            _checkMark?.SetActive(value);
        }

        public bool IsChecked()
        {
            Initialize();
            return _checkMark.activeSelf;
        }
    }
}
