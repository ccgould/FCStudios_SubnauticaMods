using System.Collections.Generic;
using FCS_AlterraHub.Helpers;
using FCS_EnergySolutions.Mods.PowerStorage.Enums;
using FCSCommon.Utilities;
using FMOD;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.UniversalCharger.Mono
{
    internal class PowercellCharger : Charger
    {
        private UniversalChargerController _mono;
        private GameObject[] _uiBatteries;
        private PowerChargerMode _powerChargerMode;
        private StorageContainer _storageContainer;

        public override HashSet<TechType> allowedTech => _powerChargerMode == PowerChargerMode.Powercell ? TechDataHelpers.PowercellTech : TechDataHelpers.BatteryTech;
        public override string labelInteract => _powerChargerMode == PowerChargerMode.Powercell  ? "PowerCellChargerInteract" : "BatteryChargerInteract";
        public override string labelStorage => _powerChargerMode == PowerChargerMode.Powercell ? "PowerCellChargerLabel" : "BatteryChargerStorageLabel";
        public override string labelIncompatibleItem => _powerChargerMode == PowerChargerMode.Powercell ? "PowerCellChargerIncompatibleItem" : "BatteryChargerIncompatibleItem";
        public override string labelCantDeconstruct => _powerChargerMode == PowerChargerMode.Powercell ? "PowerCellChargerCantDeconstruct" : "BatteryChargerCantDeconstruct";

        public override bool Initialize()
        {
            var genericHandTarget = gameObject.EnsureComponent<GenericHandTarget>();
            genericHandTarget.onHandClick = new HandTargetEvent();
            genericHandTarget.onHandHover = new HandTargetEvent();
            genericHandTarget.onHandClick.AddListener(OnHandClick);
            genericHandTarget.onHandHover.AddListener(OnHandHover);

            _storageContainer = _mono.gameObject.GetComponent<StorageContainer>();

            this.equipmentRoot = _mono.gameObject.FindChild("EquipmentRoot").GetComponent<ChildObjectIdentifier>();

            this.animator = gameObject.GetComponent<Animator>();
            this.ui = _mono.GetUI();
            this.uiPowered = GameObjectHelpers.FindGameObject(_mono.gameObject, "Powered");
            this.uiUnpowered = GameObjectHelpers.FindGameObject(_mono.gameObject, "UnPowered");
            this.uiUnpoweredText = uiUnpowered.GetComponentInChildren<Text>();
            
            if (this.slotDefinitions == null)
            {
                slotDefinitions = new();
            }

            if (this.equipment == null)
            {
                this.equipment = new Equipment(base.gameObject, this.equipmentRoot.transform);
                
                this.equipment.isAllowedToAdd = new IsAllowedToAdd(this.IsAllowedToAdd);
                this.equipment.onEquip += this.OnEquip;
                this.equipment.onUnequip += this.OnUnequip;
                this.batteries = new Dictionary<string, IBattery>();
                this.slots = new Dictionary<string, Charger.SlotDefinition>();

                RefreshSlots();
                return true;
            }
            return false;
		}

        private void RefreshSlots()
        {
            equipment.Clear();
            batteries.Clear();
            slots.Clear();

            this.equipment.SetLabel(this.labelStorage);

            for (var index = 0; index < _uiBatteries.Length; index++)
            {
                var slotName = _powerChargerMode == PowerChargerMode.Powercell ? $"UVPowerCellCharger{index + 1}" : $"UCBatteryCharger{index + 1}";

                GameObject battery = _uiBatteries[index];
                slotDefinitions.Add(new SlotDefinition
                {
                    id = slotName,
                    bar = _uiBatteries[index].FindChild("BatteryFill").GetComponent<Image>(),
                    text = _uiBatteries[index].GetComponentInChildren<Text>()
                });
            }

            int i = 0;
            int count = this.slotDefinitions.Count;
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
        }

        internal bool HasRechargeables()
        {
            return equipmentRoot.transform.childCount > 0;
        }

        internal PowerChargerMode GetMode()
        {
            return _powerChargerMode;
        }

        internal void Initialize(UniversalChargerController mono, GameObject[] uiBatteries)
        {
            _mono = mono;
            _uiBatteries = uiBatteries;
        }

        public Dictionary<string, string> Save()
        {
            this.serializedSlots = equipment.SaveEquipment();
            return equipment.SaveEquipment();
        }

        public void Load(Dictionary<string, string> savedData)
        {
            QuickLogger.Debug($"loading powercells {savedData.Count}",true);
            this.serializedSlots = savedData;
            Start();
        }
    }
}
