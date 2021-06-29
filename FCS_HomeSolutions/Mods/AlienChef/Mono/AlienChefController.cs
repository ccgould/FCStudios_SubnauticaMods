﻿using System.Collections.Generic;
using System.Reflection;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class AlienChefController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private AlienChiefDataEntry _saveData;
        private InterfaceInteraction _interactionHelper;
        const int MAXSLOTS = 48;
        internal DisplayManager DisplayManager { get; private set; }
        internal Cooker Cooker { get; set; }
        internal FCSStorage StorageSystem { get; set; }
        public bool PullFromDataStorage { get; set; }
        public bool IsSendingToSeaBreeze { get; set; }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlienChiefTabID, Mod.ModPackID);
            DisplayManager?.UpdateStorageAmount(StorageSystem?.GetCount()??0);
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.ChangeColor(_saveData.Fcs.Vector4ToColor());
                RefreshUI();
                _fromSave = false;
            }
        }

        public override float GetPowerUsage()
        {
            return Cooker.IsCooking ? 0.05f : 0f;
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (Cooker == null)
            {
                Cooker = gameObject.EnsureComponent<Cooker>();
                Cooker.Initialize(this);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.EnsureComponent<DisplayManager>();
                DisplayManager.Setup(this);
            }

            if (StorageSystem == null)
            {
                StorageSystem = gameObject.GetComponent<FCSStorage>();
                StorageSystem.NotAllowedToAddItems = true;
                StorageSystem.SlotsAssigned = MAXSLOTS;
                StorageSystem.Deactivate();
                StorageSystem.ItemsContainer.onAddItem += type =>
                {
                    DisplayManager.UpdateStorageAmount(StorageSystem.GetCount());
                };
                StorageSystem.ItemsContainer.onRemoveItem += type =>
                {
                    DisplayManager.UpdateStorageAmount(StorageSystem.GetCount());
                };
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            Mod.GetFoodCustomTrees();

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial, gameObject, 5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetAlienChiefSaveData(id);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.AlienChefFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.AlienChefFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }
            
#if SUBNAUTICA_STABLE
            //StorageSystem.RestoreItems(serializer, _saveData.Bytes);
#else
            //StartCoroutine(StorageSystem.RestoreItemsync(serializer, _saveData.Bytes));

#endif

            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {

                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new AlienChiefDataEntry();
            }

            _saveData.Id = id;
            _saveData.Fcs = _colorManager.GetColor().ColorToVector4();
            //_saveData.Bytes = StorageSystem.Save(serializer);
            newSaveData.AlienChiefDataEntries.Add(_saveData);
        }

        internal void SendToSeaBreeze(InventoryItem inventoryItem)
        {
            var seabreezes = Manager.GetDevices(Mod.SeaBreezeTabID);
            foreach (FcsDevice device in seabreezes)
            {
                if (!device.IsConstructed) continue;
                if (device.CanBeStored(1, inventoryItem.item.GetTechType()))
                {
                    device.AddItemToContainer(inventoryItem);
                }
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public bool HasPowerToConsume()
        {
            return Manager.HasEnoughPower(GetPowerUsage());
        }

        public bool AttemptToCook(CookerItemController dialog, int amount, bool autoStartCooking = true)
        {
            if(StorageSystem.IsFull) return false;

            //Check for ingredients
            if (dialog.CheckForIngredients(amount))
            {
                for (int i = 0; i < amount; i++)
                {
                    //Consume all ingredients
                    foreach (KeyValuePair<TechType, int> ingredient in dialog.GetIngredients())
                    {
                        int target = ingredient.Value * amount;

                        for (int i1 = 0; i1 < ingredient.Value * amount; i1++)
                        {
                            if (PullFromDataStorage)
                            {
                                if (Inventory.main.container.Contains(ingredient.Key) && target != 0)
                                {
                                    Destroy(Inventory.main.container.RemoveItem(ingredient.Key).gameObject);
                                    target--;
                                }

                                if (target == 0) break;

                                if (Manager.HasItem(ingredient.Key))
                                {
                                    Destroy(Manager.TakeItem(ingredient.Key).gameObject);
                                    target--;
                                }

                                if (target == 0) break;
                            }
                            else
                            {
                                if (Inventory.main.container.Contains(ingredient.Key) && target != 0)
                                {
                                    Destroy(Inventory.main.container.RemoveItem(ingredient.Key).gameObject);
                                    target--;
                                }
                                if (target == 0) break;
                            }
                        }
                    }

                    //Add Craft
                    Cooker.AddToQueue(dialog.CookingItem);
                }

                if (autoStartCooking)
                {
                    Cooker.StartCooking();
                }
                return true;
            }
            
            QuickLogger.ModMessage($"Alien Chef cant find all the required ingredients for this craft");
            return false;
        }

        public void AddToOrder(CookerItemController cookerItemDialog, int amount)
        {
            DisplayManager.AddToOrder(cookerItemDialog, amount);
        }

        public override void OnHandHover(GUIHand hand)
        {
            if(!IsInitialized || !IsConstructed  || _interactionHelper.IsInRange) return;
            base.OnHandHover(hand);
            
            var data = new[]
            {
                AlterraHub.PowerPerMinute(GetPowerUsage() * 60)
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            //Not In Use
        }
    }
}