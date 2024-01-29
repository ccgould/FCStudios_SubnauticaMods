using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono.uGUI;
internal class uGUI_HarvesterSpeedSwitch : MonoBehaviour 
{
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite[] _images;
    private int _index;

    public void OnClick()
    {
        SwitchImage();
    }

    private void SwitchImage()
    {
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
            case HarvesterSpeedModes.Off:
                _index = 0;
                _icon.sprite = _images[_index];
                break;
        }
    }
}
