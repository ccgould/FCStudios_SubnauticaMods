using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Mods.AutoCrafter.Helpers;
using FCS_ProductionSolutions.Mods.AutoCrafter.Patches;
using FCSCommon.Utilities;
using SMLHelper.Handlers;
using FCS_AlterraHub.Helpers;
using Newtonsoft.Json;
using UWE;

using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States
{
    internal class CrafterCraftingState : CrafterBaseState
    {
        [JsonProperty] internal CraftingOperation _operation;
        [JsonProperty] internal float _timeLeft;
        [JsonProperty] internal Dictionary<TechType, int> _consumable;
        [JsonProperty] internal Dictionary<TechType, int> _crafted;

        [JsonProperty] internal Dictionary<TechType, int> _availableItems = new();
        [JsonProperty] internal Dictionary<TechType, int> _recipeItems = new();
        [JsonProperty] internal Dictionary<TechType, int> _toCraftItems = new();

        private const float MAXTIME = 7f;
        public override string Name => "CrafterCraftingState";

        public CrafterCraftingState()
        {
        }

        public CrafterCraftingState(CrafterStateManager manager) : base(manager)
        {
        }

        public override void EnterState()
        {
            QuickLogger.Debug("======================================", true);
            QuickLogger.Debug("Entering Crafter Crafting State", true);
            _operation = _manager.GetOperation();
            _timeLeft = MAXTIME; //-1 was causing craft to start to fast
        }

        public override Type UpdateState()
        {
            if (_operation != null)
            {
                _timeLeft -= Time.deltaTime;
                if (_timeLeft < 0)
                {
                    if (TryCraft())
                    {
                        _timeLeft = 0;
                        _manager.Reset();
                        return typeof(CrafterIdleState);
                    }

                    _timeLeft = MAXTIME;
                }
            }

            return typeof(CrafterCraftingState);
        }

        private bool TryCraft()
        {
            
            QuickLogger.Debug("Trycraft", true);

            if (_operation != null && (_operation.IsComplete || IsLimitedCheck()))
            {
                return false;
            }


            if ( IsCraftRecipeFulfilledAdvanced(_operation.TechType) &&
                CheckIfLinkedItemsAllowed())
            {
                QuickLogger.Debug("Crafting", true);

                _manager.Crafter.CraftMachine.OnNeededItemFound?.Invoke();

                if (!_operation.OriginalBypass())
                {
                    CraftItem(_operation.TechType);
                }
                else
                {
                    Dictionary<TechType, int> dictionary = new Dictionary<TechType, int>();
                    Dictionary<TechType, int> crafted = new Dictionary<TechType, int>();

                    if (!_IsCraftRecipeFulfilledAdvanced(_operation.TechType, _operation.TechType, dictionary, crafted))
                    {
                        return false;
                    }

                    ConsumeIngredients(dictionary);
                    OnItemComplete();
                }

                CraftLinkedItems(_operation.TechType);

                if (!_operation.IsComplete) return false;

                CompleteOperation();

                return true;
            }

            QuickLogger.Debug("try craft failed", true);
            return false;
        }

        private bool IsLimitedCheck()
        {
            if (!_manager.Crafter.GetIsRecursive()) return false;
            return _manager.Crafter.CraftMachine.GetLimitAmount() > 0 &&
                   _manager.Crafter.GetIsLimitedOperation() && 
                   _manager.Crafter.Manager.GetItemCount(_operation.FixCustomTechType()) >=
                   _manager.Crafter.CraftMachine.GetLimitAmount();
        }

        private bool CheckIfLinkedItemsAllowed()
        {
            var totalAmount = _operation.ReturnAmount + _operation.LinkedItemCount * _operation.ReturnAmount;

            //TODO I need a way to check is the base can hold all the linked items and orginal item in storage and dss
            return _manager.Crafter.Manager.IsAllowedToAdd(_operation.FixCustomTechType(), _operation.ReturnAmount,
                true);
        }

        private void CraftLinkedItems(TechType techType)
        {
            if (_operation.LinkedItemCount > 0)
            {
                foreach (var dataLinkedItem in _operation.LinkedItems)
                {
                    QuickLogger.Debug("Crafting Linked Item", true);
                    Craft(dataLinkedItem, null);
                }
            }
        }

        private void CompleteOperation()
        {
            QuickLogger.Debug($"{_manager.Crafter.UnitID} Operation Complete.", true);

            _operation = null;
            _consumable?.Clear();
            _crafted?.Clear();
            AutocrafterHUD.Main.OnComplete();
            _manager.Crafter.CraftMachine.CancelOperation();
            _manager.Crafter.CancelLinkedCraftersOperations();
        }
        
        private void CraftItem(TechType techType)
        {
            if (techType == TechType.None) return;

            QuickLogger.Debug($"Crafting item: {Language.main.Get(techType)}", true);
            Dictionary<TechType, int> consumable = new Dictionary<TechType, int>();
            Dictionary<TechType, int> crafted = new Dictionary<TechType, int>();
            if (!_IsCraftRecipeFulfilledAdvanced(techType, techType, consumable, crafted))
            {
                return;
            }

            ConsumeIngredients(consumable);

            QuickLogger.Debug("Attempting to add item", true);

            bool result = false;

            for (int i = 0; i < _operation.ReturnAmount; i++)
            {
                CoroutineHost.StartCoroutine(Craft(techType, (result =>
                {
                    if (result)
                    {
                        OnItemComplete();
                    }
                })));
            }
        }

        private IEnumerator Craft(TechType techType, Action<bool> callback)
        {
            bool result = false;
            if (_manager.Crafter.Manager.IsAllowedToAdd(techType, 1, true))
            {
                QuickLogger.Debug("Item was allowed trying to add network", true);

                var pickupableTask = new TaskResult<bool>();
                yield return AttemptToAddToNetwork(techType, pickupableTask);
                result = pickupableTask.Get();
                QuickLogger.Debug($"Result : {result}", true);
            }
            else
            {
                QuickLogger.Debug("Item not allowed adding to storage", true);
                result = _manager.Crafter.AddItemToStorage(techType);
            }

            _manager.Crafter.CraftMachine.AppendItemToBelt(techType);

            callback?.Invoke(result);
        }


        private void OnItemComplete()
        {
            _operation.AppendCompletion();
            _manager.Crafter.CraftMachine.OnItemCrafted?.Invoke();
        }


        private void ConsumeIngredients(Dictionary<TechType, int> consumable)
        {
            QuickLogger.Debug("Consume Ingredients", true);
            if (UWEHelpers.RequiresIngredients())
            {
                QuickLogger.Debug("------------------------------------------------------------", true);
                foreach (KeyValuePair<TechType, int> valuePair in consumable)
                {
                    QuickLogger.Debug(
                        $"consumable includes: {Language.main.Get(valuePair.Key)} with the amount {valuePair.Value}",
                        true);
                }

                QuickLogger.Debug("------------------------------------------------------------", true);

                foreach (KeyValuePair<TechType, int> keyValuePair in consumable)
                {
                    if (keyValuePair.Value > 0)
                    {
                        QuickLogger.Debug($"Consuming {Language.main.Get(keyValuePair.Key)}", true);
                        _manager.Crafter.Manager.DestroyItemsFromBase(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                foreach (KeyValuePair<TechType, int> keyValuePair2 in consumable)
                {
                    if (keyValuePair2.Value < 0)
                    {
                        for (int i = 0; i < Mathf.Abs(keyValuePair2.Value); i++)
                        {
                            CoroutineHost.StartCoroutine(AttemptToAddToContainerAsync(keyValuePair2.Key));
                        }
                    }
                }
            }
        }

        private IEnumerator AttemptToAddToContainerAsync(TechType techType)
        {
            TaskResult<InventoryItem> taskResult = new TaskResult<InventoryItem>();
            yield return AsyncExtensions.ToInventoryItemAsync(techType, taskResult);
            var inventoryItem = taskResult.Get();
            if (inventoryItem != null)
            {
                _manager.Crafter.Manager.AddItemToContainer(inventoryItem);
            }
            else
            {
                QuickLogger.Error("Failed to add item to storage container");
            }

            yield break;
        }
        private IEnumerator AttemptToAddToNetwork(TechType techType, IOut<bool> boolResult)
        {
            QuickLogger.Debug($"Attempting to add {techType} to network", true);

            var itemTask = new TaskResult<InventoryItem>();
            yield return techType.ToInventoryItem(itemTask);

            var inventoryItem = itemTask.Get();
            QuickLogger.Debug($"InventoryItemLegacy returned: {Language.main.Get(inventoryItem.item.GetTechType())}");

            var result = BaseManager.AddItemToNetwork(_manager.Crafter.Manager, inventoryItem, true);

            if (!result)
            {
                _manager.Crafter.ShowMessage(
                    $"Failed to add {Language.main.Get(techType)} to storage. Please build a locker, remote storage or add more space to your data storage system. Your item will be added to the autocrafter storage/");
                _manager.Crafter.AddItemToStorage(techType);
                GameObject.Destroy(inventoryItem.item.gameObject);
            }

            boolResult.Set(true);
            yield break;
        }

        #region EasyCraft Code

        public bool IsCraftRecipeFulfilledAdvanced(TechType techType)
        {
            if (Inventory.main == null)
            {
                return false;
            }

            _availableItems.Clear();
            _recipeItems.Clear();
            _toCraftItems.Clear();
            GetRequiredItems(techType);

            if (!UWEHelpers.RequiresIngredients())
            {
                return true;
            }

            _consumable = new Dictionary<TechType, int>();
            _crafted = new Dictionary<TechType, int>();
            return _IsCraftRecipeFulfilledAdvanced(techType, techType, _consumable, _crafted, 0);
        }

        private bool _IsCraftRecipeFulfilledAdvanced(TechType parent, TechType techType,
            Dictionary<TechType, int> consumable, Dictionary<TechType, int> crafted, int depth = 0)
        {
            if (depth >= 5)
            {
                return false;
            }

            var techData = CraftDataHandler.GetTechData(techType);

            if (techData != null)
            {
                crafted.Inc(techType);

                int i = 0;
                var ingredientCount = techData.ingredientCount;

                while (i < ingredientCount)
                {
                    var ingredient = techData.GetIngredient(i);

                    TechType ingredientTechType = ingredient.techType;

                    if (parent == ingredientTechType)
                    {
                        return false;
                    }

                    int pickupCount = /*_manager.Crafter.Storage.container.GetCount(ingredientTechType);*/
                        _manager.Crafter.Manager.GetItemCount(ingredientTechType);

                    int consumableTechTypeCount =
                        consumable.ContainsKey(ingredientTechType) ? consumable[ingredientTechType] : 0;

                    int amountRemainder = Mathf.Max(0, pickupCount - consumableTechTypeCount);

                    int ingredientAmount = ingredient.amount;

                    if (amountRemainder < ingredientAmount)
                    {
                        //if (!CheckIfInvalidEquipmentType(ingredientTechType)) return false;

                        QuickLogger.Debug(
                            $"PT1 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {amountRemainder}");

                        consumable.Inc(ingredientTechType, amountRemainder);

                        for (int j = 0; j < ingredientAmount - amountRemainder; j++)
                        {
                            if (!_IsCraftRecipeFulfilledAdvanced(parent, ingredientTechType, consumable, crafted,
                                    depth + 1))
                            {
                                return false;
                            }

                            var ingredientTechTypeTechData = CraftDataHandler.GetTechData(ingredientTechType);

                            if (ingredientTechTypeTechData != null)
                            {
                                j += ingredientTechTypeTechData.craftAmount - 1;

                                QuickLogger.Debug(
                                    $"PT2 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {-ingredientTechTypeTechData.craftAmount}");
                                consumable.Inc(ingredientTechType, -ingredientTechTypeTechData.craftAmount);

                                if (ingredientTechTypeTechData.linkedItemCount > 0)
                                {
                                    for (int k = 0; k < ingredientTechTypeTechData.linkedItemCount; k++)
                                    {
                                        TechType linkedItem = ingredientTechTypeTechData.GetLinkedItem(k);
                                        QuickLogger.Debug(
                                            $"PT3 | Adding Consumable: {Language.main.Get(linkedItem)} | Amount: -1");
                                        consumable.Inc(linkedItem, -1);
                                    }
                                }
                            }
                        }

                        QuickLogger.Debug(
                            $"PT4 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {ingredientAmount - amountRemainder}");
                        consumable.Inc(ingredientTechType, ingredientAmount - amountRemainder);
                        consumableTechTypeCount = (consumable.ContainsKey(ingredientTechType)
                            ? consumable[ingredientTechType]
                            : 0);
                        if (pickupCount < consumableTechTypeCount)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        QuickLogger.Debug(
                            $"PT5 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {ingredient.amount}");

                        consumable.Inc(ingredientTechType, ingredient.amount);
                    }

                    i++;
                }

                return true;
            }

            return false;
        }

        private bool _IsCraftRecipeFulfilledAdvanced1(TechType parent, TechType techType,
            Dictionary<TechType, int> consumable, Dictionary<TechType, int> crafted, int depth = 0,
            int amountToCraft = 1)
        {
            for (int l = 0; l < amountToCraft; l++)
            {
                if (depth >= 5)
                {
                    return false;
                }

                var techData = CraftDataHandler.GetTechData(techType);

                if (techData != null)
                {
                    crafted.Inc(techType);

                    int i = 0;
                    var ingredientCount = techData.ingredientCount;

                    while (i < ingredientCount)
                    {
                        var ingredient = techData.GetIngredient(i);

                        TechType ingredientTechType = ingredient.techType;

                        if (parent == ingredientTechType)
                        {
                            return false;
                        }

                        int pickupCount = _manager.Crafter.Manager.GetItemCount(ingredientTechType);

                        int consumableTechTypeCount = consumable.ContainsKey(ingredientTechType)
                            ? consumable[ingredientTechType]
                            : 0;

                        int amountRemainder = Mathf.Max(0, pickupCount - consumableTechTypeCount);

                        int ingredientAmount = ingredient.amount * amountRemainder;

                        if (amountRemainder < ingredientAmount)
                        {
                            //if (!CheckIfInvalidEquipmentType(ingredientTechType)) return false;

                            QuickLogger.Debug(
                                $"PT1 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {amountRemainder}");

                            consumable.Inc(ingredientTechType, amountRemainder);

                            for (int j = 0; j < ingredientAmount - amountRemainder; j++)
                            {
                                if (!_IsCraftRecipeFulfilledAdvanced(parent, ingredientTechType, consumable, crafted,
                                        depth + 1))
                                {
                                    return false;
                                }

                                var ingredientTechTypeTechData = CraftDataHandler.GetTechData(ingredientTechType);

                                if (ingredientTechTypeTechData != null)
                                {
                                    j += ingredientTechTypeTechData.craftAmount - 1;

                                    QuickLogger.Debug(
                                        $"PT2 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {-ingredientTechTypeTechData.craftAmount}");
                                    consumable.Inc(ingredientTechType, -ingredientTechTypeTechData.craftAmount);

                                    if (ingredientTechTypeTechData.linkedItemCount > 0)
                                    {
                                        for (int k = 0; k < ingredientTechTypeTechData.linkedItemCount; k++)
                                        {
                                            TechType linkedItem = ingredientTechTypeTechData.GetLinkedItem(k);
                                            QuickLogger.Debug(
                                                $"PT3 | Adding Consumable: {Language.main.Get(linkedItem)} | Amount: -1");
                                            consumable.Inc(linkedItem, -1);
                                        }
                                    }
                                }
                            }

                            QuickLogger.Debug(
                                $"PT4 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {ingredientAmount - amountRemainder}");
                            consumable.Inc(ingredientTechType, ingredientAmount - amountRemainder);
                            consumableTechTypeCount = (consumable.ContainsKey(ingredientTechType)
                                ? consumable[ingredientTechType]
                                : 0);
                            if (pickupCount < consumableTechTypeCount)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            QuickLogger.Debug(
                                $"PT5 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {ingredient.amount}");

                            consumable.Inc(ingredientTechType, ingredient.amount);
                        }

                        i++;
                    }
                }

                return true;
            }

            return false;
        }

        private void GetRequiredItems(TechType techType)
        {
            var techData = CraftDataHandler.GetTechData(techType);

            if (techData != null)
            {
                for (int i = 0; i < techData.ingredientCount; i++)
                {
                    var ingredient = techData.GetIngredient(i);
                    var baseIngredientCount = _manager.Crafter.Manager.GetItemCount(ingredient.techType);
                    _recipeItems.Inc(ingredient.techType, ingredient.amount);


                    if (baseIngredientCount > 0)
                    {
                        if (_availableItems.ContainsKey(ingredient.techType)) continue;
                        _availableItems.Inc(ingredient.techType, ingredient.amount);
                    }
                    else
                    {
                        _toCraftItems.Inc(ingredient.techType, ingredient.amount);
                    }

                    if (CraftDataHandler.GetTechData(ingredient.techType) != null)
                    {
                        GetRequiredItems(ingredient.techType);
                    }
                }
            }
        }

        private static bool CheckIfInvalidEquipmentType(TechType ingredientTechType)
        {
#if SUBNAUTICA
            EquipmentType equipmentType = CraftData.GetEquipmentType(ingredientTechType);
#else
            EquipmentType equipmentType = TechData.GetEquipmentType(ingredientTechType);
#endif
            if (equipmentType == EquipmentType.Body || equipmentType == EquipmentType.Chip ||
                equipmentType == EquipmentType.CyclopsModule || equipmentType == EquipmentType.ExosuitModule ||
                equipmentType == EquipmentType.Foots || equipmentType == EquipmentType.Gloves ||
                equipmentType == EquipmentType.Head || equipmentType == EquipmentType.SeamothModule ||
                equipmentType == EquipmentType.Tank || equipmentType == EquipmentType.VehicleModule)
            {
                return false;
            }

            return true;
        }

        #endregion

        public Dictionary<TechType, int> GetConsumables()
        {
            var ingredients = new Dictionary<TechType, int>();

            var data = CraftDataHandler.GetTechData(_operation.TechType);

            if (data != null)
            {
                for (int i = 0; i < data.ingredientCount; i++)
                {
                    var ingredient = data.GetIngredient(i);

                    ingredients.Add(ingredient.techType, ingredient.amount);
                }
            }

            return ingredients;
        }

        public bool IsIngredientFulfilled(TechType techType, out Dictionary<TechType, int> requirements)
        {
            requirements = new Dictionary<TechType, int>();
            if (_operation == null)
            {
                return false;
            }

            var consumable = new Dictionary<TechType, int>();
            var crafted = new Dictionary<TechType, int>();
            requirements = consumable;
            return _IsCraftRecipeFulfilledAdvanced(techType, techType, consumable, crafted, 0);
        }
    }
}