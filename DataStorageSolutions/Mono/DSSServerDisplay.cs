using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using FCSCommon.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class DSSServerDisplay : AIDisplay
    {
        private DSSServerController _mono;
        private Text _counter;

        internal void Setup(DSSServerController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                UpdateDisplay();
            }
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {

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

                var catcher = interactionFace.EnsureComponent<ServerHitController>();
                catcher.Controller = _mono;
                catcher.TextLineOne = string.Format(AuxPatchers.TakeServer(), Mod.ServerFriendlyName);
                catcher.TextLineTwo = "Data: {0}";
                catcher.GetAdditionalDataFromString = true;
                catcher.ButtonMode = InterfaceButtonMode.Background;

                #endregion


                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        internal void UpdateDisplay()
        {
            _counter.text = $"{_mono.GetTotal()} / {_mono.StorageLimit}";
        }
    }
}
