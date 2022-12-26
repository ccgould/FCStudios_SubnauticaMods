using System.Collections.Generic;
using FCS_AlterraHub.Helpers;
using UnityEngine;
using UnityEngine.UI;
#if BELOWZERO
using Text = TMPro.TextMeshProUGUI;
#endif

namespace FCS_EnergySolutions.Mods.UniversalCharger.Mono
{
    internal class BatteryCharger : Charger
    {
        private UniversalChargerController _mono;
        private GameObject[] _uiBatteries;

        public override HashSet<TechType> allowedTech => TechDataHelpers.BatteryTech;

        public override string labelInteract => "BatteryChargerInteract";
        public override string labelStorage => "BatteryChargerStorageLabel";
        public override string labelIncompatibleItem => "BatteryChargerIncompatibleItem";
        public override string labelCantDeconstruct => "BatteryChargerCantDeconstruct";


        public override bool Initialize()
        {
            var genericHandTarget = gameObject.EnsureComponent<GenericHandTarget>();
            genericHandTarget.onHandClick = new HandTargetEvent();
            genericHandTarget.onHandHover = new HandTargetEvent();
            genericHandTarget.onHandClick.AddListener(OnHandClick);
            genericHandTarget.onHandHover.AddListener(OnHandHover);
            
            this.equipmentRoot = _mono.gameObject.FindChild("BEquipmentRoot").GetComponent<ChildObjectIdentifier>();
            this.animator = gameObject.GetComponent<Animator>();
            this.ui = _mono.GetUI();
            this.uiPowered = GameObjectHelpers.FindGameObject(_mono.gameObject, "Powered");
            this.uiUnpowered = GameObjectHelpers.FindGameObject(_mono.gameObject, "UnPowered");
            //TODO V2 Fix
            //this.uiUnpoweredText = uiUnpowered.GetComponentInChildren<Text>();

            if (this.slotDefinitions == null)
            {
                slotDefinitions = new();
            }
            if (this.equipment == null)
            {
                this.equipment = new Equipment(base.gameObject, this.equipmentRoot.transform);
                this.equipment.SetLabel(this.labelStorage);
                this.equipment.isAllowedToAdd = new IsAllowedToAdd(this.IsAllowedToAdd);
                this.equipment.onEquip += this.OnEquip;
                this.equipment.onUnequip += this.OnUnequip;
                this.batteries = new Dictionary<string, IBattery>();
                this.slots = new Dictionary<string, Charger.SlotDefinition>();

                for (var index = 0; index < _uiBatteries.Length; index++)
                {
                    var slotName = $"UCBatteryCharger{index + 1}";

                    GameObject battery = _uiBatteries[index];
                    slotDefinitions.Add(new SlotDefinition
                    {
                        id = slotName,
                        bar = _uiBatteries[index].FindChild("BatteryFill").GetComponent<Image>(),
                        //TODO V2 Fix
                        //text = _uiBatteries[index].GetComponentInChildren<Text>()
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
                return true;
            }
            return false;
        }

        public Dictionary<string, string> Save()
        {
            this.serializedSlots = equipment.SaveEquipment();
            return equipment.SaveEquipment();
        }

        public void Load(Dictionary<string, string> savedData)
        {
            this.serializedSlots = savedData;
            Start();
        }

        internal void Initialize(UniversalChargerController mono, GameObject[] uiBatteries)
        {
            _mono = mono;
            _uiBatteries = uiBatteries;
        }
    }
}
