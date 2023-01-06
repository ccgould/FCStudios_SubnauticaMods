using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.TrashRecycler.Model;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.TrashRecycler.Mono
{
    internal class Recycler : MonoBehaviour
    {
        public FcsDevice Controller { get; set; }
        private Queue<Waste> _wasteList = new Queue<Waste>();
        private FCSStorage _storageContainer;


        public Action OnContainerUpdated { get; set; }
        public Action<TechType> OnRecyclingItem { get; set; }
        public int MaxStorage { get; set; }
        public int BioMaterials { get; set; }

        public void Initialize(FcsDevice mono, int maxStorage)
        {
            Controller = mono;
            MaxStorage = maxStorage;
            _storageContainer = gameObject.GetComponent<FCSStorage>();
            _storageContainer.SlotsAssigned = maxStorage;
            _storageContainer.Deactivate();
            _storageContainer.ItemsContainer.onAddItem += item => { UpdateTracker(); };
            _storageContainer.ItemsContainer.onRemoveItem += item => { UpdateTracker(); };
            
        }

        internal void AddItem(InventoryItem item)
        {

            var data = CraftDataHandler.GetTechData(item.item.GetTechType());

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

                ItemsContainer inventory = Inventory.main != null? Inventory.main.container: null;
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
                    if(count > inventoryItems.Count)
                    {
                        inventoryItems.ForEach((x) => inventory.UnsafeAdd(x));
                        inventory.UnsafeAdd(item);
                        return;
                    }

                    inventoryItems.ForEach((x) => Destroy(x.item.gameObject));
                }
            }

            double time = DayNightCycle.main.timePassed;
            _wasteList.Enqueue(new Waste(item, time));
            Destroy(item.item.gameObject);
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
            
            var wasteItem = _wasteList.Dequeue();
            TechType techType = wasteItem.TechType;

            OnRecyclingItem?.Invoke(techType);
            QuickLogger.Debug($"Recycling Item ({Language.main.Get(techType)})", true);


            List<Ingredient> list = TechDataHelpers.GetIngredientsWithOutBatteries(techType);
            QuickLogger.Debug($"Valid Ingredients Count: {list?.Count ?? 0}", true);
            if (list != null && list.Count > 0 )
            {
                EnergyMixin component = gameObject.GetComponent<EnergyMixin>();
                if (component)
                {
                    GameObject battery = component
                        .GetBatteryGameObject();
                    if (battery)
                    {
                        QuickLogger.Debug($"Removing Battery from {Language.main.Get(techType)}", true);
                        Pickupable pickupable = battery.GetComponent<Pickupable>();
                        InventoryItem inventoryItem2 = new InventoryItem(pickupable);
                        _storageContainer.ItemsContainer.UnsafeAdd(inventoryItem2);
                    }
                }

                StartCoroutine(RecycleCoroutine(techType, list));
            }
            else
            {
                QuickLogger.Debug($"Attempting Recycling Item Failed.", true);
            }
        }

        private IEnumerator RecycleCoroutine(TechType techType, List<Ingredient> list)
        {
            foreach (Ingredient ingredient in list)
            {
                for (int i = 0; i < ingredient.amount; i++)
                {

                    CoroutineTask<GameObject> getPrefab = CraftData.GetPrefabForTechTypeAsync(ingredient.techType, false);
                    yield return getPrefab;

                    GameObject ingredientObject = Instantiate(getPrefab.GetResult());
                    ingredientObject.SetActive(false);
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

                        if (BioMaterials >= 10 && TechTypeHandler.TryGetModdedTechType("FCSBioFuel", out TechType FCSBioFuel))
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

            UpdateTracker();
            OnContainerUpdated?.Invoke();
            yield break;
        }

        public bool IsAllowedToAdd(TechType techType)
        {
            throw new NotImplementedException("This method cannot be used please use IsAllowedToAdd(Pickupable pickupable, bool verbose).");
        }

        public bool IsAllowedToAdd(Pickupable pickupable)
        {
            TechType techType = pickupable.GetTechType();
            List<Ingredient> list = TechDataHelpers.GetIngredientsWithOutBatteries(techType);
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

        internal void Save(ProtobufSerializer serializer, TrashRecyclerDataEntry savedData)
        {
            savedData.QueuedItems = _wasteList;
            savedData.BioMaterialsCount = BioMaterials;
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
            if (_wasteList == null || !_wasteList.Any()) return string.Empty;
            return Language.main.Get(_wasteList.Peek().TechType);
        }

        public void Load(TrashRecyclerDataEntry data)
        {
            if (data.QueuedItems != null)
            {
                _wasteList = data.QueuedItems;
            }
            
            BioMaterials = data.BioMaterialsCount;
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