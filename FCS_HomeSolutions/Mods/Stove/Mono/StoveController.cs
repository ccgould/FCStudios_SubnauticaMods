using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.SeaBreeze.Buildable;
using FCS_HomeSolutions.Mods.Stove.Buildable;
using FCS_HomeSolutions.Mods.Stove.Struct;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.Stove.Mono
{
    internal class StoveController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private StoveDataEntry _saveData;
        private static HashSet<TechType> _allowedTech;
        private static Dictionary<TechType, CookingItem> _knownCookingData = new();
        private Text _clock;
        internal Cooker Cooker { get; set; }
        internal FCSStorage StorageSystem { get; set; }
        public bool IsSendingToSeaBreeze { get; set; }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, StoveBuildable.StoveTabID, Mod.ModPackID);
        }

        private void Update()
        {
            if (_clock != null)
                _clock.text = WorldHelpers.GetGameTimeFormat();
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
                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                Cooker.Load(_saveData.QueuedItems);
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
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            if (Cooker == null)
            {
                Cooker = gameObject.EnsureComponent<Cooker>();
                Cooker.Initialize(this);
            }

            if (StorageSystem == null)
            {
                StorageSystem = gameObject.GetComponent<FCSStorage>();
                StorageSystem.SlotsAssigned = 16;

                StorageSystem.OnContainerOpened += () =>
                {
                    if (StorageSystem.container != null && (StorageSystem.container.allowedTech == null || !StorageSystem.container.allowedTech.Any()))
                    {
                        StorageSystem.container.allowedTech = GetAllowedTech();
                    }
                };

                StorageSystem.OnContainerClosed += () =>
                {
                    QuickLogger.Debug("On Container Closed.", true);
                    Cooker.StartCooking();
                };
            }
            _clock = GameObjectHelpers.FindGameObject(gameObject, "Time").GetComponent<Text>();


            Mod.GetFoodCustomTrees();
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private static HashSet<TechType> GetAllowedTech()
        {
            if (_allowedTech == null)
            {
                _allowedTech = new HashSet<TechType>();

#if SUBNAUTICA
                foreach (var item in CraftData.cookedCreatureList)
                {
                    _allowedTech.AddIfNotPresent(item.Key);
                }
#else
                foreach (TechType techType in Enum.GetValues(typeof(TechType)))
                {
                    if (TechData.GetProcessed(techType) != TechType.None)
                        _allowedTech.AddIfNotPresent(techType);
                }
#endif
                foreach (var item in Mod.CuredCreatureList)
                {
                    _allowedTech.AddIfNotPresent(item.Key);
                }

                foreach (var item in Mod.CustomFoods)
                {
                    _allowedTech.AddIfNotPresent(item.Key);
                }

            }

            return _allowedTech;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetStoveSaveData(id);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {StoveBuildable.StoveFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {StoveBuildable.StoveFriendly}");
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
                _saveData = new StoveDataEntry();
            }

            _saveData.Id = id;
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
            _saveData.QueuedItems = Cooker.Save();
            newSaveData.StoveDataEntries.Add(_saveData);
        }

        internal IEnumerator SendToSeaBreeze(TechType techType)
        {
            var seabreezes = Manager.GetDevices(SeaBreezeBuildable.SeaBreezeTabID);
            foreach (FcsDevice device in seabreezes)
            {
                if (!device.IsConstructed) continue;
                if (device.CanBeStored(1, techType))
                {
#if SUBNAUTICA_STABLE
                    device.AddItemToContainer(techType.ToInventoryItem());
                    yield break;
#else
                    var itemTask = new TaskResult<InventoryItem>();
                    yield return techType.ToInventoryItem(itemTask);
                    device.AddItemToContainer(itemTask.Get());
#endif
                }
            }
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public bool HasPowerToConsume()
        {
            return Manager.HasEnoughPower(GetPowerUsage());
        }
        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;
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

        public static CookingItem GetCookingItemData(TechType techType)
        {
            if (_knownCookingData.ContainsKey(techType))
            {
                return _knownCookingData[techType];
            }

#if SUBNAUTICA 
            TechType foodData = TechType.None;
            if (CraftData.cookedCreatureList.ContainsKey(techType))
            {
                foodData = CraftData.cookedCreatureList[techType];
#else

            var foodData = TechData.GetProcessed(techType);
            if (foodData != TechType.None)
            {
#endif
                var newCookingData = new CookingItem { TechType = techType, ReturnItem = foodData };
                _knownCookingData.Add(techType, newCookingData);
                return newCookingData;
            }


            if (Mod.CuredCreatureList.ContainsKey(techType))
            {
                foodData = Mod.CuredCreatureList[techType];
                var newCookingData = new CookingItem { TechType = techType, ReturnItem = foodData };
                _knownCookingData.Add(techType, newCookingData);
                return newCookingData;
            }

            if (Mod.CustomFoods.ContainsKey(techType))
            {
                foodData = Mod.CustomFoods[techType];
                var newCookingData = new CookingItem { TechType = techType, ReturnItem = foodData };
                _knownCookingData.Add(techType, newCookingData);
                return newCookingData;
            }

            return new CookingItem();

        }
    }
}