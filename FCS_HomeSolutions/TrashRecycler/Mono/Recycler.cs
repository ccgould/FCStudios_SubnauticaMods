using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
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
            _storageContainer = gameObject.AddComponent<FCSStorage>();
            _storageContainer.Initialize(maxStorage);
            _storageContainer.ItemsContainer.onAddItem += item => { UpdateTracker(); };
            _storageContainer.ItemsContainer.onRemoveItem += item => { UpdateTracker(); };
            
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

                inventoryItems.ForEach((x) => Destroy(x.item.gameObject));
            }

            double time = DayNightCycle.main.timePassed;
            _wasteList.Enqueue(new Waste(item, time));
            OnStartingRecycle?.Invoke();
            OnContainerUpdated?.Invoke();
        }

        public Action OnStartingRecycle { get; set; }

        internal Pickupable RemoveItem(TechType item)
        {
            var inventoryItem = _storageContainer.ItemsContainer.RemoveItem(item);
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

            List<IIngredient> list = TechDataHelpers.GetIngredients(wasteItem.InventoryItem.item);
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
                            _storageContainer.ItemsContainer.UnsafeAdd(inventoryItem2);
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
                    if (!TechDataHelpers.BatteryTech.Contains(ingredient.techType))
                    {
                        CoroutineTask<GameObject> getPrefab = CraftData.GetPrefabForTechTypeAsync(ingredient.techType, false);
                        yield return getPrefab;

                        GameObject ingredientObject = Instantiate(getPrefab.GetResult());
                        if (!ingredientObject.GetComponent<LiveMixin>() && !ingredientObject.GetComponent<Plantable>() && !ingredientObject.GetComponent<Eatable>())
                        {
                            InventoryItem newInventoryItem = new InventoryItem(ingredientObject.GetComponent<Pickupable>());
                            QuickLogger.Debug($"Message: {newInventoryItem.item.name}", true);
                            _storageContainer.ItemsContainer.UnsafeAdd(newInventoryItem);
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
                                _storageContainer.ItemsContainer.UnsafeAdd(newInventoryItem);
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
        
        public bool IsAllowedToAdd(Pickupable pickupable)
        {
            var techType = pickupable.GetTechType();
            List<IIngredient> list = TechDataHelpers.GetIngredients(pickupable);
            var result = TechDataHelpers.ContainsValidCraftData(techType) && !TechDataHelpers.IsUsedBattery(pickupable) && (list.Count + _storageContainer.GetCount()) <= MaxStorage;
            QuickLogger.Debug($"Can hold item result: {result} || Storage Total: {list.Count + _storageContainer.GetCount()}", true);
            return result;
        }


        private void UpdateTracker()
        {
            Controller.RefreshUI();
        }

        public FCSStorage GetStorage()
        {
            return _storageContainer;
        }

        public int GetCount(TechType techType)
        {
            return _storageContainer.ItemsContainer.GetCount(techType);
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
            return _wasteList.Count > 0 || _storageContainer.ItemsContainer.count > 0;
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

        public bool CanPendItem(Pickupable pickupable)
        {
            var ingredientCount = TechDataHelpers.GetIngredientCount(pickupable);
            var pendingIngredientCount = _wasteList.Sum(x => x.IngredientCount);
            var storageTotal = _storageContainer.GetCount();
            QuickLogger.Debug($"Can Pend Calc: {ingredientCount + pendingIngredientCount + storageTotal} | Result: {ingredientCount + pendingIngredientCount <= MaxStorage}",true);
            return ingredientCount + pendingIngredientCount + storageTotal <= MaxStorage;

        }
    }
}