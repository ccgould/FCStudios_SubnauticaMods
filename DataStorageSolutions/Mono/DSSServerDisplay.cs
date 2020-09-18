using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using DataStorageSolutions.Configuration;
using FCSCommon.Components;
using FCSCommon.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class DSSServerDisplay : AIDisplay
    {
        private DSSServerController _mono;
        private Text _counter;
        private Pickupable _pickupable;
        private Collider _collider;

        internal void Setup(DSSServerController mono)
        {
            _mono = mono;
            _pickupable = _mono.gameObject.GetComponentInChildren<Pickupable>();
            _collider = _pickupable?.gameObject.GetComponentInChildren<Collider>();

            if (FindAllComponents())
            {
                UpdateDisplay();
            }
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {
            throw new NotImplementedException();
        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Canvas  
                var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

                if (canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Counter

                _counter = canvasGameObject.GetComponentInChildren<Text>();
                #endregion

                #region Hit

                var interactionFace = InterfaceHelpers.FindGameObject(canvasGameObject, "Hit");
                var catcher = interactionFace.AddComponent<InterfaceButton>();
                catcher.ButtonMode = InterfaceButtonMode.TextColor;
                catcher.OnInterfaceButton += OnInterfaceButton;
                
                #endregion

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void OnInterfaceButton(bool obj)
        {
            QuickLogger.Info("OnInteractionChanged");
            //if (_pickupable != null && _mono.IsMounted)
            //{
            //    QuickLogger.Info("Changing Pickupable");
            //    _pickupable.isPickupable = obj;
            //    if (_collider != null)
            //    {
            //        _collider.isTrigger = !obj;
            //    }

            //}
        }
        
        internal void UpdateDisplay()
        {
            _counter.text = $"{_mono.GetTotal()} / {_mono.StorageLimit}";
        }
    }
}
