using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

internal class DevicePageController : Page
{
    [SerializeField]
    private Transform _content;
    [SerializeField]
    private Toggle _showAllToggle;
    [SerializeField]
    private GameObject _categoryPrefab;
    private Dictionary<string, DeviceCatergory> _groups = new();

    private void Awake()
    {
        _content = GameObjectHelpers.FindGameObject(gameObject.FindChild("Body"),"Content").transform;

        //_showAllToggle.onValueChanged.AddListener((b) => 
        //{
        //    Enter(null);
        //});
    }

    internal bool GetShowAllState()
    {
        return _showAllToggle.isOn;
    }

    public override void Enter(object arg)
    {
        base.Enter();

        for (int i = _groups.Count - 1; i >= 0; i--)
        {
            var item = _groups.ElementAt(i);
            Destroy(item.Value.gameObject);
            _groups.Remove(item.Key);
        }

        //Generate a list of devices
        var devices = HabitatService.main.GetDevicesInCurrentBase();

        foreach (var device in devices)
        {
            var category = Instantiate(_categoryPrefab);
            var deviceCat = category.GetComponent<DeviceCatergory>();
            deviceCat.Initialize(this,device);
            deviceCat.gameObject.transform.SetParent(_content, false);
            _groups.Add(device.Key, deviceCat);
        }
    }

    public override void OnBackButtonClicked()
    {
        FCSPDAController.Main.GetGUI().GoBackAPage();
    }
}