using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
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

        public HarvesterSpeedModes GetMode()
        {
            switch (_index)
            {
                case 0:
                    return HarvesterSpeedModes.Off;
                case 1:
                    return HarvesterSpeedModes.Min;
                case 2:
                    return HarvesterSpeedModes.Low;
                case 3:
                    return HarvesterSpeedModes.High;
                case 4:
                    return HarvesterSpeedModes.Max;
            }

            return HarvesterSpeedModes.Off;
        }

        public void SetSpeedMode(HarvesterSpeedModes harvesterSpeedMode)
        {
            Initialize();

            switch (harvesterSpeedMode)
            {
                case HarvesterSpeedModes.Max:
                    _index = _images.Length - 1;
                    _icon.sprite = _images[_index];
                    break;
                case HarvesterSpeedModes.High:
                    _index = 3;
                    _icon.sprite = _images[_index];
                    break;
                case HarvesterSpeedModes.Low:
                    _index = 2;
                    _icon.sprite = _images[_index];
                    break;
                case HarvesterSpeedModes.Min:
                    _index = 1;
                    _icon.sprite = _images[_index];
                    break;
            }

            ReplicatorController.SetSpeedMode(harvesterSpeedMode);
        }
    }
}