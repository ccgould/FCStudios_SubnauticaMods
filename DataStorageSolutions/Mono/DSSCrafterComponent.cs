using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSCrafterComponent : MonoBehaviour
    {
        private bool _isCrafting;
        private bool _stopOperation;
        private const int MaxAmountOfCrafts = 1;
        private Queue<AutoCraftOperationData> _craftingQueue = new Queue<AutoCraftOperationData>();
        private FCSOperation _operation;
        private DSSAutoCrafterController _crafter;
        private Transform[] _crafterBeltPath;
        private BaseManager Manager { get; set; }

        private void Start()
        {
            InvokeRepeating(nameof(CheckIfCanCraft), 1f, 1f);
        }

        private void CheckIfCanCraft()
        {
            //QuickLogger.Debug("Try Craft", true);
            if (!_isCrafting)
            {
                _isCrafting = true;
                //Start Crafting
                //QuickLogger.Debug("Start Craft", true);
                StartCoroutine(TryCraft());
            }
        }

        private IEnumerator TryCraft()
        {
            if (_stopOperation)
            {
                //QuickLogger.Debug("Operation Stopped");
                _isCrafting = false;
                yield break;
            }

            if (_operation != null && !_operation.IsAutoCraftingAllowed)
            {
                //QuickLogger.Debug("Crafting is not allowed");
                _isCrafting = false;
                yield break;
            }

            if (_craftingQueue.Count == 0)
            {
                //QuickLogger.Debug("Queue Count is 0");
                _isCrafting = false;
                yield break;
            }

            var item = _craftingQueue.Peek();

            if (item.AutoCraftRequestItem == TechType.None)
            {
                //QuickLogger.Debug("TechType is none");
                _isCrafting = false;
                yield break;
            }

            //Check if there is tech data
            var techData = DSSHelpers.CheckIfTechDataAvailable(item);
            if (techData == null)
            {
                //QuickLogger.Debug("TechData is none");
                _isCrafting = false;
                yield break;
            }

            item.TechData = techData;
            //Check if we have enough items
            if (Manager.StorageManager.GetItemCount(item.AutoCraftRequestItem) >= item.AutoCraftMaxAmount)
            {
                _isCrafting = false;
                _craftingQueue.Dequeue();
                _craftingQueue.Enqueue(item);
                yield break;
            }

            //Check if we can store result
            if (!Manager.StorageManager.CanBeStored(techData.craftAmount, item.AutoCraftRequestItem))
            {
                //QuickLogger.Debug("Cannot Store item");
                _isCrafting = false;
                yield break;
            }

            //Get crafting time
            CraftData.GetCraftTime(item.AutoCraftRequestItem, out float craftingTime);

            if (CheckIfCanCraft(techData, item)) yield break;
            RemoveItemsFromBase(techData);
            yield return new WaitForSeconds(craftingTime + 2.7f);
            SpawnItem(item);

            _craftingQueue.Dequeue();
            _craftingQueue.Enqueue(item);
        }

        private bool CheckIfCanCraft(TechData techData, AutoCraftOperationData item)
        {
            //Check if we have enough items
            if (!DSSHelpers.CanCraftItem(Manager, techData))
            {
                QuickLogger.Debug("Not enough items", true);
                _isCrafting = false;
                _craftingQueue.Dequeue();
                _craftingQueue.Enqueue(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a bool depending if crafter can handle anymore operations
        /// </summary>
        /// <returns></returns>
        internal bool CanAcceptCraft()
        {
            return _craftingQueue.Count + 1 <= MaxAmountOfCrafts;
        }

        internal bool AddCraftToQueue(AutoCraftOperationData item, out string message)
        {
            //Check if craft already exist
            var exist = _craftingQueue.Any(x => x.AutoCraftRequestItem == item.AutoCraftRequestItem);
            if (exist)
            {
                message = string.Format(AuxPatchers.CraftExistErrorMessageFormat(), item.AutoCraftRequestItem);
                return false;
            }

            if (!CanAcceptCraft())
            {
                message = string.Format(AuxPatchers.MaxCraftLimitReachedFormat(), MaxAmountOfCrafts);
                return false;
            }

            _craftingQueue.Enqueue(item);
            _crafter.UpdateScreen();

            message = string.Empty;
            return true;

        }

        public void RemoveQueue(AutoCraftOperationData operation)
        {
            if (_craftingQueue.Contains(operation))
            {
                _craftingQueue = new Queue<AutoCraftOperationData>(_craftingQueue.Where(x => x != operation));
            }
            
            _crafter.UpdateScreen();
        }

        private void RemoveItemsFromBase(TechData techData)
        {

            //Remove items from base
            foreach (Ingredient ingredient in techData.Ingredients)
            {
                for (int i = 0; i < ingredient.amount; i++)
                {
                    var pickUp = Manager.StorageManager.RemoveItemFromBase(ingredient.techType, false, true);
                }
            }
        }

        private void SpawnItem(AutoCraftOperationData item)
        {
            //Spawn items
            var inv = GameObject.Instantiate(DSSModelPrefab.DSSCrafterCratePrefab);
            var follow = inv.gameObject.AddComponent<DSSAutoCrafterCrateController>();
            follow.Status = GetStatus;
            follow.OnEndOfPath += () =>
            {
                QuickLogger.Debug("At End Of path");
                _isCrafting = false;
            };
            follow.TechType = item.AutoCraftRequestItem;
            follow.Amount = item.TechData.craftAmount;
            follow.Initialize(Manager, _crafterBeltPath);
        }

        internal Status GetStatus()
        {
            return _crafter.CurrentStatus;
        }

        public void Initialize(DSSAutoCrafterController crafter, Transform[] crafterBeltPath, BaseManager manager)
        {
            _crafter = crafter;
            Manager = manager;
            _crafterBeltPath = crafterBeltPath;
        }

        public bool IsCrafting()
        {
            return _isCrafting;
        }

        public IEnumerable<AutoCraftOperationData> GetCraftingQueue()
        {
            return _craftingQueue;
        }

        public void LinkOperation(FCSOperation operation)
        {
            _operation = operation;
        }
    }
}
