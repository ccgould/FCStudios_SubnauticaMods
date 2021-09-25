using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Buildable;
using FCS_ProductionSolutions.Mods.AutoCrafter.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Mono
{

    /// <summary>
    /// Controller for the Autocrafter handles initializations, loading from saves and main controls.
    /// </summary>
    internal class AutoCrafterController : FcsDevice, IFCSSave<SaveData>
    {
        private IEnumerable<Material> _materials;
        private const float _beltSpeed = 0.01f;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private float _transferTimer;
        private List<TechType> _storedItems = new List<TechType>();
        private AutoCrafterDataEntry _saveData;
        private bool _moveBelt;

        #region Unity Methods

        private void Update()
        {
            if (WorldHelpers.CheckIfPaused()) return;
            MoveBeltMaterial();

            if (_transferTimer >= 1f)
            {
                for (int i = _storedItems.Count - 1; i >= 0; i--)
                {

                    CompatibilityMethods.AttemptToAddToNetwork(_storedItems[i], Manager, _storedItems);
                }

                _transferTimer = 0f;
            }

        }
        public bool IsBeltMoving()
        {
            return _moveBelt;
        }
        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, AutoCrafterPatch.AutoCrafterTabID, Mod.ModPackID);
            _materials = MaterialHelpers.GetMaterials(GameObjectHelpers.FindGameObject(gameObject, "ConveyorBelts"), AlterraHub.BaseOpaqueInterior);

            if (Manager != null)
            {
                Manager.OnPowerStateChanged += OnPowerStateChanged;
                OnPowerStateChanged(Manager.GetPowerState());
                Manager.NotifyByID(AutoCrafterPatch.AutoCrafterTabID, "RefreshAutoCrafterList");
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
                _colorManager.LoadTemplate(_saveData.ColorTemplate);

                if (_saveData.StoredItems != null)
                {
                    _storedItems = _saveData.StoredItems;
                }
                _fromSave = false;
            }
        }

        public override void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
                Manager.NotifyByID(AutoCrafterPatch.AutoCrafterTabID, "RefreshAutoCrafterList");
            }
            base.OnDestroy();
        }
        
        #endregion

        #region FCS Overrides

        public override void Initialize()
        {
            if (IsInitialized) return;

            QuickLogger.Debug($"Initializing");

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
                _colorManager.ChangeColor(new ColorTemplate());
            }

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }

        #endregion

        #region IProtoEventListener

        public void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || _colorManager == null)
            {
                QuickLogger.Error($"Failed to save driller {GetPrefabID()}");
                return;
            }

            if (_saveData == null)
            {
                _saveData = new AutoCrafterDataEntry();
            }


            QuickLogger.Message($"SaveData = {_saveData}", true);

            _saveData.ID = GetPrefabID();
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
            saveDataList.AutoCrafterDataEntries.Add(_saveData);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.Save(serializer);
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        #endregion

        #region IConstructable
        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (IsInitialized == false)
            {
                return true;
            }
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Info("In Constructed Changed");

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
        #endregion

        #region Internal Methods
        internal void AddItemToStorage(TechType techType)
        {
            _storedItems.Add(techType);
        }

        internal void ShowMessage(string message)
        {
            QuickLogger.Debug(message);
        }

        #endregion

        #region Private Method

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSAutoCrafterSaveData(id);
        }

        private void MoveBelt()
        {
            _moveBelt = true;
        }

        private void StopBelt()
        {
            _moveBelt = false;
        }

        private void MoveBeltMaterial()
        {
            if (_materials != null && _moveBelt)
            {
                float offset = Time.time * _beltSpeed;
                foreach (Material material in _materials)
                {
                    material.SetTextureOffset(ShaderPropertyID._MainTex, new Vector2(-offset, 0));
                    material.SetTextureOffset(ShaderPropertyID._NormalsTex, new Vector2(-offset, 0));
                    material.SetTextureOffset(ShaderPropertyID._Illum, new Vector2(-offset, 0));
                }
            }
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {

            if (obj == PowerSystem.Status.Offline)
            {
                StopBelt();
            }
            else
            {
                MoveBelt();
            }
        }

        #endregion
    }
}
