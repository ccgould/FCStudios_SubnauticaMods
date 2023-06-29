using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.Models.Mono.Handlers;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Pages;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
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
    [SerializeField] private uGUI_InputField workGroupInputField;
    [SerializeField] private uGUI_WorkUnitsPage uGUI_WorkUnitsPage;


    private TechType selectedTechType;
    private HabitatManager _baseManager;
    private HashSet<Toggle> selectedToggles = new();

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
        if (selectedTechType == TechType.None) return;

        ClearDevicesList();

        selectedToggles.Clear();

        foreach (var device in _baseManager.GetConnectedDevices(true))
        {
            var currentDevice = device.Value;
            if (currentDevice.GetTechType() != selectedTechType) continue;
            var item = Instantiate(itemTemplate, grid);
            var controller = item.GetComponent<uGUI_WorkGroupListItem>();
            var toggle = item.gameObject.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener((x) =>
            {
                if(x)
                {
                    selectedToggles.Add(toggle);
                }
                else
                {
                    selectedToggles.Remove(toggle);
                }
            });
            //toggle.group = devicesToggleGroup;
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
        Reset();
        CreateDeviceTypeItems();
    }

    private void CreateDeviceTypeItems()
    {
        var techTypes = new HashSet<TechType>();

        foreach (var device in _baseManager.GetConnectedDevices(true))
        {
            if(device.Value is IWorkUnit)
            {
                techTypes.Add(device.Value.GetTechType());
            }
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
        var groupName = workGroupInputField.text;
        if (string.IsNullOrEmpty(groupName))
        {
            //uGUI_NotificationManager.Instance.AddNotification($"");
            MessageBoxHandler.Instance.ShowMessage("Work Unit Name Required", FCSAlterraHubGUISender.PDA);
            return;
        }

        if (!selectedToggles.Any())
        {
            return;
        }

        var list = new List<IWorkUnit>();

        foreach (Toggle toggle in selectedToggles)
        {
            if (!toggle.isOn) continue;
            var listItem = toggle.GetComponent<uGUI_WorkGroupListItem>();
            list.Add((IWorkUnit)listItem.GetDevice());
        }
        _baseManager.CreateWorkUnit(list, groupName);
        uGUI_WorkUnitsPage.RefreshUI();

    }

    public void Reset()
    {
        selectedTechType = TechType.None;
        selectedToggles.Clear();
        workGroupInputField.text = string.Empty;
        ClearDeviceTypesList();
        ClearDevicesList();
    }
}
