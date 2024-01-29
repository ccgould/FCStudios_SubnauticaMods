using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages.uGUI;
internal class uGUI_VehicleButton : MonoBehaviour
{
    [SerializeField] private Text _title;
    [SerializeField] private List<Image> _icons = new List<Image>();
    [SerializeField] private DSSVehicleListDialog dialiog;


    private Vehicle _vehicle;

    private void Start()
    {
        InvokeRepeating(nameof(UpdateState), 1f, 1f);
    }

    internal void Set(string name, Vehicle vehicle)
    {
        _vehicle = vehicle;
        ChangeIcon(_vehicle is SeaMoth ? _icons[0] : _icons[1]);
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

    internal void ChangeIcon(Image icon)
    {
        if (_icons == null) return;

        icon.gameObject.SetActive(true); 
    }

    internal void Reset()
    {
        gameObject.SetActive(false);
        foreach (var item in _icons)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        dialiog.OnVehicleItemButtonClick(_vehicle);
    }
}