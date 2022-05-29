using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class VehicleButton : FCSToggleButton
    {
        private Text _title;
        private readonly List<Image> _icons = new List<Image>();
        private Vehicle _vehicle;
        private bool _isInitialized;

        private void Initialize()
        {
            if(_isInitialized) return;
            BtnName = "VehicleBTN";
            STARTING_COLOR = new Color(0.1254902f, 0.5607843f, 0.6588235f, 1);
            HOVER_COLOR = new Color(0.181f, 0.652f, 0.708f, 1);
            _title = gameObject.GetComponentInChildren<Text>();
            
            foreach (Transform child in transform)
            {
                _icons.Add(child.GetComponent<Image>());
            }
            
            
            InvokeRepeating(nameof(UpdateState), 1f, 1f);
            _isInitialized = true;
        }


        internal void Set(string name, Vehicle vehicle)
        {
            Initialize();

            Tag = vehicle;
            _vehicle = vehicle;
            ChangeIcon(_vehicle is SeaMoth ? DssListItemIcon.Seamoth : DssListItemIcon.Prawn);
            gameObject.SetActive(true);
        }


        private void UpdateState()
        {
            if (_vehicle == null) return;
            
            if (_title != null)
            {
#if SUBNAUTICA
                _title.text = _vehicle.GetName();
#else
                _title.text = _vehicle.vehicleName;
#endif
            }
        }

        internal void ChangeIcon(DssListItemIcon icon)
        {
            if (_icons == null) return;
            int index = (int)icon;

            QuickLogger.Debug($"Icon Index: {index} | Count {_icons.Count}");

            for (int i = 0; i < _icons.Count; i++)
            {
                QuickLogger.Debug($"Icon Index Name: {_icons[i]?.gameObject.name}");
                _icons[i]?.gameObject.SetActive(i == index);
            }
        }

        internal void Reset()
        {
            gameObject.SetActive(false);
        }

        internal Vehicle GetVehicle()
        {
            return _vehicle;
        }
    }
}