using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.Mods.Replicator.Mono;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class ReplicatorSpeedButton : InterfaceButton
    {
        private int _index;
        private Image _icon;
        private Sprite[] _images;
        internal ReplicatorController ReplicatorController { get; set; }

        private void Initialize()
        {
            if (_icon == null)
            {
                _icon = gameObject.EnsureComponent<Image>();
            }

            if (_images == null)
            {
                _images = new[]
                {
                    ModelPrefab.GetSprite("HH icon off"),
                    ModelPrefab.GetSprite("HH icon min"),
                    ModelPrefab.GetSprite("HH icon low"),
                    ModelPrefab.GetSprite("HH icon high"),
                    ModelPrefab.GetSprite("HH icon max")
                };
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            SwitchImage();
            ReplicatorController.SetSpeedMode(GetMode());
        }

        private void SwitchImage()
        {
            Initialize();
            _index += 1;

            if (_index > _images.Length - 1)
            {
                _index = 0;
            }
            _icon.sprite = _images[_index];
        }

        public SpeedModes GetMode()
        {
            switch (_index)
            {
                case 0:
                    return SpeedModes.Off;
                case 1:
                    return SpeedModes.Min;
                case 2:
                    return SpeedModes.Low;
                case 3:
                    return SpeedModes.High;
                case 4:
                    return SpeedModes.Max;
            }

            return SpeedModes.Off;
        }

        public void SetSpeedMode(SpeedModes speedMode)
        {
            Initialize();

            switch (speedMode)
            {
                case SpeedModes.Max:
                    _index = _images.Length - 1;
                    _icon.sprite = _images[_index];
                    break;
                case SpeedModes.High:
                    _index = 3;
                    _icon.sprite = _images[_index];
                    break;
                case SpeedModes.Low:
                    _index = 2;
                    _icon.sprite = _images[_index];
                    break;
                case SpeedModes.Min:
                    _index = 1;
                    _icon.sprite = _images[_index];
                    break;
            }  
        }
    }
}