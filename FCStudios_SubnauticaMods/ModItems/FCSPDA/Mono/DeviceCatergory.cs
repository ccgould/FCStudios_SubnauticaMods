using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

internal class DeviceCatergory : MonoBehaviour
{
    [SerializeField]
    private Transform _content;
    private DevicePageController _controller;
    [SerializeField]
    private TMP_Text _title;
    [SerializeField]
    private List<FCSDevice> _devices;
    [SerializeField]
    private GameObject _deviceEntryPrefab;

    internal void Initialize(DevicePageController controller,KeyValuePair<string, List<FCSDevice>> devices)
    {
        _controller = controller;
        _title.text = devices.Key;
        _devices = devices.Value;

        foreach (var device in devices.Value)
        {
            if (!device.IsVisibleInPDA && !_controller.GetShowAllState()) continue;
            var item = Instantiate(_deviceEntryPrefab);
            var deviceEntry = item.GetComponent<DeviceEntry>();
            deviceEntry.Initialize(device);
            item.gameObject.transform.SetParent(_content, false);
        }
    }

    public void OnInfoButtonClicked()
    {
        if (_devices.Count > 0)
        {
            FCSPDAController.Main.GetGUI().OnInfoButtonClicked?.Invoke(_devices[0]?.GetTechType() ?? TechType.None);
        }
    }

    public void OnUpdateToggleValueChanged(bool b)
    {
        _content.gameObject.SetActive(b);
    }
}