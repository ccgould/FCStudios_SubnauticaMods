using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.PowerStorage.Structs;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.PowerStorage.Mono
{
    internal class PowerCellCharger : MonoBehaviour
    {
        protected float NextChargeAttemptTimer;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        protected const float ChargeAttemptInterval = 5f;
        public float ChargeSpeed = 0.005f;
        protected Dictionary<string, IBattery> Batteries;
        protected Dictionary<string, SlotDefinition> Slots;
        private PowerSupply _powerSupply;
        private PowerStorageController _mono;
        private bool _allowedToCharge;
        private FCSStorage _storageContainer;
        private bool _bypassRemoveEvent;
        private const int MAXSLOTS = 10;
        public bool IsFull => _storageContainer?.GetCount() >= MAXSLOTS;

        private void Start()
        {
            _storageContainer.CleanUpDuplicatedStorageNoneRoutine();
        }

        public Action<string> OnBatteryAdded { get; set; }

        internal void Initialize(PowerStorageController mono, GameObject[] powercellDummies, GameObject[] uiBatteries, PowerSupply powerSupply)
        {
            _mono = mono;
            Batteries = new Dictionary<string, IBattery>();
            Slots = new Dictionary<string, SlotDefinition>();
            _powerSupply = powerSupply;
            var j = 1;
            for (var index = 0; index < powercellDummies.Length; index++)
            {
                GameObject dummy = powercellDummies[index];
                if (dummy.name.StartsWith("Battery_"))
                {
                    var slot = $"Slot{j++}";
                    var controller = dummy.AddComponent<BatteryDummyController>();
                    Slots.Add(slot, new SlotDefinition
                    {
                        id = slot,
                        battery = controller,
                        bar = uiBatteries[index].FindChild("BatteryFill").GetComponent<Image>(),
                        text = uiBatteries[index].FindChild("Text").GetComponent<Text>(),
                    });
                    controller.Initialize(slot, this);
                }
            }

            var i = 0;
            var count = Slots.Count;

            while (i < count)
            {
                var slotDefinition = Slots.ElementAt(i).Value;
                var id = slotDefinition.id;

                if (!string.IsNullOrEmpty(id) && !Batteries.ContainsKey(id))
                {
                    Batteries[id] = null;
                    Slots[id] = slotDefinition;
                    Image bar = slotDefinition.bar;
                    if (bar != null)
                    {
                        bar.material = new Material(bar.material);
                    }
                }
                i++;
            }

            if (_storageContainer == null)
            {
                _storageContainer = gameObject.GetComponent<FCSStorage>();
                _storageContainer.SlotsAssigned = 10;
                _storageContainer.container.onAddItem += AddPowercell;
                _storageContainer.container.onRemoveItem += OnUnEquip;
                _storageContainer.container.allowedTech = TechDataHelpers.PowercellTech;
                _storageContainer.Deactivate();
                var handTargetController = GameObjectHelpers.FindGameObject(gameObject, "HandTarget")?.AddComponent<PowerStorageHandTargetController>();
                if(handTargetController != null) handTargetController.storage = _storageContainer;
            }

            AddPowerSource(_powerSupply);
        }

        private void AddPowercell(InventoryItem item)
        {
            var slot = FindAvailableSlot();

            if (!string.IsNullOrWhiteSpace(slot.id))
            {
                NextChargeAttemptTimer = 0f;
                IBattery component = item.item?.GetComponent<IBattery>();
                if (component != null && Batteries.ContainsKey(slot.id))
                {
                    Batteries[slot.id] = component;
                    slot.battery.InventoryItem = item;
                    OnBatteryAdded?.Invoke(slot.id);
                }

                if (Slots.TryGetValue(slot.id, out var slotDefinition))
                {
                    if (slotDefinition.battery != null)
                    {
                        slotDefinition.battery.IsVisible(true);
                    }

                    if (component != null)
                    {
                        UpdateVisuals(slotDefinition, component.charge / component.capacity);
                    }
                }
            }
        }

        internal float GetTotal()
        {
            return Batteries.Sum(x => x.Value?.charge ?? 0);
        }

        internal float GetCapacity()
        {
            return Batteries.Sum(x => x.Value?.capacity ?? 0);
        }

        internal int GetPowerCellCount()
        {
            return Slots.Count(x => x.Value.IsOccupied());
        }

        private void Update()
        { 
            PowerRelay powerRelay = PowerSource.FindRelay(transform);
            
            if (DayNightCycle.main.deltaTime == 0f || !_allowedToCharge)
            {
                return;
            }
            if (NextChargeAttemptTimer > 0f)
            {
                NextChargeAttemptTimer -= DayNightCycle.main.deltaTime;
                if (NextChargeAttemptTimer < 0f)
                {
                    NextChargeAttemptTimer = 0f;
                }
            }

            if (NextChargeAttemptTimer <= 0f)
            {
                int num = 0;
                bool flag = false;

                if (powerRelay != null)
                {
                    float num2 = 0f;
                    foreach (KeyValuePair<string, IBattery> keyValuePair in Batteries)
                    {
                        IBattery value = keyValuePair.Value;
                        if (value != null)
                        {
                            float charge = value.charge;
                            float capacity = value.capacity;
                            if (charge < capacity)
                            {
                                num++;
                                float num3 = DayNightCycle.main.deltaTime * ChargeSpeed * capacity;
                                if (charge + num3 > capacity)
                                {
                                    num3 = capacity - charge;
                                }
                                num2 += num3;
                            }
                        }
                    }
                    float num4 = 0f;

                    if (num2 > 0f && _mono.CalculateBasePower() > num2)
                    {
                        flag = true;
                        foreach (IPowerInterface powerSource in powerRelay.inboundPowerSources)
                        {
                            if (powerSource is PowerSupply)
                            {
                                continue;
                            }

                            powerSource.ConsumeEnergy(num2, out num4);
                            num2 -= num4;
                            if (num2 <= 0) break;
                        }
                    }
                    if (num4 > 0f)
                    {
                        float num5 = num4 / num;

                        foreach (KeyValuePair<string, IBattery> keyValuePair2 in Batteries)
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
                                    if (Slots.TryGetValue(key, out var definition))
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
                    NextChargeAttemptTimer = 5f;
                }
            }
        }

        protected void UpdateVisuals()
        {
            foreach (KeyValuePair<string, SlotDefinition> keyValuePair in Slots)
            {
                string key = keyValuePair.Key;
                InventoryItem itemInSlot = GetItemInSlot(key);
                SlotDefinition value = keyValuePair.Value;
                float n = -1f;
                if (itemInSlot != null)
                {
                    Pickupable item = itemInSlot.item;
                    if (item != null)
                    {
                        IBattery component = item.GetComponent<IBattery>();
                        if (component != null)
                        {
                            n = component.charge / component.capacity;
                        }
                    }
                }
                this.UpdateVisuals(value, n);
            }
        }

        private InventoryItem GetItemInSlot(string key)
        {
            if (Slots.ContainsKey(key))
            {
                return Slots[key].battery.InventoryItem;
            }

            return null;
        }

        protected void UpdateVisuals(SlotDefinition definition, float n)
        {
            if (definition.battery != null)
            {
                definition.battery.IsVisible(n >= 0f);
            }

            //ProfilingUtils.BeginSample("ChargerUpdateText");
            Text text = definition.text;
            if (text != null)
            {
                text.text = ((n >= 0f) ? $"{n:P0}" : Language.main.Get("ChargerSlotEmpty"));
            }

            var totalPerc = GetTotal() / GetCapacity();
            if (float.IsNaN(totalPerc))
            {
                _mono.PowerTotalMeterTotal.text = _powerSupply.GetPowerString();
                _mono.PowerTotalMeterPercent.text = "0%";
                _mono.PowerTotalMeterRing.fillAmount = 0f;
            }
            else
            {
                _mono.PowerTotalMeterTotal.text = _powerSupply.GetPowerString();
                _mono.PowerTotalMeterPercent.text = $"{totalPerc:P0}";
                _mono.PowerTotalMeterRing.fillAmount = totalPerc;
            }


            //ProfilingUtils.EndSample(null);
            Image bar = definition.bar;
            if (bar != null)
            {
                if (n >= 0f)
                {
                    Color value = (n < 0.5f) ? Color.Lerp(_colorEmpty, _colorHalf, 2f * n) : Color.Lerp(_colorHalf, _colorFull, 2f * n - 1f);
                    bar.color = value;
                    bar.fillAmount = n;
                    return;
                }
                bar.color = _colorEmpty;
                bar.fillAmount = 0f;
            }
        }

        internal void OnUnEquip(string slot, BatteryDummyController item)
        {
            QuickLogger.Debug("OnUnEquip W Dummy");
            if (Batteries.ContainsKey(slot))
            {
                PlayerInteractionHelper.GivePlayerItem(GetItemInSlot(slot));
                Batteries[slot] = null;
            }

            if (Slots.TryGetValue(slot, out var definition))
            {
                UpdateVisuals(definition, -1f);
            }
        }

        private void OnUnEquip(InventoryItem item)
        {
            QuickLogger.Debug("OnUnEquip");
            if (FindSlotWithBattery(item, out var slot))
            {
                Batteries[slot.id] = null;
                if (Slots.TryGetValue(slot.id, out var definition))
                {
                    UpdateVisuals(definition, -1f);
                }
            }
        }

        private bool FindSlotWithBattery(InventoryItem item, out SlotDefinition slot)
        {
            foreach (KeyValuePair<string, SlotDefinition> slotDefinition in Slots)
            {
                if (slotDefinition.Value.battery.InventoryItem != item) continue;
                slot = slotDefinition.Value;
                return true;
            }

            slot = new SlotDefinition();
            return false;
        }

        private SlotDefinition FindAvailableSlot()
        {
            return Slots.FirstOrDefault(x => !x.Value.IsOccupied()).Value;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;

            if (IsFull) return flag;

            if (string.IsNullOrWhiteSpace(FindAvailableSlot().id))
            {
                QuickLogger.ModMessage(AuxPatchers.NoSlotsAvailable());
                return flag;
            }

            var techType = pickupable.GetTechType();
#if SUBNAUTICA
            var equipType = CraftData.GetEquipmentType(techType);
#elif BELOWZERO
            var equipType = TechData.GetEquipmentType(techType);
#endif
            QuickLogger.Debug($"Equipment Slot: {equipType}", true);

            if (equipType == EquipmentType.PowerCellCharger || BatteryInfoHelpers.IsPowercell(techType))
            {
                flag = true;
            }
            else
            {
                QuickLogger.ModMessage(AuxPatchers.OnlyPowercellsAllowed());
            }

            return flag;
        }

        public void RemoveCharge(float consumed)
        {
            //QuickLogger.Debug($"Removing: {consumed}",true);
            foreach (KeyValuePair<string, IBattery> iBattery in Batteries)
            {
                if (iBattery.Value == null) continue;

                if (iBattery.Value.charge >= Mathf.Abs(consumed))
                {
                    var battery = iBattery.Value;

                    if (consumed >= 0f)
                    {
                        battery.charge += Mathf.Min(consumed, battery.capacity - battery.charge);
                    }
                    else
                    {
                        if (GameModeUtils.RequiresPower())
                        {
                            battery.charge += -Mathf.Min(-consumed, battery.charge);
                            UpdateVisuals();
                        }
                    }
                    break;
                }

            }
        }

        public void AddPowerSource(PowerSupply powerSupply)
        {
            PowerRelay powerRelay = PowerSource.FindRelay(transform);
            if (powerRelay != null)
            {
                powerRelay.AddInboundPower(powerSupply);
            }
        }

        public void RemovePowerSource(PowerSupply powerSupply)
        {
            PowerRelay powerRelay = PowerSource.FindRelay(transform);
            if (powerRelay != null)
            {
                powerRelay.RemoveInboundPower(powerRelay);
                QuickLogger.Debug("Removing inbound power", true);
            }
        }

        public void SetAllowedToCharge(bool value)
        {
            _allowedToCharge = value;
        }
        
        public bool HasPowerCells()
        {
            if (Batteries == null) return false;
            return Batteries.Any(x => x.Value != null);
        }

        public void LoadFromSave()
        {
            foreach (InventoryItem inventoryItem in _storageContainer.ItemsContainer)
            {
                AddPowercell(inventoryItem);
            }
        }
    }
}
