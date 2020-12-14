using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.TrashRecycler.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_HomeSolutions.TrashRecycler.Mono
{
    internal class Recycler : MonoBehaviour
    {
        public FcsDevice Controller { get; set; }
        private readonly Queue<Waste> _wasteList = new Queue<Waste>();
        private static HashSet<TechType> batteryTech;
        public static HashSet<TechType> BatteryTech 
        { 
            get 
            { 
                if(batteryTech is null || batteryTech.Count == 0)
                {
                    batteryTech = new HashSet<TechType>(BatteryCharger.compatibleTech).Concat(PowerCellCharger.compatibleTech).ToHashSet();
                }
                return batteryTech;
            } 
        }

        private FCSStorage _storageContainer;
        private List<string> _saveQueuedItems = new List<string>();

        public Action OnContainerUpdated { get; set; }
        public Action<TechType> OnRecyclingItem { get; set; }
        public int MaxStorage { get; set; }
        public int BioMaterials { get; set; }

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

            var data = CraftData.Get(item.item.GetTechType());

            if(data != null && data.linkedItemCount > 0)
            {
                Dictionary<TechType, int> pairs = new Dictionary<TechType, int>();
                int count = 0;
                for (int i = 0; i < data.linkedItemCount; i++)
                {
                    TechType techType = data.GetLinkedItem(i);
                    if (pairs.ContainsKey(techType))
                        pairs[techType] += 1;
                    else
                        pairs[techType] = 1;

                    count++;
                }

                ItemsContainer inventory = Inventory.main?.container;
                List<InventoryItem> inventoryItems = new List<InventoryItem>();

                if (inventory != null)
                {
                    foreach (KeyValuePair<TechType, int> pair in pairs)
                    {
                        if (inventory.GetCount(pair.Key) >= pair.Value)
                        {
                            var items = inventory.GetItems(pair.Key);
                            for (int j =0; j< pair.Value; j++)
                            {
                                InventoryItem item1 = items[j];
                                if (inventory.RemoveItem(item1.item))
                                    inventoryItems.Add(item1);
                            }
                        }
                    }
                }

                if(count > inventoryItems.Count)
                {
                    inventoryItems.ForEach((x) => inventory.UnsafeAdd(x));
                    inventory.UnsafeAdd(item);
                    return;
                }
                else
                {
                    inventoryItems.ForEach((x) => Destroy(x.item.gameObject));
                }

            }

            double time = DayNightCycle.main.timePassed;
            _wasteList.Enqueue(new Waste(item, time));
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
            TechType techType = inventoryItem.item.GetTechType();

            List<IIngredient> list = GetIngredients(wasteItem.InventoryItem.item);
            QuickLogger.Debug($"Ingredients != Null: {list != null} | List Count: {list.Count > 0}", true);
            if (list != null && list.Count > 0 )
            {
                QuickLogger.Debug($"Attempting Recycling Item.", true);
                EnergyMixin component = gameObject.GetComponent<EnergyMixin>();
                if (component)
                {
                    GameObject battery = component.GetBattery();
                    if (battery)
                    {
                        Pickupable pickupable = battery.GetComponent<Pickupable>();
                        if(techType != TechType.Welder || pickupable.GetTechType() != TechType.Battery)
                        {
                            InventoryItem inventoryItem2 = new InventoryItem(pickupable);
                            _storageContainer.UnsafeAdd(inventoryItem2);
                        }
                    }
                }

                StartCoroutine(RecycleCoroutine(gameObject, list));
            }
            else
            {
                QuickLogger.Debug($"Attempting Recycling Item Failed.", true);
            }
        }

        private IEnumerator RecycleCoroutine(GameObject gameObject, List<IIngredient> list)
        {
            foreach (IIngredient ingredient in list)
            {
                for (int i = 0; i < ingredient.amount; i++)
                {
                    if (!BatteryTech.Contains(ingredient.techType))
                    {
                        CoroutineTask<GameObject> getPrefab = CraftData.GetPrefabForTechTypeAsync(ingredient.techType, false);
                        yield return getPrefab;

                        GameObject ingredientObject = Instantiate(getPrefab.GetResult());
                        if (!ingredientObject.GetComponent<LiveMixin>() && !ingredientObject.GetComponent<Plantable>() && !ingredientObject.GetComponent<Eatable>())
                        {
                            InventoryItem newInventoryItem = new InventoryItem(ingredientObject.GetComponent<Pickupable>());
                            QuickLogger.Debug($"Message: {newInventoryItem.item.name}", true);
                            _storageContainer.UnsafeAdd(newInventoryItem);
                        }
                        else
                        {
                            DestroyImmediate(ingredientObject);
                            BioMaterials++;

                            if(BioMaterials >= 10 && TechTypeHandler.TryGetModdedTechType("FCSBioFuel", out TechType FCSBioFuel))
                            {
                                getPrefab = CraftData.GetPrefabForTechTypeAsync(FCSBioFuel, false);
                                yield return getPrefab;

                                ingredientObject = Instantiate(getPrefab.GetResult());
                                InventoryItem newInventoryItem = new InventoryItem(ingredientObject.GetComponent<Pickupable>());
                                QuickLogger.Debug($"Message: {newInventoryItem.item.name}", true);
                                _storageContainer.UnsafeAdd(newInventoryItem);
                                BioMaterials = 0;
                            }
                        }
                    }
                }
            }

            UpdateTracker();
            Destroy(gameObject);
            OnContainerUpdated?.Invoke();
            yield break;
        }

        private bool IsUsedBattery(Pickupable pickupable)
        {
            IBattery component = pickupable.GetComponent<IBattery>();
            return component != null && (double)component.charge < (double)component.capacity * 0.97;
        }

        public List<IIngredient> GetIngredients(Pickupable pickup)
        {
            List<IIngredient> Ingredients = new List<IIngredient>();
            GameObject gObj = pickup?.gameObject;
            if (gObj)
            {
                var it = CraftData.Get(pickup.GetTechType());
                if(it != null)
                {
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
                }
            }
            return Ingredients;
        }

        private bool ContainsValidCraftData(TechType techType)
        {
            var data = CraftData.Get(techType, true);
            if (data == null || data.craftAmount > 1)
            {
                QuickLogger.Debug($"TechType '{techType}' has no valid recipe for recycling.");
                return false;
            }

            if (data.linkedItemCount > 0)
            {
                Dictionary<TechType, int> pairs = new Dictionary<TechType, int>();
                for (int i = 0; i < data.linkedItemCount; i++)
                {
                    TechType techType2 = data.GetLinkedItem(i);
                    if (pairs.ContainsKey(techType2))
                        pairs[techType2] += 1;
                    else
                        pairs[techType2] = 1;
                }

                ItemsContainer inventory = Inventory.main?.container;

                if (inventory is null)
                    return false;

                foreach(KeyValuePair<TechType, int> pair in pairs)
                {
                    if (inventory.GetCount(pair.Key) < pair.Value)
                        return false;
                }
            }

            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable)
        {
            var techType = pickupable.GetTechType();
            List<IIngredient> list = GetIngredients(pickupable);
            var result = ContainsValidCraftData(techType) && !IsUsedBattery(pickupable) && (list.Count + _storageContainer.GetCount()) <= MaxStorage;
            QuickLogger.Debug($"Can hold item result: {result} || Storage Total: {list.Count + _storageContainer.GetCount()}", true);
            return result;
        }

        private void UpdateTracker()
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
            savedData.BioMaterialsCount = BioMaterials;
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

            BioMaterials = data.BioMaterialsCount;
            _storageContainer.RestoreItems(serializer, data.Storage);
        }
    }
}