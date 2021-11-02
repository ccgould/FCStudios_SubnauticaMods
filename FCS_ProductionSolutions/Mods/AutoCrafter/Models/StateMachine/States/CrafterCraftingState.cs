using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Mods.AutoCrafter.Helpers;
using FCS_ProductionSolutions.Mods.AutoCrafter.Patches;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States
{
    internal class CrafterCraftingState : CrafterBaseState
    {
        [JsonProperty] internal  CraftingOperation _operation;
        [JsonProperty] internal float _timeLeft;
        [JsonProperty] internal Dictionary<TechType, int> _consumable;
        [JsonProperty] internal Dictionary<TechType, int> _crafted;
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
            QuickLogger.Debug("======================================",true);
            QuickLogger.Debug("Entering Crafter Crafting State",true);
            _operation = _manager.GetOperation();
            _timeLeft = -1;
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
            QuickLogger.Debug("Trycraft",true);

            if (_operation != null && IsCraftRecipeFulfilledAdvanced(_operation.TechType) &&  _manager.Crafter.Manager.IsAllowedToAdd(_operation.FixCustomTechType(),_operation.ReturnAmount,true))
            {
                QuickLogger.Debug("Crafting", true);

                _manager.Crafter.CraftMachine.OnNeededItemFound?.Invoke();

                CraftItem(_operation.TechType, true);

                if (!_operation.IsComplete) return false;
                
                CompleteOperation(_operation);
                
                return true;
            }

            QuickLogger.Debug("try craft failed", true);
            return false;
        }

        private void CompleteOperation(CraftingOperation craftingOperation)
        {
            QuickLogger.Debug("Operation Complete.",true);
            _operation = null;
            _consumable?.Clear();
            _crafted?.Clear();
            //_manager?.Crafter?.CraftMachine?.OnComplete?.Invoke(craftingOperation);
            AutocrafterHUD.Main.OnComplete(craftingOperation);
        }

        private IEnumerator SpawnItems()
        {
            foreach (KeyValuePair<TechType, int> keyValuePair in _consumable)
            {
                
            }
            _manager.Crafter.CrafterBelt.SpawnBeltItem(_operation.FixCustomTechType());
            yield return new WaitForSeconds(MAXTIME);
            _manager.Crafter.CrafterBelt.SpawnBeltItem(_operation.FixCustomTechType());
            yield break;
        }

        private void CraftItem(TechType techType, bool isComplete)
        {
            bool result = false;
            QuickLogger.Debug($"Crafting item: {Language.main.Get(techType)}",true);
            Dictionary<TechType, int> dictionary = new Dictionary<TechType, int>();
            Dictionary<TechType, int> crafted = new Dictionary<TechType, int>();
            if (!_IsCraftRecipeFulfilledAdvanced(techType, techType, dictionary, crafted))
            {
                return; 
            }

            ConsumeIngredients(dictionary);

            if (isComplete)
            {
                QuickLogger.Debug("Attempting to add item", true);
                for (int i = 0; i < _operation.ReturnAmount; i++)
                {
                    if (_manager.Crafter.Manager.IsAllowedToAdd(techType, 1, true))
                    {
                        QuickLogger.Debug("Item was allowed trying to add network", true);
                        result = AttemptToAddToNetwork(techType);
                        QuickLogger.Debug($"Result : {result}", true);
                    }
                    else
                    {
                        QuickLogger.Debug("Item not allowed adding to storage", true);
                        result = _manager.Crafter.AddItemToStorage(techType);
                    }
                }

                if(result)
                    _operation.AppendCompletion();

            }
            else
            {
                _manager.Crafter.Storage.container.UnsafeAdd(techType.ToInventoryItem());
                result = true;
            }

            if (result)
            {
                //CoroutineHost.StartCoroutine(SpawnItems());
                _manager.Crafter.CrafterBelt.SpawnBeltItem(_operation.FixCustomTechType());
                _manager.Crafter.CraftMachine.OnItemCrafted?.Invoke();
            }
        }

        private void ConsumeIngredients(Dictionary<TechType, int> consumable)
        {
            QuickLogger.Debug("Consume Ingredients",true);
            if (GameModeUtils.RequiresIngredients())
            {
                QuickLogger.Debug("------------------------------------------------------------",true);
                foreach (KeyValuePair<TechType, int> valuePair in consumable)
                {
                    QuickLogger.Debug($"consumable includes: {Language.main.Get(valuePair.Key)} with the amount {valuePair.Value}",true);
                }
                QuickLogger.Debug("------------------------------------------------------------", true); 
                
                foreach (KeyValuePair<TechType, int> keyValuePair in consumable)
                {
                    if (keyValuePair.Value > 0)
                    {
                        QuickLogger.Debug($"Consuming {Language.main.Get(keyValuePair.Key)}",true);
                        _manager.Crafter.Manager.DestroyItemsFromBase(keyValuePair.Key, keyValuePair.Value);
                    }
                }
                foreach (KeyValuePair<TechType, int> keyValuePair2 in consumable)
                {
                    if (keyValuePair2.Value < 0)
                    {
                        for (int i = 0; i < Mathf.Abs(keyValuePair2.Value); i++)
                        {
                            _manager.Crafter.Manager.AddItemToContainer(keyValuePair2.Key.ToInventoryItem());
                        }
                    }
                }
            }
        }

        private bool AttemptToAddToNetwork(TechType techType)
        {
            var inventoryItem = techType.ToInventoryItem();

            QuickLogger.Debug($"InventoryItemLegacy returned: {Language.main.Get(inventoryItem.item.GetTechType())}");

            var result = BaseManager.AddItemToNetwork(_manager.Crafter.Manager, inventoryItem, true);

            if (!result)
            {
                _manager.Crafter.ShowMessage($"Failed to add {Language.main.Get(techType)} to storage. Please build a locker, remote storage or add more space to your data storage system. Your item will be added to the autocrafter storage/");
                _manager.Crafter.AddItemToStorage(techType);
                GameObject.Destroy(inventoryItem.item.gameObject);
            }

            return result;
        }
        
        #region EasyCraft Code

        public bool IsCraftRecipeFulfilledAdvanced(TechType techType)
        {
            if (Inventory.main == null)
            {
                return false;
            }
            if (!GameModeUtils.RequiresIngredients())
            {
                return true;
            }
            
            _consumable = new Dictionary<TechType, int>();
            _crafted = new Dictionary<TechType, int>();
            return _IsCraftRecipeFulfilledAdvanced(techType, techType, _consumable, _crafted, 0);
        }

        private bool _IsCraftRecipeFulfilledAdvanced(TechType parent, TechType techType, Dictionary<TechType, int> consumable, Dictionary<TechType, int> crafted, int depth = 0)
        {
            if (depth >= 5)
            {
                return false;
            }
            ITechData techData = CraftData.Get(techType, true);

            if (techData != null)
            {
                crafted.Inc(techType);

                int i = 0;
                var ingredientCount = techData.ingredientCount;

                while (i < ingredientCount)
                {
                    IIngredient ingredient = techData.GetIngredient(i);

                    TechType ingredientTechType = ingredient.techType;
                    
                    if (parent == ingredientTechType)
                    {
                        return false;
                    }

                    int pickupCount = /*_manager.Crafter.Storage.container.GetCount(ingredientTechType);*/ _manager.Crafter.Manager.GetItemCount(ingredientTechType);
                    
                    int consumableTechTypeCount = consumable.ContainsKey(ingredientTechType) ? consumable[ingredientTechType] : 0;
                    
                    int amountRemainder = Mathf.Max(0, pickupCount - consumableTechTypeCount);
                    
                    int ingredientAmount = ingredient.amount;
                    
                    if (amountRemainder < ingredientAmount)
                    {
                        if (!CheckIfInvalidEquipmentType(ingredientTechType)) return false;

                        QuickLogger.Debug($"PT1 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {amountRemainder}");

                        consumable.Inc(ingredientTechType, amountRemainder);

                        for (int j = 0; j < ingredientAmount - amountRemainder; j++)
                        {
                            if (!_IsCraftRecipeFulfilledAdvanced(parent, ingredientTechType, consumable, crafted, depth + 1))
                            {
                                return false;
                            }
                            
                            ITechData ingredientTechTypeTechData = CraftData.Get(ingredientTechType, true);

                            if (ingredientTechTypeTechData != null)
                            {
                                j += ingredientTechTypeTechData.craftAmount - 1;
                                
                                QuickLogger.Debug($"PT2 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {-ingredientTechTypeTechData.craftAmount}");
                                consumable.Inc(ingredientTechType, -ingredientTechTypeTechData.craftAmount);

                                if (ingredientTechTypeTechData.linkedItemCount > 0)
                                {
                                    for (int k = 0; k < ingredientTechTypeTechData.linkedItemCount; k++)
                                    {
                                        TechType linkedItem = ingredientTechTypeTechData.GetLinkedItem(k);
                                        QuickLogger.Debug($"PT3 | Adding Consumable: {Language.main.Get(linkedItem)} | Amount: -1");
                                        consumable.Inc(linkedItem, -1);
                                    }
                                }
                            }
                        }
                        QuickLogger.Debug($"PT4 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {ingredientAmount - amountRemainder}");
                        consumable.Inc(ingredientTechType, ingredientAmount - amountRemainder);
                        consumableTechTypeCount = (consumable.ContainsKey(ingredientTechType) ? consumable[ingredientTechType] : 0);
                        if (pickupCount < consumableTechTypeCount)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        QuickLogger.Debug($"PT5 | Adding Consumable: {Language.main.Get(ingredientTechType)} | Amount: {ingredient.amount}");

                        consumable.Inc(ingredientTechType, ingredient.amount);
                    }
                    i++;
                }
                return true;
            }
            return false;
        }

        private static bool CheckIfInvalidEquipmentType(TechType ingredientTechType)
        {
            EquipmentType equipmentType = CraftData.GetEquipmentType(ingredientTechType);
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
            if (_operation == null) return null;
            var consumable = new Dictionary<TechType, int>();
            var crafted = new Dictionary<TechType, int>();
            _IsCraftRecipeFulfilledAdvanced(_operation.TechType, _operation.TechType, consumable, crafted, 0);
            return consumable;
        }
    }
}
