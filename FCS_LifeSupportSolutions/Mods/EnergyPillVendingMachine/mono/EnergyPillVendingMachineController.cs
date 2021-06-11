using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono
{
    internal class EnergyPillVendingMachineController : FcsDevice, IFCSSave<SaveData>
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

                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
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
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (_displayManager == null)
            {
                _displayManager = gameObject.AddComponent<EnergyPillVendingMachineDisplay>();
                _displayManager.Setup(this);
            }

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
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.EnergyPillVendingMachineEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetEnergyPillVendingMachineSaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
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
    }
}
