using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class DSSAutoCrafterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSAutoCrafterDataEntry _saveData;
        private DSSAutoCrafterDisplay _displayManager;
        private bool _hasBreakTripped;
        internal  DSSCraftManager CraftManager;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
            if (Mod.Craftables.Count == 0)
            {
               var fabricator =  CraftTree.GetTree(CraftTree.Type.Fabricator);
               GetCraftTreeData(fabricator.nodes);
               foreach (TechType craftable in Mod.Craftables)
               {
                   QuickLogger.Debug($"Craftable: {Language.main.Get(craftable)} was added.");
               }
            }
            _displayManager.RefreshCraftables();
        }

        private void GetCraftTreeData(CraftNode innerNodes)
        {
            foreach (CraftNode craftNode in innerNodes)
            {
                QuickLogger.Debug($"Craftable: {craftNode.string0} | {craftNode.string1} | {craftNode.techType0}");

                if (craftNode.techType0 != TechType.None)
                {
                    Mod.Craftables.Add(craftNode.techType0);
                }

                if (craftNode.childCount > 0)
                {
                    GetCraftTreeData(craftNode);
                }
            }
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
                _colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSAutoCrafterSaveData(id);
        }

        public override void Initialize()
        {
            if (_displayManager == null)
            {
                _displayManager = gameObject.EnsureComponent<DSSAutoCrafterDisplay>();
                _displayManager.Setup(this);
            }

            if (CraftManager == null)
            {
                CraftManager = gameObject.EnsureComponent<DSSCraftManager>();
                CraftManager.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            IsInitialized = true;

            QuickLogger.Debug($"Initialized - {GetPrefabID()}");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSAutoCrafterFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSAutoCrafterFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            try
            {
                var prefabIdentifier = GetComponent<PrefabIdentifier>();
                var id = prefabIdentifier.Id;

                if (_saveData == null)
                {
                    _saveData = new DSSAutoCrafterDataEntry();
                }

                _saveData.ID = id;
                _saveData.Body = _colorManager.GetColor().ColorToVector4();
                _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();

                newSaveData.DSSAutoCrafterDataEntries.Add(_saveData);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Failed to save {UnitID}:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.InnerException);
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
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

        public bool ToggleBreaker()
        {
            _hasBreakTripped = !_hasBreakTripped;
            return _hasBreakTripped;
        }
    }

    internal class DSSCraftManager : MonoBehaviour
    {
        public void Initialize(DSSAutoCrafterController mono)
        {
            
        }

        public void StartOperation()
        {

        }

        public void StopOperation()
        {
            
        }
    }
}