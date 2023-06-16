using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Dialogs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Pages;

internal class uGUI_WorkUnitsPage : MonoBehaviour
{
    [SerializeField] private BaseManagerSideMenu baseManagerSideMenu;
    [SerializeField] private Transform grid;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private uGUI_WorkGroupCreationDialog creationDialog;
    private HabitatManager _baseManager;

    private void Start()
    {
        _baseManager = HabitatService.main.GetPlayersCurrentBase();
    }

    private void CreateItems()
    {
        foreach (var device in _baseManager.GetWorkUnits())
        {
            var item = Instantiate(itemTemplate, grid);
            var controller = item.GetComponent<uGUI_WorkGroupListItem>();
            controller.Initialize(device.Key, device.Value);
        }
    }

    public void RefreshUI()
    {
        foreach (Transform child in grid)
        {
            if (child == itemTemplate) continue;
            Destroy(child.gameObject);
        }

        CreateItems();
    }
}
