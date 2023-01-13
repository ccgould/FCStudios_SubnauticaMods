using System;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono
{
    internal class EnergyPillVendingMachineController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private EnergyPillVendingMachineEntry _savedData;
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private EnergyPillVendingMachineDisplay _displayManager;
        internal Dictionary<string, Tuple<decimal,TechType>> Inventory = new Dictionary<string, Tuple<decimal, TechType>>
        {
            {"540",new Tuple<decimal, TechType>(2000000,Mod.RedEnergyPillTechType) },
            {"670",new Tuple<decimal, TechType>(1000000,Mod.BlueEnergyPillTechType) },
            {"242",new Tuple<decimal, TechType>(500000,Mod.GreenEnergyPillTechType) }
        };

        private InterfaceInteraction _interactionHelper;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.EnergyPillVendingMachineTabID, Mod.ModPackID);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override float GetPowerUsage()
        {
            return 0.01f;
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol,AlterraHub.BaseSecondaryCol);
            }

            if (_displayManager == null)
            {
                _displayManager = gameObject.AddComponent<EnergyPillVendingMachineDisplay>();
                _displayManager.Setup(this);
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);

            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
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

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new EnergyPillVendingMachineEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();

            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.EnergyPillVendingMachineEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetEnergyPillVendingMachineSaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public void PurchaseItem(string itemKey)
        {
            if (Inventory.ContainsKey(itemKey))
            {
                if (!PlayerInteractionHelper.HasCard())
                {
                    _displayManager.ForceUpdateDisplay(AuxPatchers.NoCard());
                    return;
                }

                if (FCSAlterraHubService.PublicAPI.IsCreditAvailable(Inventory[itemKey].Item1))
                {
                    var result = PlayerInteractionHelper.GivePlayerItem(Inventory[itemKey].Item2);
                    if (result)
                    {
                        FCSAlterraHubService.PublicAPI.ChargeAccount(Inventory[itemKey].Item1);
                    }
                    else
                    {
                        QuickLogger.ModMessage(AuxPatchers.NoInventorySpace());
                    }
                }
                else
                {
                    _displayManager.ForceUpdateDisplay(AuxPatchers.NotEnoughCredit());
                }
            }
            else
            {
                _displayManager.ForceUpdateDisplay(AuxPatchers.InvalidItem());
            }
        }

        public bool TryGetPrice(string itemKey,out decimal cost)
        {
            cost = 0;
            if (Inventory.ContainsKey(itemKey))
            {
                cost = Inventory[itemKey].Item1;
                return true;
            }
            return false;
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _interactionHelper.IsInRange) return;
            
            base.OnHandHover(hand);

            var data = new[]
            {
               AlterraHub.PowerPerMinute(GetPowerUsage() * 60)
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            // Not in use
        }
    }
}
