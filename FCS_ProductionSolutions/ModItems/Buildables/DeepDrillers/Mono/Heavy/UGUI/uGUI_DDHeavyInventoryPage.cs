using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Components.uGUIComponents;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DDHeavyInventoryPage : Page, IuGUIAdditionalPage
{
    [SerializeField] private List<uGUI_StorageItem> _trackedItems;
    [SerializeField] private Text _itemCounter;
    [SerializeField] private PaginatorController _paginatorController;
    [SerializeField] private GridHelper _inventoryGrid;

    public override void Awake()
    {
        base.Awake();
        _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
    }

    public void OnButtonClick(uGUI_StorageItem index)
    {
        QuickLogger.Debug($"Give item {index.GetTechType().AsString()}",true);
        _sender.RemoveItemFromContainer(index.GetTechType());
        index.Refresh(_sender.GetItemCount(index.GetTechType()));
    }

    private void OnLoadItemsGrid(DisplayData data)
    {
        try
        {
            if (_sender is null) return;
            var grouped = _sender.GetDDContainer();

            if(grouped == null) return;

            QuickLogger.Debug($"Inventory Count: {grouped.Count()}",true);


            QuickLogger.Debug("==================================================");
            foreach (KeyValuePair<TechType, int> pair in grouped)
            {
                QuickLogger.Debug(pair.Key.AsString());
            }
            QuickLogger.Debug("==================================================");

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            foreach (uGUI_StorageItem displayItem in _trackedItems)
            {
                displayItem.Reset();
            }

            int w = 0;

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                _trackedItems[w++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
            }


            _inventoryGrid?.UpdaterPaginator(grouped.Count);

            if (_inventoryGrid is not null)
            {
                _paginatorController?.ResetCount(_inventoryGrid.GetMaxPages());

            }

            RefreshStorageAmount();
        }
        catch (Exception e)
        {
            QuickLogger.Error("Error Caught");
            QuickLogger.Error($"Error Message: {e.Message}");
            QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
        }
    }

    internal void RefreshStorageAmount()
    {
        if (_itemCounter == null) return;
        //_itemCounter.text = AuxPatchers.InventoryStorageFormat(_sender.GetDDContainer().GetContainerTotal(), _sender.GetDDContainer().GetStorageSize());
    }

    private void Refresh(int amount)
    {
        _inventoryGrid?.DrawPage();
    }


    private DeepDrillerOperatorController _sender;


    public IFCSObject GetController()
    {
        return _sender;
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        if(arg != null)
        {
            _sender = arg as DeepDrillerOperatorController;
            _sender.OnStorageCountUpdated += Refresh;
        }
    }

    public override void Exit()
    {
        if(_sender is not null)
        {
            _sender.OnStorageCountUpdated -= Refresh;
        }
        base.Exit();
    }
}
