using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Helpers;
using FCSCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static FCS_EnergySolutions.Configuration.SaveData;

namespace FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Mono;
internal class AlterraGenController : FCSDevice, IFCSSave<SaveData>
{
    private int _isOperational;
    private AlterraGenDataEntry _savedData;

    [SerializeField] private GameObject _xBubbles;
    [SerializeField] private HoverInteraction _interaction;

    [SerializeField] private AlterraGenPowerManager powerManager;
    [SerializeField] private AnimationManager animationManager;
    [SerializeField] private DumpContainer dumpContainer;
    [SerializeField] private FCSStorage storageContainer;
    [SerializeField] private AlterraGenDisplay  alterraGenDisplay;


    //public override TechType[] AllowedTransferItems { get; } = Mod.AllowedBioItems().ToArray();

    //public override int MaxItemAllowForTransfer { get; } = 9;

    private void onSettingsKeyPressed(TechType type)
    {
        dumpContainer.OpenStorage();
    }

    private void OnPowerUpdateCycle()
    {
        if (powerManager.ProducingPower)
        {
            _xBubbles.SetActive(true);
            if (animationManager.GetBoolHash(_isOperational)) return;
            animationManager.SetBoolHash(_isOperational, true);
            QuickLogger.Debug("Starting Animation", true);
        }
        else
        {
            _xBubbles.SetActive(false);
            if (!animationManager.GetBoolHash(_isOperational)) return;
            animationManager.SetBoolHash(_isOperational, false);
            QuickLogger.Debug("Stopping Animation", true);

        }
    }

    #region Unity Methods

    public override void Awake()
    {
        base.Awake();

        InvokeRepeating(nameof(OnPowerUpdateCycle), 0.5f, 0.5f);
        _isOperational = Animator.StringToHash("IsOperational");
        _interaction.onSettingsKeyPressed += onSettingsKeyPressed;
        storageContainer.ItemsContainer.onAddItem += ItemsContainer_onAddItem;
        storageContainer.ItemsContainer.onAddItem += ItemsContainer_onRemoveItem;

        foreach (var item in AlterraGenHelpers.GetBioChargeValues())
        {
            dumpContainer.GetFCSStorage().AddAllowedTech(item.Key);
        }       
    }

    private void ItemsContainer_onAddItem(InventoryItem item)
    {
        alterraGenDisplay.RefreshItems();
    }

    private void ItemsContainer_onRemoveItem(InventoryItem item)
    {
        alterraGenDisplay.RefreshItems();
    }

    public override void OnEnable()
    {
        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (IsFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                var save = _savedData as AlterraGenDataEntry;

                if(save != null)
                {
                    //_colorManager.LoadTemplate(_savedData.ColorTemplate);

                    powerManager.LoadFromSave(_savedData);
                }
            }

            _runStartUpOnEnable = false;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if(storageContainer is not null)
        {
            storageContainer.ItemsContainer.onAddItem -= ItemsContainer_onAddItem;
            storageContainer.ItemsContainer.onRemoveItem -= ItemsContainer_onRemoveItem;
        }

    }

    #endregion

    #region Public Methods



    public override void Initialize()
    {
        //MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
        //_xBubbles = GameObjectHelpers.FindGameObject(gameObject, "xBubbles");

        IsInitialized = true;
    }

    internal void SetXBubbles(bool value)
    {
        if (_xBubbles != null)
        {
            _xBubbles.SetActive(value);
        }
    }

    public bool AddItemToContainer(InventoryItem item)
    {
        return true;//powerManager.AddItemToContainer(item);
    }

    #endregion

    #region IConstructable

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;

        if (constructed)
        {
            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                IsInitialized = true;
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }
    }


    #endregion

    #region IProtoEventListener


    public override void ReadySaveData()
    {
        _savedData = ModSaveManager.GetSaveData<AlterraGenDataEntry>(GetPrefabID());
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer)
    {
        QuickLogger.Debug("Saving Server Rack", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new AlterraGenDataEntry();
        }

        var save = _savedData as AlterraGenDataEntry;

        _savedData.Id = GetPrefabID();

        QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
        //save.ColorTemplate = _colorManager.SaveTemplate();
        save.ToConsume = powerManager.GetToConsume();
        save.PowerState = powerManager.PowerState;
        save.StoredPower = powerManager.GetStoredPower();
        save.Power = powerManager.GetPowerSourcePower();
        newSaveData.Data.Add(_savedData);
    }

    internal int GetMaxSlots()
    {
        return storageContainer.SlotsAssigned;
    }

    internal AnimationManager GetAnimationManager()
    {
        return animationManager;
    }

    internal AlterraGenPowerManager GetPowerManager()
    {
        return powerManager;
    }

    internal int GetItemCount()
    {
        return storageContainer.GetCount();
    }

    internal Dictionary<TechType,int> GetStorgeItems()
    {
        return storageContainer.GetItemsWithin();
    }

    #endregion
}
