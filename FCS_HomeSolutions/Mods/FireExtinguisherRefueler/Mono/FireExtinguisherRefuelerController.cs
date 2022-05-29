using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Buildable;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Mono
{
    internal class FireExtinguisherRefuelerController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private FEXRDataEntry _savedData;
        private StorageContainer _storageContainer;
        private FireExtinguisher _fireEx;
        private float _time = 1f;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private readonly Color _colorDefault = Color.cyan;
        private Image _bar;
        private bool _subscribed;

        public override void Awake()
        {
            base.Awake();
            _storageContainer = gameObject.GetComponent<StorageContainer>();
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this,
                FireExtinguisherRefuelerBuildable.FireExtinguisherRefuelerTabID, Mod.ModPackID);
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
            _storageContainer.enabled = true;

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

            if (_storageContainer?.container != null)
            {
                _storageContainer.container.onAddItem -= OnStorageAddItem;
                _storageContainer.container.onRemoveItem -= OnStorageRemoveItem;
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
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 2.5f);
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject,
                    new Color(0, 1, 1, 1));
            }

            InvokeRepeating(nameof(FindExtinguisher), 1, 1);

            IsInitialized = true;
            QuickLogger.Info("Initialized", true);
        }

        private void LateUpdate()
        {
            Subscribe(true);
        }

        private void OnDisable()
        {
            Subscribe(false);
            _storageContainer.enabled = false;
        }

        private void Subscribe(bool state)
        {
            if (_subscribed == state)
            {
                return;
            }

            if (_storageContainer.container == null)
            {
                QuickLogger.Debug("FireExtinguisher.Subscribe(): container null; will retry next frame");
                return;
            }

            if (_subscribed)
            {
                _storageContainer.container.onAddItem -= OnStorageAddItem;
                _storageContainer.container.onRemoveItem -= OnStorageRemoveItem;
                _storageContainer.container.isAllowedToAdd = null;
                _storageContainer.container.isAllowedToRemove = null;
            }
            else
            {
                _storageContainer.container.onAddItem += OnStorageAddItem;
                _storageContainer.container.onRemoveItem += OnStorageRemoveItem;
                _storageContainer.container.isAllowedToAdd = IsAllowedToAdd;
            }

            _subscribed = state;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return pickupable.GetTechType() == TechType.FireExtinguisher;
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
            QuickLogger.Debug("OnStorageAddItem", true);


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
            if (_storageContainer?.container == null) return;

            if (_fireEx != null)
            {
                CancelInvoke(nameof(FindExtinguisher));
                return;
            }

            if (_storageContainer.container.Contains(TechType.FireExtinguisher))
            {
                _fireEx = _storageContainer.container.GetItems(TechType.FireExtinguisher)[0].item
                    .GetComponent<FireExtinguisher>();
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
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
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
            if (_storageContainer != null && _storageContainer.container.GetCount(TechType.FireExtinguisher) > 0)
            {
                reason = AuxPatchers.ModNotEmptyFormat(FireExtinguisherRefuelerBuildable
                    .FireExtinguisherRefuelerFriendly);
                return false;
            }

            reason = String.Empty;
            return true;
        }

        public override void OnHandHover(GUIHand hand)
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
            if (_storageContainer != null && pda != null)
            {
                Inventory.main.SetUsedStorage(_storageContainer.container);
                pda.Open(PDATab.Inventory, null, OnDumpClose
#if SUBNAUTICA_STABLE
                    , 4f
#endif
                );
            }
            else
            {
                QuickLogger.Error(
                    $"Failed to open the pda values: PDA = {pda} || Storage Container: {_storageContainer}");
            }
        }

        internal void UpdateColor()
        {
            if (_fireEx == null)
            {
                _bar.fillAmount = 0f;
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, _colorDefault);
                return;
            }

            var percentage = _fireEx.fuel / _fireEx.maxFuel;

            if (percentage >= 0f)
            {
                Color value = (percentage < 0.5f)
                    ? Color.Lerp(_colorEmpty, _colorHalf, 2f * percentage)
                    : Color.Lerp(_colorHalf, _colorFull, 2f * percentage - 1f);
                _bar.fillAmount = percentage;
                _bar.color = value;
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, value);
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