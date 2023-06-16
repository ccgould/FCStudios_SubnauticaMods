using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Dialogs;
internal class uGUI_WorkGroupCreationDialog : MonoBehaviour
{
    [SerializeField] private Transform grid;
    [SerializeField] private Transform deviceTypesGrid;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private ToggleGroup devicesToggleGroup;
    [SerializeField] private uGUI_WorkUnitsPage uGUI_WorkUnitsPage;
    [SerializeField] private MenuController menuManager;
    private TechType selectedTechType;
    private HabitatManager _baseManager;
    private string _unitName;
    private TechType _currentTechType;

    public event EventHandler<OnWorkGroupCreatedArg> OnWorkGroupCreated;
    public class OnWorkGroupCreatedArg : EventArgs
    {
        public List<FCSDevice> devices = new();
    }


    private void Start()
    {
        _baseManager = HabitatService.main.GetPlayersCurrentBase();
    }

    private void CreateItems()
    {
        if (selectedTechType == TechType.None || selectedTechType == _currentTechType) return;

        _currentTechType = selectedTechType;

        ClearDevicesList();

        foreach (var device in _baseManager.GetConnectedDevices(true))
        {
            var currentDevice = device.Value;
            if (currentDevice.GetTechType() != selectedTechType) continue;
            var item = Instantiate(itemTemplate, grid);
            var controller = item.GetComponent<uGUI_WorkGroupListItem>();
            var toggle = item.gameObject.GetComponent<Toggle>();
            toggle.group = devicesToggleGroup;
            controller.Initialize(currentDevice);
        }
    }

    private void ClearDevicesList()
    {
        foreach (Transform child in grid)
        {
            if (child == itemTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    public void RefreshUI()
    {
        CreateDeviceTypeItems();
    }

    private void CreateDeviceTypeItems()
    {
        var techTypes = new HashSet<TechType>();

        ClearDeviceTypesList();

        foreach (var device in _baseManager.GetConnectedDevices(true))
        {
            techTypes.Add(device.Value.GetTechType());
        }

        foreach (var techType in techTypes)
        {
            var item = Instantiate(itemTemplate, deviceTypesGrid);
            var controller = item.GetComponent<uGUI_WorkGroupListItem>();
            controller.Initialize(techType);
            var toggle = item.GetComponent<Toggle>();
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener((s) =>
            {
                selectedTechType = toggle.gameObject.GetComponent<uGUI_WorkGroupListItem>().GetTechType();
                CreateItems();
            });
        }
    }

    private void ClearDeviceTypesList()
    {
        foreach (Transform child in deviceTypesGrid)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnDone()
    {
        if(string.IsNullOrEmpty(_unitName))
        {
            return;
        }

        if(!devicesToggleGroup.AnyTogglesOn())
        {
            return;
        }

        var list = new List<IWorkUnit>();
        var toggles = grid.GetAllComponentsInChildren<Toggle>();

        foreach (Toggle toggle in toggles)
        {
            if (!toggle.isOn) continue;
            var listItem = toggle.GetComponent<uGUI_WorkGroupListItem>();
            list.Add((IWorkUnit)listItem.GetDevice());
        }

        _baseManager.CreateWorkUnit(list);

        uGUI_WorkUnitsPage.RefreshUI();

        ClearDeviceTypesList();

        ClearDevicesList();

        menuManager.PopPage();
    }

    public void OnTextChanged(string value)
    {
        _unitName = value;
    }
}
