using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.TrashRecycler.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Json.ExtensionMethods;
using UnityEngine;


namespace FCS_HomeSolutions.TrashRecycler.Mono
{
    internal class Recycler : MonoBehaviour
    {
        public FcsDevice Controller { get; set; }
        private readonly Queue<Waste> _wasteList = new Queue<Waste>();
        private static readonly List<IIngredient> Ingredients = new List<IIngredient>();
        private static readonly HashSet<TechType> BatteryTech = new HashSet<TechType>
        {
            TechType.Battery,
            TechType.PowerCell,
            TechType.PrecursorIonBattery,
            TechType.PrecursorIonPowerCell,
            TechType.PrecursorIonCrystal
        };
        public static readonly HashSet<TechType> BannedTech = new HashSet<TechType>
        {
            TechType.CookedBladderfish,
            TechType.CookedBoomerang,
            TechType.CookedEyeye,
            TechType.CookedGarryFish,
            TechType.CookedHoleFish,
            TechType.CookedHoopfish,
            TechType.CookedHoverfish,
            TechType.CookedLavaBoomerang,
            TechType.CookedLavaEyeye,
            TechType.CookedOculus,
            TechType.CookedPeeper,
            TechType.CookedReginald,
            TechType.CookedSpadefish,
            TechType.CookedSpinefish,
            TechType.CuredBladderfish,
            TechType.CuredBoomerang,
            TechType.CuredEyeye,
            TechType.CuredGarryFish,
            TechType.CuredHoleFish,
            TechType.CuredHoopfish,
            TechType.CuredHoverfish,
            TechType.CuredLavaBoomerang,
            TechType.CuredLavaEyeye,
            TechType.CuredOculus,
            TechType.CuredPeeper,
            TechType.CuredReginald,
            TechType.CuredSpadefish,
            TechType.CuredSpinefish,
            TechType.AirBladder,
            TechType.FilteredWater,
            TechType.Titanium,
            TechType.Bleach,
            TechType.Lubricant,
            TechType.Silicone
        };


        private FCSStorage _storageContainer;
        private List<string> _saveQueuedItems = new List<string>();

        public Action OnContainerUpdated { get; set; }
        public Action<TechType> OnRecyclingItem { get; set; }
        public int MaxStorage { get; set; }

        public void Initialize(FcsDevice mono, int maxStorage)
        {
            Controller = mono;
            MaxStorage = maxStorage;
            _storageContainer = mono.gameObject.AddComponent<FCSStorage>();
            _storageContainer.Initialize(maxStorage, GameObjectHelpers.FindGameObject(gameObject, "ProcessingStorageRoot"));
            _storageContainer.onAddItem += item => { UpdateTracker(); };
            _storageContainer.onRemoveItem += item => { UpdateTracker(); };
            
        }

        internal void AddItem(InventoryItem item)
        {
            _wasteList.Enqueue(new Waste(item, DayNightCycle.main.timePassed));
            OnStartingRecycle?.Invoke();
            OnContainerUpdated?.Invoke();
        }

        public Action OnStartingRecycle { get; set; }

        internal Pickupable RemoveItem(TechType item)
        {
            var inventoryItem = _storageContainer.RemoveItem(item);
            UpdateTracker();
            OnContainerUpdated?.Invoke();
            return inventoryItem;
        }

        public void Recycle()
        {
            if (Controller == null || Controller.Manager == null ||!Controller.IsOperational || _wasteList == null || _wasteList.Count <= 0)
            {
                return;
            }
            
            QuickLogger.Debug($"Recycling Item",true);

            var wasteItem = _wasteList.Dequeue();
            OnRecyclingItem?.Invoke(wasteItem.InventoryItem.item.GetTechType());
            QuickLogger.Debug($"Recycling Item {wasteItem.InventoryItem.item.GetTechType()}", true);

            InventoryItem inventoryItem = wasteItem.InventoryItem;
            GameObject gameObject = inventoryItem.item.gameObject;
            TechType techType = CraftData.GetTechType(gameObject);
            List<IIngredient> list = GetIngredients(wasteItem.InventoryItem.item);
            QuickLogger.Debug($"List Null: {list != null} | List Count: {list.Count > 0} | Craft Amount: {CraftData.Get(techType).craftAmount <= 1} | {!BannedTech.Contains(techType)}", true);
            if (list != null && list.Count > 0 && CraftData.Get(techType).craftAmount <= 1 && !BannedTech.Contains(techType))
            {
                QuickLogger.Debug($"Attempting Recycling Item.", true);
                EnergyMixin component = gameObject.GetComponent<EnergyMixin>();
                if (component)
                {
                    GameObject battery = component.GetBattery();
                    if (battery)
                    {
                        //TODO Remove Battery
                        InventoryItem inventoryItem2 = new InventoryItem(battery.GetComponent<Pickupable>());
                        _storageContainer.UnsafeAdd(inventoryItem2);
                    }
                }
                foreach (IIngredient ingredient in list)
                {
                    for (int i = 0; i < ingredient.amount; i++)
                    {
                        if (!BatteryTech.Contains(ingredient.techType))
                        {
                            InventoryItem newInventoryItem = new InventoryItem(Instantiate(CraftData.GetPrefabForTechType(ingredient.techType)).GetComponent<Pickupable>());
                            QuickLogger.Debug($"Message: {newInventoryItem.item.name}", true);
                            _storageContainer.UnsafeAdd(newInventoryItem);
                        }
                    }
                }

                UpdateTracker();
                Destroy(gameObject);
                OnContainerUpdated?.Invoke();
            }
            else
            {
                QuickLogger.Debug($"Attempting Recycling Item Failed.", true);
            }
        }

        private bool IsUsedBattery(GameObject obj)
        {
            Battery component = obj.GetComponent<Battery>();
            return component != null && (double)component.charge < (double)component.capacity * 0.97;
        }

        public List<IIngredient> GetIngredients(Pickupable pickup)
        {
            Ingredients.Clear();
            GameObject gObj = pickup.gameObject;
            if (gObj)
            {
                var it = CraftData.Get(CraftData.GetTechType(gObj));
                List<IIngredient> readOnlyCollection = new List<IIngredient>();
                for (int i = 0; i < it.ingredientCount; i++)
                {
                    readOnlyCollection.Add(it.GetIngredient(i));
                }

                foreach (IIngredient ingredient in readOnlyCollection)
                {
                    if (!BatteryTech.Contains(ingredient.techType))
                    {
                        Ingredients.Add(ingredient);
                    }
                }

                EnergyMixin component = gObj.GetComponent<EnergyMixin>();
                if (component)
                {
                    GameObject battery = component.GetBattery();
                    if (battery)
                    {
                        Ingredients.Add(new Ingredient(CraftData.GetTechType(battery), 1));
                    }
                }
            }
            return Ingredients;
        }

        private bool ContainsCraftData(TechType techType)
        {
            var originData = CraftData.Get(techType, true);
            if (originData == null)
            {
                QuickLogger.DebugError($"Failed to load ITechData for TechType '{techType}'.");
                return false;
            }

            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable)
        {
            var techType = pickupable.GetTechType();
            var result = ContainsCraftData(techType) && !BannedTech.Contains(techType) && !IsUsedBattery(gameObject) && (GetIngredients(pickupable).Count + _storageContainer.GetCount()) <= MaxStorage;
            QuickLogger.Debug($"Can hold item result: {result} || Storage Total: {GetIngredients(pickupable).Count + _storageContainer.GetCount()}", true);
            return result;
        }

        private void  UpdateTracker()
        {
            Controller.RefreshUI();
        }

        public Dictionary<TechType,int> GetStorage()
        {
            return _storageContainer.GetItems();
        }

        public int GetCount(TechType techType)
        {
            return _storageContainer.GetCount(techType);
        }

        internal byte[] Save(ProtobufSerializer serializer, TrashRecyclerDataEntry savedData)
        {
            savedData.QueuedItems = GetQueuedItems();
            return _storageContainer.Save(serializer);
        }

        private IEnumerable<string> GetQueuedItems()
        {
            foreach (Waste waste in _wasteList)
            {
                yield return waste.InventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            }
        }

        public int GetCount()
        {
            return _storageContainer.GetCount();
        }

        public bool HasItems()
        {
            return _wasteList.Count > 0;
        }

        public string GetCurrentItem()
        {
            return Language.main.Get(_wasteList.Peek().InventoryItem.item.GetTechType());
        }

        public void Load(TrashRecyclerDataEntry data, ProtobufSerializer serializer)
        {
            if (data.QueuedItems != null)
            {
                _saveQueuedItems = data.QueuedItems.ToList();
            }
            _storageContainer.RestoreItems(serializer, data.Storage);
        }
    }
}