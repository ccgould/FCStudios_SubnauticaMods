using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class PowercellSlot : MonoBehaviour ,IHandTarget, IItemSelectorManager
    {
        private AlterraFabricatorStationController _mono;
        private StorageSlot batterySlot;
        private string _slotID;
        private GameObject _batteryVisual;
        private bool _allowBatteryReplacement = true;
        private GeneratorController _generatorController;
        private HashSet<TechType> compatibleBatteries => TechDataHelpers.PowercellTech;

        internal bool AllowBatteryReplacement
        {
            get => _allowBatteryReplacement;
            set
            {
                _allowBatteryReplacement = value;
                if (_batteryVisual == null)
                {
                    _batteryVisual = gameObject.transform.GetChild(0).gameObject;
                }
                _batteryVisual?.gameObject.SetActive(!value);
            }
        }

        internal string GetID()
        {
            return _slotID;
        }

        internal void Initialize(GeneratorController generatorController, AlterraFabricatorStationController mono, string slotID)
        {
            _slotID = slotID;
            _mono = mono;
            _generatorController = generatorController;
            
            if (batterySlot != null)
            {
                return;
            }

            batterySlot = new StorageSlot(transform, string.Empty);
            batterySlot.onAddItem += OnAddItem;
            batterySlot.onRemoveItem += OnRemoveItem;
        }

        protected void OnAddItem(InventoryItem item)
        {
            AllowBatteryReplacement = false;
            _generatorController.UpdateScreen();
            Destroy(item.item.gameObject);
        }

        protected void OnRemoveItem(InventoryItem item)
        {

        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            if (!AllowBatteryReplacement)
            {
#if SUBNAUTICA
                main.SetInteractText(AlterraHub.CannotRemovePowercell());
#else
                main.SetText(HandReticle.TextType.Hand,AlterraHub.CannotRemovePowercell(),false);
#endif
                main.SetIcon(HandReticle.IconType.HandDeny);
                return;
            }


            string text = "";
            InventoryItem storedItem = batterySlot.storedItem;
            string text2;
            if (storedItem != null)
            {
                Pickupable item = storedItem.item;
                text2 = Language.main.Get(item.GetTechName());
                IBattery component = item.GetComponent<IBattery>();
                if (component != null)
                {
                    text = component.GetChargeValueText();
                }
            }
            else
            {
                text2 = Language.main.Get("PowerSourceUnloaded");
            }
#if SUBNAUTICA
            main.SetInteractText(text2, text, false, false, true);
#else
            main.SetText(HandReticle.TextType.Hand,$"{Language.main.Get(text2)}\n{Language.main.Get(text)}", false);
#endif
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!AllowBatteryReplacement)
            {

                return;
            }

            uGUI.main.itemSelector.Initialize(this, SpriteManager.Get(SpriteManager.Group.Item, "nobattery"), new List<IItemsContainer>
            {
                Inventory.main.container,
                batterySlot
            });
        }

        public bool Filter(InventoryItem item)
        {
            return compatibleBatteries.Contains(item.item.GetTechType());
        }

        public int Sort(List<InventoryItem> items)
        {
            InventoryItem storedItem = batterySlot.storedItem;
            bool flag = storedItem != null && items.Remove(storedItem);
            EnergyMixin.sGroups.Clear();
            int i = 0;
            int count = items.Count;
            while (i < count)
            {
                InventoryItem inventoryItem = items[i];
                TechType techType = inventoryItem.item.GetTechType();
                List<InventoryItem> list;
                if (!EnergyMixin.sGroups.TryGetValue(techType, out list))
                {
                    list = new List<InventoryItem>();
                    EnergyMixin.sGroups.Add(techType, list);
                }
                list.Add(inventoryItem);
                i++;
            }
            foreach (KeyValuePair<TechType, List<InventoryItem>> keyValuePair in EnergyMixin.sGroups)
            {
                keyValuePair.Value.Sort(new Comparison<InventoryItem>(CompareByCharge));
            }
            items.Clear();
            if (flag)
            {
                items.Insert(0, storedItem);
            }
            int j = 0;
            int count2 = compatibleBatteries.Count;
            while (j < count2)
            {
                TechType key = compatibleBatteries.ElementAt(j);
                List<InventoryItem> collection;
                if (EnergyMixin.sGroups.TryGetValue(key, out collection))
                {
                    items.AddRange(collection);
                    EnergyMixin.sGroups.Remove(key);
                }
                j++;
            }
            if (EnergyMixin.sGroups.Count > 0)
            {
                foreach (KeyValuePair<TechType, List<InventoryItem>> keyValuePair in EnergyMixin.sGroups)
                {
                    items.AddRange(keyValuePair.Value);
                }
                EnergyMixin.sGroups.Clear();
            }
            if (!flag)
            {
                return -1;
            }
            return 0;
        }

        protected int CompareByCharge(InventoryItem item1, InventoryItem item2)
        {
            IBattery component = item1.item.GetComponent<IBattery>();
            IBattery component2 = item2.item.GetComponent<IBattery>();
            if (component != null && component2 != null)
            {
                float charge = component.charge;
                float charge2 = component2.charge;
                return charge.CompareTo(charge2);
            }
            if (component == null && component2 == null)
            {
                return 0;
            }
            if (component2 == null)
            {
                return -1;
            }
            return 1;
        }

        public string GetText(InventoryItem item)
        {
            if (item != null)
            {
                Pickupable item2 = item.item;
                string text = Language.main.Get(item2.GetTechName());
                IBattery component = item2.GetComponent<IBattery>();
                if (component != null)
                {
                    return string.Format("{0}\n{1}", text, component.GetChargeValueText());
                }
                return text;
            }
            else
            {
                InventoryItem storedItem = batterySlot.storedItem;
                if (storedItem != null)
                {
                    Pickupable item3 = storedItem.item;
                    string arg = Language.main.Get(item3.GetTechName());
                    return Language.main.GetFormat<string>("PowerSourceUnload", arg);
                }
                return Language.main.Get("PowerSourceUnloaded");
            }
        }

        public void Select(InventoryItem item)
        {
            InventoryItem storedItem = batterySlot.storedItem;
            if (storedItem != null)
            {
                if (item == null)
                {
                    batterySlot.RemoveItem();
                    Inventory.main.ForcePickup(storedItem.item);
                    return;
                }
                if (storedItem != item)
                {
                    batterySlot.RemoveItem();
                    batterySlot.AddItem(item);
                    Inventory.main.ForcePickup(storedItem.item);
                    Pickupable item2 = item.item;
                    if (item2 != null)
                    {
                        uGUI_IconNotifier.main.Play(item2.GetTechType(), uGUI_IconNotifier.AnimationType.To, null);
                        return;
                    }
                }
            }
            else if (item != null)
            {
                batterySlot.AddItem(item);
                Pickupable item3 = item.item;
                if (item3 != null)
                {
                    uGUI_IconNotifier.main.Play(item3.GetTechType(), uGUI_IconNotifier.AnimationType.To, null);
                }
            }
        }
    }
}