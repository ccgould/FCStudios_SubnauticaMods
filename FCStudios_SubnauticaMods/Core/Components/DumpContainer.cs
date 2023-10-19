using FCS_AlterraHub.Models.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
public class DumpContainer : MonoBehaviour
{
    [SerializeField] private ChildObjectIdentifier _containerRoot;
    [SerializeField] private int containerWidth;
    [SerializeField] private int containerHeight;
    [SerializeField] private string storageLabel = "Storage";
    private ItemsContainer _dumpContainer;

    [SerializeField] private FCSStorage _storage;

    public event Action OnDumpContainerClosed;
    public event Action<InventoryItem> OnDumpContainerAddItem;

    public void Start()
    {
        if (_dumpContainer == null)
        {
            QuickLogger.Debug("Initializing Container");

            _dumpContainer = new ItemsContainer(containerWidth, containerHeight, _containerRoot.transform, storageLabel, null);

            _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
            _dumpContainer.onAddItem += DumpContainerOnOnAddItem;
        }
    }

    public ItemsContainer GetItemsContainer()
    {
        return _dumpContainer;
    }

    public int GetTechTypeCount(TechType techType)
    {
        return _dumpContainer.GetItems(techType)?.Count ?? 0;
    }

    public IList<InventoryItem> GetTechType(TechType techType)
    {
        return _dumpContainer.GetItems(techType);
    }

    private void DumpContainerOnOnAddItem(InventoryItem item)
    {
        OnDumpContainerAddItem?.Invoke(item);
    }

    public int GetCount()
    {
        return _dumpContainer.count;
    }

    private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        var result = _storage.IsAllowedToAdd(pickupable, verbose);
        return result;
    }

    public void OpenStorage()
    {
        Player main = Player.main;
        PDA pda = main.GetPDA();

        if (_dumpContainer != null && pda != null)
        {
            Inventory.main.SetUsedStorage(_dumpContainer);
            pda.Open(PDATab.Inventory, null, OnDumpClose);
        }
        else
        {
            QuickLogger.Error($"Failed to open the pda values: PDA = {pda} || Dump Container: {_dumpContainer}");
        }
    }

    internal virtual void OnDumpClose(PDA pda)
    {
        QuickLogger.Debug($"Store Items Dump Count: {_dumpContainer.count}");
        var amount = _dumpContainer.count;

        for (int i = amount - 1; i > -1; i--)
        {
            QuickLogger.Debug($"Number of iteration: {i}");
            var item = _dumpContainer.ElementAt(0);
            _dumpContainer.RemoveItem(item.item, true);
            _storage.AddItemToContainer(item);
        }

        OnDumpContainerClosed?.Invoke();
    }
}
