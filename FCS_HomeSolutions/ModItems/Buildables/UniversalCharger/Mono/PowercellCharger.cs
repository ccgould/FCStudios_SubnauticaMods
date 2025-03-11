using FCS_AlterraHub.Core.Helpers;
using FCS_HomeSolutions.ModItems.Buildables.UniversalCharger.Buildables;
using FCS_HomeSolutions.ModItems.Buildables.UniversalCharger.Enumerators;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.ModItems.Buildables.UniversalCharger.Mono;
internal class PowercellCharger : Charger
{
    [SerializeField] private UniversalChargerController controller;
    [SerializeField] private GameObject[] uiBatteries;

    private PowerChargerMode _powerChargerMode;
    private float _consumedPower;

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
        RefreshSlots();
    }

    private new void Update()
    {
        _consumedPower = 0;
        sequence.Update();
        if (Time.deltaTime == 0f)
        {
            return;
        }
        if (nextChargeAttemptTimer > 0f)
        {
            nextChargeAttemptTimer -= DayNightCycle.main.deltaTime;
            if (nextChargeAttemptTimer < 0f)
            {
                nextChargeAttemptTimer = 0f;
            }
        }
        bool charging = false;
        if (nextChargeAttemptTimer <= 0f)
        {
            int num = 0;
            bool flag = false;
            PowerRelay powerRelay = PowerSource.FindRelay(transform);
            if (powerRelay != null)
            {
                float num2 = 0f;
                foreach (KeyValuePair<string, IBattery> keyValuePair in batteries)
                {
                    IBattery value = keyValuePair.Value;
                    if (value != null)
                    {
                        float charge = value.charge;
                        float capacity = value.capacity;
                        if (charge < capacity)
                        {
                            num++;
                            float num3 = DayNightCycle.main.deltaTime * chargeSpeed * capacity;
                            if (charge + num3 > capacity)
                            {
                                num3 = capacity - charge;
                            }
                            num2 += num3;
                        }
                    }
                }
                float num4 = 0f;
                if (num2 > 0f && powerRelay.GetPower() > num2)
                {
                    flag = true;
                    powerRelay.ConsumeEnergy(num2, out num4);
                    _consumedPower = num4;
                }
                if (num4 > 0f)
                {
                    charging = true;
                    float num5 = num4 / num;
                    foreach (KeyValuePair<string, IBattery> keyValuePair2 in batteries)
                    {
                        string key = keyValuePair2.Key;
                        IBattery value2 = keyValuePair2.Value;
                        if (value2 != null)
                        {
                            float charge2 = value2.charge;
                            float capacity2 = value2.capacity;
                            if (charge2 < capacity2)
                            {
                                float num6 = num5;
                                float num7 = capacity2 - charge2;
                                if (num6 > num7)
                                {
                                    num6 = num7;
                                }
                                value2.charge += num6;
                                SlotDefinition definition;
                                if (slots.TryGetValue(key, out definition))
                                {
                                    UpdateVisuals(definition, value2.charge / value2.capacity);
                                }
                            }
                        }
                    }
                }
            }
            if (num == 0 || !flag)
            {
                nextChargeAttemptTimer = 5f;
            }
            ToggleUIPowered(num == 0 || flag);
        }
        if (nextChargeAttemptTimer >= 0f)
        {
            int num8 = Mathf.CeilToInt(nextChargeAttemptTimer);
            string text = null;
            if (!unpoweredNotifyStrings.TryGetValue(num8, out text))
            {
                text = Language.main.GetFormat("ChargerInsufficientPower", num8);
                unpoweredNotifyStrings.Add(num8, text);
            }
            uiUnpoweredText.text = text;
        }
        ToggleChargeSound(charging);
    }

    internal float GetConsumedPower()
    {
        return _consumedPower;
    }

    private void RefreshSlots()
    {
        equipment.Clear();
        batteries.Clear();
        slots.Clear();
        slotDefinitions.Clear();

        equipment.SetLabel(labelStorage);

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
            SlotDefinition slotDefinition = slotDefinitions[i];
            string id = slotDefinition.id;
            if (!string.IsNullOrEmpty(id) && !batteries.ContainsKey(id))
            {
                batteries[id] = null;
                slots[id] = slotDefinition;
                Image bar = slotDefinition.bar;
                if (bar != null)
                {
                    bar.material = new Material(bar.material);
                }
            }

            i++;
        }

        UnlockDefaultEquipmentSlots();
        UpdateVisuals();
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
        serializedSlots = equipment.SaveEquipment();
        return serializedSlots;
    }

    public void Load(Dictionary<string, string> savedData)
    {
        QuickLogger.Debug($"loading powercells {savedData.Count}", true);
        serializedSlots = savedData;
        Start();
    }

    internal PowerChargerMode ToggleMode()
    {
        var newMode = _powerChargerMode == PowerChargerMode.Powercell ? PowerChargerMode.Battery : PowerChargerMode.Powercell;
        SetMode(newMode);
        return newMode;
    }
}
