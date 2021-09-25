using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Mono
{
    internal class CraftMachine : MonoBehaviour
    {
        internal bool IsOccupied { get; private set; }
        public int CrafterID { get; set; }
        internal Action<CraftingOperation> OnComplete { get; set; }
        private int _goal;
        public AutoCrafterController _mono;
        private Dictionary<TechType, int> _neededItems = new();

        private int _testTechType = 34;//11371;
        //private Dictionary<TechType,bool> _ingredients = new();
        private  float MAXTIME = 7f;
        private readonly Queue<TechType> _craftingQueue = new();
        private CraftingOperation _operation;
        private bool _isInitialized;
        private GameObject _beltPathPoints;
        private Transform[] _crafterBeltPath;
        //private CraftMachine _craftSlot1;
        private int _itemsOnBelt;
        private float _checkWaitPeriod = 1f;
        private bool _isCrafting;

        private void Start()
        {
            //InvokeRepeating(nameof(TryCraftRequiredItems),1f,1f);
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            _beltPathPoints = GameObjectHelpers.FindGameObject(gameObject, "Belt1WayPoints");
            _crafterBeltPath = _beltPathPoints.GetChildrenT();
            //_craftSlot1 = gameObject.AddComponent<CraftMachine>();
            //_craftSlot1.CrafterID = 1;
            OnComplete += item =>
            {
                if (item == null) return;
                
                if (!item.IsRecursive)
                {
                    _mono.Manager.RemoveCraftingOperation(item);
                    item.UnMount(_mono);
                    _neededItems.Clear();
                    _isCrafting = false;
                    _mono.Manager.NotifyByID("DTC", "RefreshCraftingGrid");
                }
            };

            _isInitialized = true;
        }

        public bool ItemsOnBelt()
        {
            return _itemsOnBelt > 0;
        }

        private void Test()
        {
            StartCrafting(new CraftingOperation
            {
                Amount = 1, 
                TechType = (TechType)_testTechType
            });

            //_neededItems = new Dictionary<TechType,int>();
            //GetRequiredItems((TechType)_testTechType);
            //GetIngredients((TechType) _testTechType);
            //QuickLogger.Debug($"Ingredients Count: {TechDataHelpers.ContainsValidCraftData((TechType)11360)}");
        }

        public void StartCrafting(CraftingOperation craftingItem)
        {
            if (craftingItem == null) return;

            _operation = craftingItem;
            //GetIngredients(craftingItem.TechType);
            GetRequiredItems(craftingItem.TechType);

            //Now that we got the items try craft the missing Items
            

            //_mono.CraftManager.GetCraftingOperation().IsBeingCrafted = true;

            StartCoroutine(TryCraftRequiredItems());

            _goal = craftingItem.Amount;
            IsOccupied = true;
        }

        private IEnumerator TryCraftRequiredItems()
        {
            if (_craftingQueue.Any() && !IsCrafting())
            {
                while (!CheckIfAllItemsAvailable())
                {
                    QuickLogger.Debug($"{_mono.UnitID} : All item not available to complete craft.",true);
                    _isCrafting = false;
                    yield return new WaitForSeconds(_checkWaitPeriod);
                    yield return null;
                }

                _isCrafting = true;

                for (int i = 0; i < _craftingQueue.Count; i++)
                {
                    CraftItem(_craftingQueue.Dequeue());
                    yield return new WaitForSeconds(MAXTIME);
                }

                if (_craftingQueue.Any())
                {
                    _isCrafting = false;
                    yield return StartCoroutine(TryCraftRequiredItems());
                }
                else
                {
                    CraftItem(_operation.TechType,true);
                    OnComplete?.Invoke(_operation);
                }
            }
            yield break;
        }

        private bool CheckIfAllItemsAvailable()
        {
            foreach (KeyValuePair<TechType, int> item in _neededItems)
            {
                if (_mono.Manager.GetItemCount(item.Key) < item.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void CraftItem(TechType techType,bool isComplete)
        {
            var amount = _operation.ReturnAmount;

            if (!_mono.Manager.IsAllowedToAdd(techType, 1, true, false))
            {
                _mono.ShowMessage($"{Language.main.Get(techType)} is not allowed in your system. Please add a server that can store this item or add an unformatted server.");
                return;
            }

            if (_mono.Manager.HasIngredientsFor(techType))
            {
                _mono.Manager.ConsumeIngredientsFor(techType);
                AttemptToAddToNetwork(techType);
                var techData = CraftDataHandler.GetTechData(techType);

                foreach (Ingredient ingredient in techData.Ingredients)
                {
                    if (_neededItems.ContainsKey(ingredient.techType))
                    {
                        _neededItems[ingredient.techType] -= ingredient.amount;
                        if (_neededItems[ingredient.techType] <= 0)
                        {
                            _neededItems.Remove(ingredient.techType);
                        }
                    }
                }

            }
            else
            {
                QuickLogger.Debug($"All items not crafted for {Language.main.Get(techType)}. Re-adding to the bottom of the queue and crafting next in queue.", true);
                _craftingQueue.Enqueue(techType);
                if (_craftingQueue.Count > 1)
                    CraftItem(_craftingQueue.Dequeue(),false);
                return;
            }

            SpawnBeltItem(techType);
        }

        private void SpawnBeltItem(TechType techType)
        {
            if (_crafterBeltPath == null) return;

            //Spawn items
            var inv = GameObject.Instantiate(ModelPrefab.DSSCrafterCratePrefab);
            inv.transform.SetParent(_beltPathPoints.transform, false);
            var follow = inv.AddComponent<AutoCrafterCrateController>();
            follow.Initialize(_crafterBeltPath, _mono, techType);
            follow.OnPathComplete += OnPathComplete;
            _itemsOnBelt++;
        }

        private void OnPathComplete()
        {
            if (_itemsOnBelt == 0) return;
            _itemsOnBelt -= 1;
            if (_itemsOnBelt < 0) _itemsOnBelt = 0;
        }

        private bool IsCrafting()
        {
            return _isCrafting;
        }

        public int GetGoal()
        {
            return _goal;
        }

        private void Update()
        {
            if(WorldHelpers.CheckIfPaused())  return;



            //if (_mono?.CraftManager?.GetCraftingOperation() == null || Mod.Craftables == null || _mono?.Manager == null || !IsOccupied) return;

            //var techType = _mono.CraftManager.GetCraftingOperation().TechType;
            //var amount = _mono.CraftManager.GetCraftingOperation().ReturnAmount;

            //if (!Mod.Craftables.Contains(techType)) return;

            //if (!_mono.Manager.IsAllowedToAdd(techType, amount, true, false))
            //{
            //    _mono.ShowMessage($"{Language.main.Get(techType)} is not allowed in your system. Please add a server that can store this item or add an unformatted server.");
            //    _mono.CancelOperation();
            //    return;
            //}

            //if (_startBuffer > 0)
            //{
            //    _startBuffer -= DayNightCycle.main.deltaTime;
            //    return;
            //}

            //if (!Mod.Craftables.Contains(techType)) return;

            //var hasIngredients = _mono.Manager.HasIngredientsFor(techType);

            //if (hasIngredients && !_mono.CraftManager.GetCraftingOperation().IsComplete)
            //{
            //    _mono.CraftManager.GetCraftingOperation().AppendCompletion();
            //    _mono.Manager.ConsumeIngredientsFor(techType);
            //    var craftingTechType = _mono.CraftManager.GetCraftingOperation().FixCustomTechType();

            //    for (int i = 0; i < amount; i++)
            //    {
            //        AttemptToAddToNetwork(craftingTechType);
            //    }

            //    _startBuffer = MAXTIME;
            //    _mono.CraftManager.SpawnItem(_mono.CraftManager.GetCraftingOperation().TechType);
            //}
            //else
            //{
            //    _startBuffer = 1;
            //}

            //if (!hasIngredients)
            //{
            //    if (NotMetIngredients.Any())
            //    {
            //        _mono.AskForCraftingAssistance(NotMetIngredients.ElementAt(0).Key);
            //    }
            //}

            //if (_mono.CraftManager.GetCraftingOperation().IsComplete)
            //{
            //    QuickLogger.Debug($"Is Complete", true);
            //    OnComplete?.Invoke(_mono?.CraftManager?.GetCraftingOperation());

            //    if (_mono?.CraftManager?.GetCraftingOperation() == null)
            //    {
            //        IsOccupied = false;
            //    }
            //    else if (!_mono.CraftManager.GetCraftingOperation().IsRecursive)
            //    {
            //        IsOccupied = false;
            //    }
            //}
        }

        private void AttemptToAddToNetwork(TechType techType)
        {
            var inventoryItem = techType.ToInventoryItemLegacy();

            QuickLogger.Debug($"InventoryItemLegacy returned: {Language.main.Get(inventoryItem.item.GetTechType())}");

            var result = BaseManager.AddItemToNetwork(_mono.Manager, inventoryItem, true);

            if (!result)
            {
                _mono.ShowMessage($"Failed to add {Language.main.Get(techType)} to storage. Please build a locker, remote storage or add more space to your data storage system. Your item will be added to the autocrafter storage/");
                _mono.AddItemToStorage(techType);
                Destroy(inventoryItem.item.gameObject);
            }
        }

        //private void GetIngredients(TechType techType)
        //{
        //    _ingredients.Clear();

        //    foreach (IIngredient ingredient in TechDataHelpers.GetIngredients(techType))
        //    {
        //        _ingredients.Add(ingredient.techType, _mono.Manager.GetItemCount(ingredient.techType) >= ingredient.amount);
        //    }
        //}

        private void GetRequiredItems(TechType craftingItemTechType)
        {
            var ingredients = new List<IIngredient>(TechDataHelpers.GetIngredients(craftingItemTechType));

            foreach (IIngredient ingredient in ingredients)
            {
                var amount = _neededItems.ContainsKey(ingredient.techType)
                    ? ingredient.amount + _neededItems[ingredient.techType]
                    : ingredient.amount;

                if (_mono.Manager.GetItemCount(ingredient.techType) >= amount)
                {
                    AppendIngredient(ingredient.techType, ingredient.amount);
                    continue;
                }

                if (TechDataHelpers.GetIngredientCount(ingredient.techType) > 0)
                {
                    _craftingQueue.Enqueue(ingredient.techType);
                    GetRequiredItems(ingredient.techType);
                }
                else
                {
                    AppendIngredient(ingredient.techType,ingredient.amount);
                }
                
                //NotMetIngredients.Add(ingredient.techType, ingredient.amount);
                //_mono.AddMissingItem(Language.main.Get(ingredient.techType), ingredient.amount);
            }
        }

        private void AppendIngredient(TechType techType, int amount)
        {
            if (_neededItems.ContainsKey(techType))
            {
                _neededItems[techType] += amount;
            }
            else
            {
                _neededItems.Add(techType,amount);   
            }
        }

        public void Reset(bool bypass)
        {
            //if (_mono?.CraftManager?.GetCraftingOperation() == null) return;
            //if (!_mono.CraftManager.GetCraftingOperation().IsRecursive || bypass)
            //{
            //    _mono.CraftManager.StopOperation();
            //    IsOccupied = false;
            //    _goal = 0;
            //}
            //_startBuffer = 0;
        }
    }
}
