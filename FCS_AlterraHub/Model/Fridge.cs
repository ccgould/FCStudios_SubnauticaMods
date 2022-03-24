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
    public class Fridge : MonoBehaviour, IFCSStorage
    { 
        private int _itemLimit;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        private bool _decay;
        private FCSGameMode _modMode;
        private FcsDevice _mono;
        public bool IsFull => NumberOfItems >= _itemLimit;
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

        public List<EatableEntities> FridgeItems { get; private set; } = new List<EatableEntities>();
        
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

        public void Initialize(FcsDevice mono, int itemLimit)
        {
            _mono = mono;
            _itemLimit = itemLimit;
            float time = UnityEngine.Random.Range(0f, 5f);
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

        public void AddItem(InventoryItem item,bool fromSave = false, float timeDecayPause = 0)
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
            FridgeItems.Add(eatableEntity);
            OnContainerUpdate?.Invoke(NumberOfItems, _itemLimit);
            OnContainerAddItem?.Invoke(_mono,item.item.GetTechType());
            Destroy(item.item.gameObject);
        }
        
        public bool IsEmpty()
        {
            return NumberOfItems <= 0;
        }

        public int GetAmount(TechType techType)
        {
            QuickLogger.Debug($"Getting amount for: {techType} || Fridge amount: {FridgeItems.Count}");
            return FridgeItems.Count(x => x.TechType == techType);
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
                var item = new InventoryItem(food.gameObject.GetComponent<Pickupable>());//if you get items on load this is why you removed pickup false
#elif BELOWZERO
                Pickupable pickupable = food.gameObject.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                var item = new InventoryItem(pickupable);
#endif

                AddItem(item,true,eatableEntities.TimeDecayPause);

                QuickLogger.Debug(
                    $"Load Item {item.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}");
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

        public int GetContainerFreeSpace => _itemLimit - NumberOfItems;
        public bool CanBeStored(int amount, TechType techType = TechType.None)
        {
            return amount < GetContainerFreeSpace;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            if (item == null) return false;

            if (!IsFull)
            {
                if (!IsAllowedToAdd(item.item))
                {
                    return false;
                }

                AddItem(item);
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
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

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType)
        {
            //TODO Update to the new EXP System
            //EatableEntities match = FindMatch(techType, EatableType.Any);

            //if (match == null) return null;

            //var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(techType));
            //var eatable = go.GetComponent<Eatable>();
            //var pickup = go.GetComponent<Pickupable>();

            //match.UnpauseDecay();
            //eatable.timeDecayStart = match.TimeDecayStart;
            //FridgeItems.Remove(match);
            //OnContainerUpdate?.Invoke(NumberOfItems, _itemLimit);

            //return pickup;
            return null;
        }


#if SUBNAUTICA_STABLE
        public void RemoveItem(TechType techType, EatableType eatableType)
        {

            var pickupable = techType.ToPickupable();

            if (pickupable == null)
            {
                QuickLogger.Error("Failed to convert food item to pickupable");
                return;
            }


            if (Inventory.main.HasRoomFor(pickupable))
            {
                EatableEntities match = FindMatch(techType, eatableType);

                if (match != null)
                {
                    var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(techType));
                    var eatable = go.GetComponent<Eatable>();
                    var pickup = go.GetComponent<Pickupable>();

                    match.UnpauseDecay();
                    eatable.timeDecayStart = match.TimeDecayStart;

                    if (Inventory.main.Pickup(pickup))
                    {
                        QuickLogger.Debug($"Removed Match Before || Fridge Count {FridgeItems.Count}");
                        FridgeItems.Remove(match);
                        QuickLogger.Debug($"Removed Match || Fridge Count {FridgeItems.Count}");
                    }
                    else
                    {
                        QuickLogger.Message(LanguageHelpers.GetLanguage("InventoryFull"), true);
                    }
                    GameObject.Destroy(pickupable);
                    OnContainerUpdate?.Invoke(NumberOfItems, _itemLimit);
                    OnContainerRemoveItem?.Invoke(_mono, techType);
                }
            }
            else
            {
                Destroy(pickupable);
            }
        }
#else

        public void RemoveItem(TechType techType, EatableType eatableType)
        {

#if SUBNAUTICA
            var pickupable = techType.ToPickupable();
#else
            var pickupable = TechTypeHelpers.ConvertToPickupable(techType,(pickupable =>
            {
                if (Inventory.main.HasRoomFor(pickupable))
                {
                    EatableEntities match = FindMatch(techType, eatableType);

                    if (match != null)
                    {
                        StartCoroutine(AttemptToRemoveAsync(techType, match, pickupable));
                    }
                }
                else
                {
                    Destroy(pickupable);
                }
            }));
#endif
        }

        private IEnumerator AttemptToRemoveAsync(TechType techType, EatableEntities match, Pickupable pickupable)
        {
            var prefabForTechType = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return prefabForTechType;

            var prefabResult = prefabForTechType.GetResult();

            var go = GameObject.Instantiate(prefabResult);
            var eatable = go.GetComponent<Eatable>();
            var pickup = go.GetComponent<Pickupable>();

            match.UnpauseDecay();
            eatable.timeDecayStart = match.TimeDecayStart;

            TaskResult<bool> pickupResult = new TaskResult<bool>();
#if SUBNAUTICA
            yield return Inventory.main.PickupAsync(pickup, pickupResult);
#else
            yield return Inventory.main.Pickup(pickup);
#endif

            if (pickupResult.Get())
            {
                QuickLogger.Debug($"Removed Match Before || Fridge Count {FridgeItems.Count}");
                FridgeItems.Remove(match);
                QuickLogger.Debug($"Removed Match || Fridge Count {FridgeItems.Count}");
            }
            else
            {
                QuickLogger.Message(LanguageHelpers.GetLanguage("InventoryFull"), true);
            }

            Destroy(pickupable);
            OnContainerUpdate?.Invoke(NumberOfItems, _itemLimit);
            OnContainerRemoveItem?.Invoke(_mono, techType);
            yield break;
        }
#endif


        public Dictionary<TechType, int> GetItemsWithin()
        {
            var lookup = FridgeItems?.Where(x => x != null).ToLookup(x => x.TechType).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => count.Count());
        }

        public bool ContainsItem(TechType techType)
        {
            return FridgeItems.Any(x => x.TechType == techType);
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {
            return NumberOfItems;
        }

        public void Clear()
        {
            FridgeItems.Clear();
            OnContainerUpdate?.Invoke(NumberOfItems, _itemLimit);
        }
    }
}