using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static FCS_AlterraHub.Configuation.SaveData;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseModuleRack.Mono;
internal class BaseManagerRackController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField]
    private Transform _storageRoot;
    private Equipment _equipment;

    [SerializeField]
    private uGUI_Icon _iconSlot1;
    [SerializeField]
    private uGUI_Icon _iconSlot2;
    [SerializeField]
    private uGUI_Icon _iconSlot3;
    [SerializeField]
    private uGUI_Icon _iconSlot4;
    [SerializeField]
    private uGUI_Icon _iconSlot5;
    [SerializeField]
    private uGUI_Icon _iconSlot6;

    private Equipment equipment
    {
        get
        {
            if (this._equipment == null)
            {
                this._equipment = new Equipment(base.gameObject, _storageRoot);
                this._equipment.SetLabel("Base Manager Rack");
                this._equipment.isAllowedToAdd = new IsAllowedToAdd(this.IsAllowedToAdd);
                this._equipment.isAllowedToRemove = new IsAllowedToRemove(this.IsAllowedToRemove);
                this._equipment.compatibleSlotDelegate = new Equipment.DelegateGetCompatibleSlot(this.GetCompatibleSlot);
                this._equipment.onEquip += this.OnEquip;
                this._equipment.onUnequip += this.OnUnequip;
                this.UnlockDefaultEquipmentSlots();
            }
            return this._equipment;
        }
    }

    internal static string[] slotIDs = new string[]
    {
            "BaseManagerModule1",
            "BaseManagerModule2",
            "BaseManagerModule3",
            "BaseManagerModule4",
            "BaseManagerModule5",
            "BaseManagerModule6",
    };

    private List<TechType> allowedTech = new List<TechType>()
    {
        BaseManagerBuildable.RemoteModuleTechType,
        BaseManagerBuildable.TranceiverModuleTechType,

    };
    private int slotsFilled;

    private bool GetCompatibleSlot(EquipmentType itemType, out string slot)
    {
        if (itemType == EquipmentType.NuclearReactor)
        {
            int num = slotIDs.Length;
            for (int i = 0; i < num; i++)
            {
                string text = slotIDs[i];
                InventoryItem itemInSlot = this.equipment.GetItemInSlot(text);
                if (itemInSlot == null)
                {
                    slot = text;
                    return true;
                }
                Pickupable item = itemInSlot.item;
                if (item != null && item.GetTechType() == TechType.DepletedReactorRod)
                {
                    slot = text;
                    return true;
                }
            }
        }
        slot = null;
        return false;
    }

    private void OnUnequip(string slot, InventoryItem item)
    {
        switch (GetSlotIndex(slot))
        {
            case "1":
                _iconSlot1.sprite = SpriteManager.Get(TechType.None);
                _iconSlot1.gameObject.SetActive(false);
                break;
            case "2":
                _iconSlot2.sprite = SpriteManager.Get(TechType.None);
                _iconSlot2.gameObject.SetActive(false);
                break;
            case "3":
                _iconSlot3.sprite = SpriteManager.Get(TechType.None);
                _iconSlot3.gameObject.SetActive(false);
                break;
            case "4":
                _iconSlot4.sprite = SpriteManager.Get(TechType.None);
                _iconSlot4.gameObject.SetActive(false);
                break;
            case "5":
                _iconSlot5.sprite = SpriteManager.Get(TechType.None);
                _iconSlot5.gameObject.SetActive(false);
                break;
            case "6":
                _iconSlot6.sprite = SpriteManager.Get(TechType.None);
                _iconSlot6.gameObject.SetActive(false);
                break;
        }

        slotsFilled -= 1;
    }

    private void OnEquip(string slot, InventoryItem item)
    {
        switch (GetSlotIndex(slot))
        {
            case "1":
                _iconSlot1.sprite = SpriteManager.Get(item.techType);
                _iconSlot1.gameObject.SetActive(true);
                break;
            case "2":
                _iconSlot2.sprite = SpriteManager.Get(item.techType);
                _iconSlot2.gameObject.SetActive(true);
                break;
            case "3":
                _iconSlot3.sprite = SpriteManager.Get(item.techType);
                _iconSlot3.gameObject.SetActive(true);
                break;
            case "4":
                _iconSlot4.sprite = SpriteManager.Get(item.techType);
                _iconSlot4.gameObject.SetActive(true);
                break;
            case "5":
                _iconSlot5.sprite = SpriteManager.Get(item.techType);
                _iconSlot5.gameObject.SetActive(true);
                break;
            case "6":
                _iconSlot6.sprite = SpriteManager.Get(item.techType);
                _iconSlot6.gameObject.SetActive(true);
                break;
        }

        slotsFilled += 1;
    }

    private string GetSlotIndex(string slot)
    {
        return slot.Substring(slot.Length - 1);
    }

    private void UnlockDefaultEquipmentSlots()
    {
        equipment.AddSlots(slotIDs);
    }

    public override void Awake()
    {
        base.Awake();
        var interaction = gameObject.GetComponent<HoverInteraction>();
        interaction.onSettingsKeyPressed += onSettingsKeyPressed;
        Plugin.AlterraHubSaveData.OnStartedSaving += OnBeforeSave;
    }

    public override void Start()
    {
        base.Start();

        if (Plugin.AlterraHubSaveData.savedRacks is null || Plugin.AlterraHubSaveData.savedRacks.Count <= 0)
        {
            Plugin.AlterraHubSaveData.Load();
        }        

        CoroutineHost.StartCoroutine(LoadSaveData());        
    }

    public void OnBeforeSave(object sender, EventArgs e)
    {
        if (!IsConstructed) return;

        if (Plugin.AlterraHubSaveData.savedRacks.TryGetValue(GetPrefabID(), out BaseRackSaveData data))
        {
            data.EquippedTechTypesInSlots = new Dictionary<string, int>();

            foreach (var pair in _equipment.equipment)
            {
                if (pair.Value == null || pair.Value.item == null)
                    data.EquippedTechTypesInSlots.Add(pair.Key, 0);
                else
                    data.EquippedTechTypesInSlots.Add(pair.Key, (int)pair.Value.item.GetTechType());
            }
        }
        else
        {
            BaseRackSaveData saveData = new BaseRackSaveData();

            saveData.EquippedTechTypesInSlots = new Dictionary<string, int>();
            foreach (var pair in equipment.equipment)
            {
                if (pair.Value == null || pair.Value.item == null)
                {
                    saveData.EquippedTechTypesInSlots.Add(pair.Key, 0);
                }
                else
                {
                    saveData.EquippedTechTypesInSlots.Add(pair.Key, (int)pair.Value.item.GetTechType());
                }
            }
            Plugin.AlterraHubSaveData.savedRacks.Add(GetPrefabID(), saveData);
        }
    }

    public IEnumerator LoadSaveData()
    {               
        if (Plugin.AlterraHubSaveData.savedRacks.TryGetValue(GetPrefabID(), out BaseRackSaveData data))
        {
            foreach (var SlottechType in data.EquippedTechTypesInSlots)
            {
                if (SlottechType.Value != 0)
                {
                    var obj = new TaskResult<GameObject>();
                    yield return CraftData.InstantiateFromPrefabAsync((TechType)SlottechType.Value, obj);
                    Pickupable pickupable = obj.Get().GetComponent<Pickupable>();

                    if (equipment.AddItem(SlottechType.Key, new InventoryItem(pickupable), true))
                    {
                        pickupable.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void onSettingsKeyPressed(TechType techType)
    {
        Player main = Player.main;

        PDA pda = main.GetPDA();

        Inventory.main.SetUsedStorage(equipment, false);
        pda.Open(PDATab.Inventory, gameObject.transform, (s) =>
        {
            QuickLogger.Debug("Container Closed", true);
        });
    }

    private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
    {
        return true;
    }

    private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        bool flag = false;
        if (pickupable != null)
        {
            TechType techType = pickupable.GetTechType();
            flag = allowedTech.Contains(techType);
        }
        if (!flag && verbose)
        {
            ErrorMessage.AddMessage("Item not allowed");
        }
        return flag;

        //RemoteConnectionModule
    }

    private bool ContainsTechType(TechType techType)
    {
        return false;
    }

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override bool IsDeconstructionObstacle()
    {
        return true;
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        
    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {

    }

    public override void ReadySaveData()
    {

    }

    internal int GetItemCount(TechType techType)
    {
        return equipment.GetCount(techType);
    }

    internal int GetItemCount()
    {
        return slotsFilled;
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
    }

    internal bool HasModule(TechType remoteModelTechType)
    {
        foreach (var slotID in slotIDs)
        {
            if(equipment.GetTechTypeInSlot(slotID) == remoteModelTechType)
                return true;
        }

        return false;
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {energyPerSecond * 60:F2}] [Is Connected: {IsRegisteredToBaseManager()}] [Slots: {slotsFilled}/6]",
        };
    }
}
