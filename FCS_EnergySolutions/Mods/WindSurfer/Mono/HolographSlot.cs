using System;
using FCS_EnergySolutions.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    public class HolographSlot : MonoBehaviour
    {
        public Transform Target { get; set; }
        internal WindSurferOperatorController WindSurferOperatorController
        {
            get
            {
                if (_windsurferController == null)
                {
                    _windsurferController = GetComponentInParent<WindSurferOperatorController>();
                }

                return _windsurferController;
            }
        }

        internal PlatformController PlatformController
        {
            get
            {
                if (_platformController == null)
                {
                    _platformController = GetComponentInParent<PlatformController>();
                }

                return _platformController;
            }
        }

        internal HoloGraphControl HoloGraphControl
        {
            get
            {
                if (_holoGraphControl == null)
                {
                    _holoGraphControl = GetComponentInParent<HoloGraphControl>();
                }

                return _holoGraphControl;
            }
        }

        internal int ID;
        private WindSurferOperatorController _windsurferController;
        private Button _button;
        private HoloGraphControl _holoGraphControl;
        private PlatformController _platformController;
        private bool _initialized;

        internal Vector2Int Direction
        {
            get
            {
                switch (ID)
                {
                    case 1:
                        return Vector2Int.up;
                    case 2:
                        return Vector2Int.left;
                    case 3:
                        return Vector2Int.down;
                    case 4:
                        return Vector2Int.right;
                    default:
                        return Vector2Int.zero;
                }
            }
        }

        internal void Initialize()
        {
            if(_initialized) return;
            var idString = gameObject.name.Substring(gameObject.name.Length - 1);
            ID = Convert.ToInt32(idString);
            _button = gameObject.GetComponent<Button>();

            _button.onClick.AddListener((() =>
            {
                var result = WindSurferOperatorController.AddPlatform(this, GetPosition() + Direction);
                
                if (result)
                {
                    WindSurferOperatorController.RefreshHoloGrams();
                }

            }));

            InvokeRepeating(nameof(UpdateSlot),.1f,.1f);

            _initialized = true;
        }

        private void UpdateSlot()
        {
            _button.interactable = WindSurferOperatorController.ScreenTrigger.selected && WindSurferOperatorController.Grid.ElementAt(GetPosition() + Direction) == null;
        }


        private Vector2Int GetPosition()
        {
            return WindSurferOperatorController.Grid.Position(HoloGraphControl);
        }
    }
}