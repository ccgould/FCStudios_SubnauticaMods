using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Mono
{
    internal class FireExtinguisherRefuelerController : FcsDevice,IFCSSave<SaveData> ,IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private FEXRDataEntry _savedData;
        private StorageContainer _storage;
        private FireExtinguisher _fireEx;
        private float _time = 1f;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private readonly Color _colorDefault = Color.cyan;
        private Image _bar;


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.FireExtinguisherRefuelerTabID, Mod.ModPackID);
        }

        private void Update()
        {
            if (_fireEx != null)
            {
                _time -= DayNightCycle.main.deltaTime;

                if (_time <= 0f)
                {
                    _fireEx.fuel = Mathf.Clamp(_fireEx.fuel + 1, 0, _fireEx.maxFuel);
                    _time = 1f;
                    UpdateColor();
                }
            }
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

                    if (_savedData != null)
                    { 
                        _colorManager.LoadTemplate(_savedData.ColorTemplate);
                        _isFromSave = false;
                    }
                }
                _runStartUpOnEnable = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_storage?.container != null)
            {
                _storage.container.onAddItem -= OnStorageAddItem;
                _storage.container.onRemoveItem -= OnStorageRemoveItem;
            }
        }

        public override void Initialize()
        {
            QuickLogger.Info("Initializing", true);

            _bar = GameObjectHelpers.FindGameObject(gameObject, "Bar")?.GetComponent<Image>();

            if (_colorManager == null)
            {
                QuickLogger.Info($"Creating Color Component", true);
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
                MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial, gameObject,2.5f);
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissionControllerMaterial,gameObject, new Color(0, 1, 1, 1));
            }

            _storage = gameObject.GetComponent<StorageContainer>();
            _storage.container.onAddItem += OnStorageAddItem;
            _storage.container.onRemoveItem += OnStorageRemoveItem;
            _storage.container.SetAllowedTechTypes(new[] { TechType.FireExtinguisher });
            
            InvokeRepeating(nameof(FindExtinguisher),1,1);

            IsInitialized = true;
            QuickLogger.Info("Initialized", true);
        }

        private void OnStorageRemoveItem(InventoryItem item)
        {
            item.item.gameObject.SetActive(false);
            _fireEx = null;
            var collisions = item.item.GetComponentsInChildren<Collider>();

            foreach (Collider collider in collisions)
            {
                collider.isTrigger = true;
            }
            UpdateColor();
        }

        private void OnStorageAddItem(InventoryItem item)
        {
            QuickLogger.Debug("OnStorageAddItem",true);



            _fireEx = item.item.GetComponentInChildren<FireExtinguisher>();

            DisableComponents(item.item.gameObject);

            item.item.gameObject.SetActive(true);
        }

        private static void DisableComponents(GameObject item)
        {
            var rigidBodies = item.GetComponentsInChildren<Rigidbody>();
            var collisions = item.GetComponentsInChildren<Collider>();

            foreach (Rigidbody rg in rigidBodies)
            {
                rg.isKinematic = true;
            }

            foreach (Collider collider in collisions)
            {
                collider.isTrigger = true;
            }
        }

        private void FindExtinguisher()
        {
            if (_storage?.container == null) return;
            
            if (_fireEx != null)
            {
                CancelInvoke(nameof(FindExtinguisher));
                return;
            }
            
            if (_storage.container.Contains(TechType.FireExtinguisher))
            {
                _fireEx = _storage.container.GetItems(TechType.FireExtinguisher)[0].item.GetComponent<FireExtinguisher>();
                DisableComponents(_fireEx.gameObject);
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new FEXRDataEntry();
            }

            _savedData.Id = GetPrefabID();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.FEXRDataEntries.Add(_savedData);
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

            if (!IsInitialized)
            {
                Initialize();
            }

            //_storage.RestoreItems(serializer, _savedData.Data);
            _isFromSave = true;
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

        public override bool CanDeconstruct(out string reason)
        {
            if (_storage != null && _storage.container.GetCount(TechType.FireExtinguisher) > 0)
            {
                reason = AuxPatchers.ModNotEmptyFormat(Mod.FireExtinguisherRefuelerFriendly);
                return false;
            }
            reason = String.Empty;
            return true;
        }

        public override void  OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;

            
            //HandReticle main = HandReticle.main;
            //main.SetInteractText(AuxPatchers.ClickToOpenFormatted(Mod.FireExtinguisherRefuelerFriendly));
            //main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {

            Player main = Player.main;
            PDA pda = main.GetPDA();
            if (_storage != null && pda != null)
            {
                Inventory.main.SetUsedStorage(_storage.container);
                pda.Open(PDATab.Inventory, null, OnDumpClose, 4f);
            }
            else
            {
                QuickLogger.Error($"Failed to open the pda values: PDA = {pda} || Storage Container: {_storage}");
            }
        }

        internal void UpdateColor()
        {
            if (_fireEx == null)
            {
                _bar.fillAmount = 0f;
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissionControllerMaterial, gameObject, _colorDefault);
                return;
            }

            var percentage = _fireEx.fuel / _fireEx.maxFuel;

            if (percentage >= 0f)
            {
                Color value = (percentage < 0.5f) ? Color.Lerp(_colorEmpty, _colorHalf, 2f * percentage) : Color.Lerp(_colorHalf, _colorFull, 2f * percentage - 1f);
                _bar.fillAmount = percentage;
                _bar.color = value;
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissionControllerMaterial, gameObject, value);
            }
        }

        private void OnDumpClose(PDA pda)
        {
            
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.FEXREntrySaveData(GetPrefabID());
        }
    }
}
