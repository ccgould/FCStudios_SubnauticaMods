using FCS_AlterraHub.Core.Helpers;
using FCS_EnergySolutions.ModItems.Buildables.JetStream.Buildables;
using FCS_EnergySolutions.ModItems.Buildables.UniversalCharger.Enumerators;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.ModItems.Buildables.UniversalCharger.Mono;
internal class PowercellCharger : Charger
{
    [SerializeField] private UniversalChargerController controller;
    [SerializeField] private GameObject[] uiBatteries;

    private PowerChargerMode _powerChargerMode;
    //private StorageContainer _storageContainer;

    public override HashSet<TechType> allowedTech => _powerChargerMode == PowerChargerMode.Powercell ? TechDataHelpers.PowercellTech : TechDataHelpers.BatteryTech;
    public override string labelInteract => _powerChargerMode == PowerChargerMode.Powercell ? "PowerCellChargerInteract" : "BatteryChargerInteract";
    public override string labelStorage => _powerChargerMode == PowerChargerMode.Powercell ? "PowerCellChargerLabel" : "BatteryChargerStorageLabel";
    public override string labelIncompatibleItem => _powerChargerMode == PowerChargerMode.Powercell ? "PowerCellChargerIncompatibleItem" : "BatteryChargerIncompatibleItem";
    public override string labelCantDeconstruct => _powerChargerMode == PowerChargerMode.Powercell ? "PowerCellChargerCantDeconstruct" : "BatteryChargerCantDeconstruct";

    //public override bool Initialize()
    //{
    //    //if (this.slotDefinitions == null)
    //    //{
    //    //    slotDefinitions = new();
    //    //}

    //    //if (this.equipment == null)
    //    //{
    //    //    this.equipment = new Equipment(base.gameObject, this.equipmentRoot.transform);

    //    //    this.equipment.isAllowedToAdd = new IsAllowedToAdd(this.IsAllowedToAdd);
    //    //    this.equipment.onEquip += this.OnEquip;
    //    //    this.equipment.onUnequip += this.OnUnequip;
    //    //    this.batteries = new Dictionary<string, IBattery>();
    //    //    this.slots = new Dictionary<string, Charger.SlotDefinition>();

    //    //    RefreshSlots();
    //    //    return true;
    //    //}

    //    var f = base.Initialize();

    //    RefreshSlots();
    //    return f;
    //}

    private new void Awake()
    {
        equipmentRoot.classId = UniversalChargerBuildable.PatchedClassID;
        base.Initialize();
    }

    private void RefreshSlots()
    {
        equipment.Clear();
        batteries.Clear();
        slots.Clear();
        slotDefinitions.Clear();

        this.equipment.SetLabel(this.labelStorage);

        for (var index = 0; index < 10; index++)
        {
            var slotName = _powerChargerMode == PowerChargerMode.Powercell ? UniversalChargerBuildable.ucPowercellSlots[index] : UniversalChargerBuildable.ucBatterySlots[index];

            slotDefinitions.Add(new SlotDefinition
            {
                id = slotName,
                bar = uiBatteries[index].FindChild("BatteryFill").GetComponent<Image>(),
                text = uiBatteries[index].GetComponentInChildren<TextMeshProUGUI>()
            });
        }

        int i = 0;
        int count = slotDefinitions.Count;

        while (i < count)
        {
            Charger.SlotDefinition slotDefinition = this.slotDefinitions[i];
            string id = slotDefinition.id;
            if (!string.IsNullOrEmpty(id) && !this.batteries.ContainsKey(id))
            {
                this.batteries[id] = null;
                this.slots[id] = slotDefinition;
                Image bar = slotDefinition.bar;
                if (bar != null)
                {
                    bar.material = new Material(bar.material);
                }
            }

            i++;
        }

        this.UnlockDefaultEquipmentSlots();
        this.UpdateVisuals();
    }
    
    internal void SetMode(PowerChargerMode mode)
    {
        _powerChargerMode = mode;
        RefreshSlots();
        controller.UpdateUIIToggles(mode);
    }

    internal PowerChargerMode GetMode()
    {
        return _powerChargerMode;
    }

    public Dictionary<string, string> Save()
    {
        this.serializedSlots = equipment.SaveEquipment();
        return serializedSlots;
    }

    public void Load(Dictionary<string, string> savedData)
    {
        QuickLogger.Debug($"loading powercells {savedData.Count}", true);
        this.serializedSlots = savedData;
        Start();
    }

    internal PowerChargerMode ToggleMode()
    {
        var newMode = _powerChargerMode == PowerChargerMode.Powercell ? PowerChargerMode.Battery : PowerChargerMode.Powercell;
        SetMode(newMode);
        return newMode;
    }
}
