using FCS_AlterraHub.Models.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

internal class DeviceEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private TechType _techType;
    private FCSDevice _device;
    [SerializeField]
    private uGUI_Icon _icon;
    [SerializeField]
    private TMP_Text _text;

    public void OnPointerClick(PointerEventData eventData)
    {
        FCSPDAController.Main.OpenDeviceUI(_techType, _device);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    internal void Initialize(FCSDevice device)
    {
        _techType = device.GetTechType();
        _device = device;
        _icon.sprite = SpriteManager.Get(device.GetTechType());
        _text.text = device.GetDeviceName();
    }
}