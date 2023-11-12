using FCS_AlterraHub.Models.Interfaces;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using static RendererMaterialsStorageManager;

namespace FCS_AlterraHub.Models.Mono;
public class FCSStorage : StorageContainer, IFCSStorage
{
    [SerializeField] private string classID;
    [SerializeField] [Description("Allows storage default click to open storage")] private bool isAllowedToClickStorage = true;
    public bool IsAllowedToRemove { get; set; } = true;
    public Action OnContainerOpened { get; set; }
    public int SlotsAssigned;
    public Action<int, int> OnContainerUpdate { get; set; }
    public int GetContainerFreeSpace => SlotsAssigned - GetCount();
    public bool IsFull => GetCount() >= SlotsAssigned;
    public List<TechType> InvalidTechTypes = new List<TechType>();
    private bool _isSubscribed;
    [SerializeField] private List<TechType> AllowedTech;
    [Description("Allows storage to be visible in DSS Network")] public bool IsVisibleInNetwork;

    public IsAllowedToAdd isAllowedToAdd;

    public ItemsContainer ItemsContainer
    {
        get
        {
            if (container != null || storageRoot == null) return container;
            CreateContainer();
            Subscribe();
            return container;
        }
    }

    public override void Awake()
    {
        storageRoot.classId = classID;
        base.Awake();
        Subscribe();
    }

    private void Start()
    {
        if(isAllowedToClickStorage)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    private void Subscribe()
    {
        if (container == null || _isSubscribed) return;
        container.isAllowedToAdd += IsAllowedToAdd;
        container.isAllowedToRemove += IsAllowedToRemoveItems;
        container.onRemoveItem += OnRemoveItem;
        foreach (var item in AllowedTech)
        {
            AddAllowedTech(item);
        }
        _isSubscribed = true;
    }

    public void AddAllowedTech(TechType item)
    {
        if (container.allowedTech is null)
        {
            container.allowedTech = new();
        }
        container.allowedTech.Add(item);
    }

    private void OnRemoveItem(InventoryItem item)
    {
        QuickLogger.Debug("FCSStorage Container Item Removed", true);
    }


    public Action OnContainerClosed { get; set; }

    public int GetCount()
    {
        var i = 0;

        if (container == null) return i;

        foreach (TechType techType in ItemsContainer.GetItemTypes())
        {
            i += ItemsContainer.GetItems(techType).Count;
        }

        return i;
    }

    public int GetFreeSpace()
    {
        return SlotsAssigned - GetCount();
    }

    public Dictionary<TechType, int> GetItemsWithin()
    {
        List<TechType> keys = ItemsContainer.GetItemTypes();
        var lookup = keys?.Where(x => x != TechType.None).ToLookup(x => x).ToArray();
        return lookup?.ToDictionary(count => count.Key, count => ItemsContainer.GetCount(count.Key));
    }

    public virtual bool AddItemToContainer(InventoryItem item)
    {
        container.UnsafeAdd(item);
        return true;
    }

    public virtual bool IsAllowedToRemoveItems(Pickupable pickupable, bool verbose)
    {
        return true;
    }

    public override void Open(Transform useTransform)
    {
        QuickLogger.Debug("Attempting Open Storage",true);
        if (!IsAllowedToOpen) return;
        QuickLogger.Debug("Opening Storage", true);
        base.Open(useTransform);
        OnContainerOpened?.Invoke();
    }

    public bool IsAllowedToOpen { get; set; } = true;

    public virtual bool CanBeStored(int amount, TechType techType)
    {
        QuickLogger.Debug($"GetCount: {GetCount()} | Amount {amount} | Slots: {SlotsAssigned}", true);

        if (InvalidTechTypes.Contains(techType) || IsFull || (container.allowedTech != null && container.allowedTech.Any() && !container.allowedTech.Contains(techType))) return false;

        return GetCount() + amount <= SlotsAssigned;
    }

    public virtual bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        return !NotAllowedToAddItems && CanBeStored(1, pickupable.GetTechType()) && (isAllowedToAdd is  null ||  isAllowedToAdd.Invoke(pickupable,verbose));
    }

    public bool NotAllowedToAddItems { get; set; }

    public virtual bool IsAllowedToRemoveItems()
    {
        return IsAllowedToRemove;
    }

    public virtual Pickupable RemoveItemFromContainer(TechType techType)
    {
        return ItemsContainer.RemoveItem(techType);
    }

    public virtual bool ContainsItem(TechType techType)
    {
        return ItemsContainer.Contains(techType);
    }

    public override void OnClose()
    {
        OnContainerClosed?.Invoke();
    }

    /// <summary>
    /// To deactivate the storage container to prevent hover
    /// </summary>
    public void Deactivate()
    {
        enabled = false;
    }

    /// <summary>
    /// To activate the storage container to allow hover
    /// </summary>
    public void Activate()
    {
        enabled = true;
    }
}
