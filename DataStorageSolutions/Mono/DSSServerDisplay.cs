using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Linq;
using System.Text;
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
        private readonly StringBuilder _sb = new StringBuilder();

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
                catcher.GetAdditionalString += FormatData;
                catcher.ButtonMode = InterfaceButtonMode.Background;
                catcher.IsClickable = IsAllowedToClick;

                #endregion


                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool IsAllowedToClick()
        {
            if (_mono.GetSlot() != null && _mono.GetSlot().GetConnectedDevice().IsDeviceOpen())
            {
                return true;
            }

            return false;
        }

        internal void UpdateDisplay()
        {
            _counter.text = $"{_mono.GetTotal()} / {_mono.StorageLimit}";
        }

        private string FormatData()
        {
            _sb.Clear();

            _sb.Append(string.Format(AuxPatchers.FiltersCheckFormat(), _mono.GetFilters().Any()));
            _sb.Append(Environment.NewLine);
            var items = _mono.GetItemsWithin().ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                if (i < 4)
                {
                    _sb.Append($"{items[i].Key.AsString()} x{items[i].Value}");
                    _sb.Append(Environment.NewLine);
                }
                else
                {
                    _sb.Append($"And More.....");
                    break;
                }
            }

            return _sb.ToString();
        }
    }
}
