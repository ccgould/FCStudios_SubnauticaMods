
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Model
{
    public class FCSFridge : FCSStorage
    {
        //Need a way to prevent pulling of rotten items

        private bool _decay;
        private FCSGameMode _modMode;
        private FcsDevice _mono;
        private EatableType _targetEatableType = EatableType.Any;
        public int NumberOfItems => FridgeItems.Count;
        
        private void AttemptDecay()
        {
            if (_decay && _modMode == FCSGameMode.HardCore)
            {
                foreach (EatableEntities eatableEntities in FridgeItems)
                {
                    eatableEntities.IterateRotten();
                }
            }
        }

        public List<EatableEntities> FridgeItems { get; } = new();
        
        private EatableEntities FindMatch(TechType techType, EatableType eatableType)
        {
            switch (eatableType)
            {
                case EatableType.Any:
                    return FridgeItems.FirstOrDefault(x => x.TechType == techType);
                case EatableType.Rotten:
                    return FridgeItems.FirstOrDefault(x => (x.GetFoodValue() < 0 && x.GetWaterValue() < 0) && x.TechType == techType);
                default:
                    return FridgeItems.FirstOrDefault(x => (x.GetFoodValue() > 0 || x.GetWaterValue() > 0) && x.TechType == techType);
            }
        }

        public void Initialize(FcsDevice mono)
        {
            _mono = mono;
            float time = UnityEngine.Random.Range(0f, 5f);
            ItemsContainer.onRemoveItem += ContainerOnRemoveItem;
            InvokeRepeating(nameof(AttemptDecay), time, 5f);
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable)
        {
            bool flag = false;

            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();

                QuickLogger.Debug(techType.ToString());

                if (pickupable.GetComponent<Eatable>() != null)
                    flag = true;
            }

            return flag;
        }

        public void CreateEntityAndAddItem(InventoryItem item,bool fromSave = false, float timeDecayPause = 0)
        {
            if (IsFull)
            {
                return;
            }
            
            var eatableEntity = new EatableEntities();
            eatableEntity.Initialize(item.item,false);
            eatableEntity.PauseDecay();
            if (fromSave)
            {
                eatableEntity.TimeDecayPause = timeDecayPause;
            }
            else
            {
                base.AddItemToContainer(item);
            }
            FridgeItems.Add(eatableEntity);
            OnContainerUpdate?.Invoke(NumberOfItems, SlotsAssigned);
            OnContainerAddItem?.Invoke(_mono,item.item.GetTechType());
            //Destroy(item.item.gameObject);
        }
        
        public List<EatableEntities> Save()
        {
            return FridgeItems;
        }

        public IEnumerator LoadSave(List<EatableEntities> save)
        {
            if (save == null) yield break;

            foreach (EatableEntities eatableEntities in save)
            {
                QuickLogger.Debug($"Adding entity {eatableEntities.Name}");

                var prefabForTechType = CraftData.GetPrefabForTechTypeAsync(eatableEntities.TechType, false);
                yield return prefabForTechType;

                var food = GameObject.Instantiate(prefabForTechType.GetResult());

                var eatable = food.gameObject.GetComponent<Eatable>();
                eatable.timeDecayStart = eatableEntities.TimeDecayStart;

#if SUBNAUTICA
                var item = new InventoryItem(food.gameObject.GetComponent<Pickupable>());
                //if you get items on load this is why you removed pickup false.
#elif BELOWZERO
                Pickupable pickupable = food.gameObject.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                var item = new InventoryItem(pickupable);
#endif
                CreateEntityAndAddItem(item,true,eatableEntities.TimeDecayPause);
                QuickLogger.Debug($"Load Item {item.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}");
            }
        }

        public void SetDecay(bool value)
        {
            _decay = value;
            RefreshDecayState();
        }

        private void RefreshDecayState()
        {
            if (_decay && _modMode == FCSGameMode.HardCore)
            {
                foreach (EatableEntities fridgeItem in FridgeItems)
                {
                    fridgeItem.UnpauseDecay();
                }
            }
            else
            {
                foreach (EatableEntities fridgeItem in FridgeItems)
                {
                    fridgeItem.PauseDecay();
                }
            }
        }

        public void SetModMode(FCSGameMode modMode)
        {
            _modMode = modMode;
            RefreshDecayState();
        }
        
        public override bool AddItemToContainer(InventoryItem item)
        {
            if (item == null) return false;

            if (!IsFull)
            {
                if (!IsAllowedToAdd(item.item))
                {
                    return false;
                }

                CreateEntityAndAddItem(item);
            }
            else
            {
                return false;
            }

            return true;
        }

        public override bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            //if (!base.IsAllowedToAdd(pickupable, verbose)) return false;

            bool flag = false;

            if (IsFull)
            {
                QuickLogger.Message(LanguageHelpers.GetLanguage("InventoryFull"), true);
                return false;
            }

            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();

                QuickLogger.Debug(techType.ToString());

                if (pickupable.GetComponent<Eatable>() != null)
                    flag = true;
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
            {
                QuickLogger.Message(LanguageHelpers.GetLanguage("TimeCapsuleItemNotAllowed"), true);
                flag = false;
            }

            return flag;
        }
        
        public override  Pickupable RemoveItemFromContainer(TechType techType)
        {

            QuickLogger.Debug($"RemoveItemFromContainer Called {techType.AsString()}");

            EatableEntities match = FindMatch(techType, _targetEatableType);

            if (match == null) return null;
            _targetEatableType = EatableType.Any;
            return base.RemoveItemFromContainer(techType);
        }

        public void RemoveItem(TechType techType, EatableType eatableType)
        {
            QuickLogger.Debug($"RemoveItem Called {techType.AsString()}");

#if SUBNAUTICA
            var slotSize = CraftData.GetItemSize(techType);
#else
            var slotSize = TechData.GetItemSize(techType);
#endif



            if (Inventory.main.HasRoomFor(slotSize.x, slotSize.y))
            {
                EatableEntities match = FindMatch(techType, eatableType);

                if (match != null)
                {
                    Inventory.main.Pickup(RemoveItemFromContainer(techType));
                }
                else
                {
                    QuickLogger.Message(LanguageHelpers.GetLanguage("InventoryFull"), true);
                }
            }
            
        }

        private void ContainerOnRemoveItem(InventoryItem item)
        {
            QuickLogger.Debug($"ContainerOnRemoveItem Called {item.item.GetTechName()}");
            var techType = item.item.GetTechType();
            var pickup = item.item;

            QuickLogger.Debug($"Removing {techType} from SeaBreeze", true);
            EatableEntities match = FindMatch(techType, EatableType.Any);

            if (match != null)
            {

                var eatable = pickup.GetComponent<Eatable>();
                
                match.UnpauseDecay();
                eatable.timeDecayStart = match.TimeDecayStart;
                FridgeItems.Remove(match);
                OnContainerUpdate?.Invoke(NumberOfItems, SlotsAssigned);
                OnContainerRemoveItem?.Invoke(_mono, techType);
            }
        }


        public void Clear()
        {
            FridgeItems.Clear();
            OnContainerUpdate?.Invoke(NumberOfItems, SlotsAssigned);
        }
    }
}