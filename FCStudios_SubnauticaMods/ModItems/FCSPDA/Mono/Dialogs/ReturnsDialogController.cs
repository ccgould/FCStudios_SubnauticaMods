using System;
using System.Collections.Generic;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class ReturnsDialogController : MonoBehaviour
{
    [SerializeField]
    private Transform _list;
    private readonly List<AlterraHubReturnItemController> _toggles = new List<AlterraHubReturnItemController>();
    [SerializeField]
    private FCSAlterraHubGUI _mono;

    public Action OnClose { get; set; }

    public void OnDoneButtonClicked()
    {
        foreach (AlterraHubReturnItemController toggle in _toggles)
        {
            if (toggle.IsChecked)
            {
                AccountService.main.Refund(FCSAlterraHubGUISender.None, toggle.InventoryItem.item.GetTechType());
                Destroy(toggle.InventoryItem.item.gameObject);
            }
        }
        Close();
    }

    private void RefreshReturnItemsList()
    {
        for (var i = _toggles.Count - 1; i >= 0; i--)
        {
            AlterraHubReturnItemController item = _toggles[i];
            item.UnRegisterAndDestroy();
            _toggles.Remove(item);
        }

        foreach (InventoryItem inventoryItem in Inventory.main.container)
        {
            var techType = inventoryItem.item.GetTechType();
            if (!StoreInventoryService.IsKit(techType) || StoreInventoryService.InvalidReturnItem(techType)) continue;

            var depotPrefab = GameObject.Instantiate(ModPrefabService.GetPrefab("AlterraHubDepotItem"));
            var controller = depotPrefab.AddComponent<AlterraHubReturnItemController>();
            controller.Initialize(inventoryItem, _list);
            _toggles.Add(controller);
        }
    }

    public void Close(bool isCancel = false)
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        RefreshReturnItemsList();
        gameObject.SetActive(true);
    }

    public bool IsOpen => gameObject.activeSelf;
}