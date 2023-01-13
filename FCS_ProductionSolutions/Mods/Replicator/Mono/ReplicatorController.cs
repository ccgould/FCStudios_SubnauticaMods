﻿using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.Mods.Replicator.Buildable;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace FCS_ProductionSolutions.Mods.Replicator.Mono
{
    internal class ReplicatorController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private ReplicatorDataEntry _saveData;
        private ReplicatorSlot _replicatorSlot;
        private List<TechType> _loadedDNASamples;
        private GameObject _samplesGrid;
        private uGUI_Icon _techTypeIcon;
        private Text _unitPerSecond;
        private Text _powerUsagePerSecond;
        private Text _containerAmount;
        private GameObject _spawnPoint;
        private StorageContainer _storageContainer;

        private ReplicatorSpeedButton _speedBTN;
        private GameObject _canvas;
        private InterfaceInteraction _interactionHelper;
        private FCSMessageBox _messageBox;
        private const float PowerUsage = 0.85f;
        public override bool IsOperational => IsConstructed && IsInitialized;
        public override bool IsVisible => true;
        public override StorageType StorageType => StorageType.Replicator;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, ReplicatorBuildable.ReplicatorTabID, Mod.ModPackID);
            if (Manager == null)
            {
                _canvas.SetActive(false);
            }
            else
            {
                Manager.AlertNewFcsStoragePlaced(this);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            IPCMessage -= OnIpcMessage;
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

                _speedBTN.SetSpeedMode(_saveData.HarvesterSpeed);

                if (_saveData.TargetItem != TechType.None)
                {
                    QuickLogger.Debug("Loading Replicator save");
                    _replicatorSlot.ChangeTargetItem(_saveData.TargetItem, true);
                    //_replicatorSlot.SetItemCount(_saveData.ItemCount);
                    _replicatorSlot.CurrentHarvesterSpeedMode = _saveData.HarvesterSpeed;
                    _replicatorSlot.GenerationProgress = _saveData.Progress;
                    _techTypeIcon.sprite = SpriteManager.Get(_saveData.TargetItem);
                    StartCoroutine(SpawnModel(_saveData.TargetItem));
                }

                UpdateUI();

                _fromSave = false;
            }
        }

        public override ItemsContainer GetItemsContainer()
        {
            if (_storageContainer == null)
            {
                _storageContainer = GetComponent<StorageContainer>();
            }

            if (_storageContainer)
            {
                return _storageContainer.container;
            }

            return null;
        }

        internal void LoadKnownSamples()
        {
            if (_loadedDNASamples == null)
            {
                _loadedDNASamples = new List<TechType>();
            }

            var knownSamples = Mod.GetHydroponicKnownTech();

            if (knownSamples == null) return;

            foreach (var sample in knownSamples)
            {
                if (sample.TechType == TechType.None || _loadedDNASamples.Contains(sample.TechType) ||
                    !WorldHelpers.IsNonePlantableAllowedList.Contains(sample.TechType)) continue;
                var button = GameObject.Instantiate(ModelPrefab.HydroponicDNASamplePrefab)
                    .AddComponent<InterfaceButton>();
                var icon = GameObjectHelpers.FindGameObject(button.gameObject, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(sample.TechType);
                button.TextLineOne = Language.main.Get((sample.TechType));
                button.Tag = sample.TechType;
                button.HOVER_COLOR = Color.white;
                button.STARTING_COLOR = Color.gray;

                button.OnButtonClick += (s, o) =>
                {
                    if (!_replicatorSlot.IsOccupied)
                    {
                        Clear();
                        var techType = (TechType) o;
                        StartCoroutine(SpawnModel(techType));
                        _techTypeIcon.sprite = SpriteManager.Get(techType);
                        _replicatorSlot.ChangeTargetItem(techType);
                        UpdateUI();
                    }
                    else
                    {
                        QuickLogger.ModMessage(AuxPatchers.PleaseClearReplicatorSlot());
                    }
                };
                _loadedDNASamples.Add(sample.TechType);
                button.gameObject.transform.SetParent(_samplesGrid.transform, false);
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetReplicatorSaveData(id);
        }

        public override float GetPowerUsage()
        {
            if (!IsOperational || _replicatorSlot == null || !_replicatorSlot.IsOccupied ||
                _replicatorSlot.IsFull) return 0f;

            switch (_replicatorSlot.CurrentHarvesterSpeedMode)
            {
                case HarvesterSpeedModes.Off:
                    return 0;
                case HarvesterSpeedModes.Max:
                    return PowerUsage * 4;
                case HarvesterSpeedModes.High:
                    return PowerUsage * 3;
                case HarvesterSpeedModes.Low:
                    return PowerUsage * 2;
                case HarvesterSpeedModes.Min:
                    return PowerUsage;
                default:
                    return 0f;
            }
        }

        public override void Initialize()
        {
            _canvas = gameObject.GetComponentInChildren<Canvas>().gameObject;

            var prxy = _canvas.EnsureComponent<ProximityActivate>();
            prxy.Initialize(_canvas, gameObject, 2);
            prxy.OnVisible += () =>
            {
                LoadKnownSamples();
                UpdateUI();
            };



            _samplesGrid = GameObjectHelpers.FindGameObject(gameObject, "Grid");
            _techTypeIcon = GameObjectHelpers.FindGameObject(gameObject, "CurrentTechTypeIcon")
                .EnsureComponent<uGUI_Icon>();
            _techTypeIcon.sprite = SpriteManager.Get(TechType.None);
            _unitPerSecond = GameObjectHelpers.FindGameObject(gameObject, "UnitPerSecond").GetComponent<Text>();
            _powerUsagePerSecond =
                GameObjectHelpers.FindGameObject(gameObject, "PowerUsagePerSecond").GetComponent<Text>();
            _containerAmount = GameObjectHelpers.FindGameObject(gameObject, "Amount").GetComponent<Text>();
            _spawnPoint = GameObjectHelpers.FindGameObject(gameObject, "SpawnPnt");

            var bobbing = _spawnPoint.AddComponent<Bobbing>();
            bobbing.SetState(true);

            var itemBTNObj = InterfaceHelpers.FindGameObject(gameObject, "ActiveSampleSlot");
            InterfaceHelpers.CreateButton(itemBTNObj, "ItemBtn", InterfaceButtonMode.Background,
                OnButtonClick, Color.gray, Color.white, 5);

            var clearBTNObj = InterfaceHelpers.FindGameObject(gameObject, "RemoveDNABTN");
            
            var removeDnaBtnLongPressListener = clearBTNObj.AddComponent<FCSMultiClickButton>();
            removeDnaBtnLongPressListener.TextLineOne = AuxPatchers.HarvesterDeleteSample();
            removeDnaBtnLongPressListener.TextLineTwo = AuxPatchers.HarvesterDeleteSampleDesc();
            removeDnaBtnLongPressListener.onLongPress += OnLongPress;
            removeDnaBtnLongPressListener.onSingleClick += Clear;


            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            if (_replicatorSlot == null)
            {
                _replicatorSlot = gameObject.GetComponent<ReplicatorSlot>();
                _replicatorSlot.Initialize(this);
            }

            IPCMessage += OnIpcMessage;

            var speedBTNObj = GameObjectHelpers.FindGameObject(gameObject, "SpeedBTN");
            _speedBTN = speedBTNObj.AddComponent<ReplicatorSpeedButton>();
            _speedBTN.ReplicatorController = this;

            LoadKnownSamples();

            UpdateUI();

            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionStrength(string.Empty, gameObject, 5f);

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();
            _messageBox = GameObjectHelpers.FindGameObject(_canvas, "MessageBox").AddComponent<FCSMessageBox>();

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private void OnLongPress()
        {
            ShowMessage(AuxPatchers.ClearSlotLongPress(), FCSMessageButton.YESNO, (result =>
            {
                if (result != FCSMessageResult.OKYES) return;
                _replicatorSlot.TryClear(true);
                Clear();
            }));
        }

        internal void ShowMessage(string message, FCSMessageButton button = FCSMessageButton.OK, Action<FCSMessageResult> result = null)
        {
            _messageBox.Show(message, button, result);
        }

        private void OnButtonClick(string btnName, object additionalData)
        {
            switch (btnName)
            {
                case "ItemBtn":

                    var pickUp = _replicatorSlot.RemoveItem();
                    if (pickUp != null)
                    {
                        PlayerInteractionHelper.GivePlayerItem(pickUp);
                    }
                    break;
            }
        }

        public override Pickupable RemoveItemFromContainer(TechType techType)
        {
            return _replicatorSlot.RemoveItemFromContainer(techType);
        }

        public override bool RemoveItemFromContainer(InventoryItem item)
        {
            return RemoveItemFromContainer(item.item.GetTechType());
        }

        private void Clear()
        {
            QuickLogger.Debug("Trying to clear.", true);
            if (_replicatorSlot.TryClear())
            {
                if (_spawnPoint.transform.childCount > 0)
                {
                    foreach (Transform child in _spawnPoint.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }

                _techTypeIcon.sprite = SpriteManager.Get(TechType.None);
            }
        }

        private void OnIpcMessage(string message)
        {
            QuickLogger.Debug($"Recieving Message: {message}", true);

            if (message.Equals("UpdateDNA"))
            {
                LoadKnownSamples();
                QuickLogger.Debug("Loading DNA Samples", true);
            }
        }

        internal void UpdateUI()
        {
            _unitPerSecond.text =
                AuxPatchers.GenerationTimeMinutesOnlyFormat(
                    Convert.ToSingle(_replicatorSlot.CurrentHarvesterSpeedMode));
            _powerUsagePerSecond.text = AuxPatchers.PowerUsagePerSecondFormat(GetPowerUsage());
            _containerAmount.text = $"{_replicatorSlot.GetCount()}/{_replicatorSlot.GetMaxCount()}";
            UpdateTerminals();
        }

        private void UpdateTerminals()
        {
            BaseManager.GlobalNotifyByID("DTC", "ItemUpdateDisplay");
        }

        private IEnumerator SpawnModel(TechType techType)
        {
            if (_spawnPoint.transform.childCount > 0)
            {
                foreach (Transform child in _spawnPoint.transform)
                {
                    Destroy(child);
                }
            }

            var task = new TaskResult<GameObject>();
            yield return CraftData.GetPrefabForTechTypeAsync(techType, false, task);
            GameObject prefab = task.Get();

            if (prefab == null) yield break;
            
            var go = Instantiate(prefab);
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            ;

            var rg = go.GetComponent<Rigidbody>();
            if (rg != null)
            {
                Destroy(rg);
            }

            var wf = go.GetComponent<WorldForces>();
            if (wf != null)
            {
                Destroy(wf);
            }

            var pickup = go.GetComponent<Pickupable>();
            if (pickup != null)
            {
                Destroy(pickup);
            }

            var pf = go.GetComponent<PrefabIdentifier>();
            if (pf != null)
            {
                Destroy(pf);
            }

            var eniTag = go.GetComponent<EntityTag>();
            if (eniTag != null)
            {
                Destroy(eniTag);
            }

            var gaspod = go.GetComponent<GasPod>();
            if (gaspod != null)
            {
                Destroy(gaspod);
            }

            var resourceTracker = go.GetComponent<ResourceTracker>();
            if (resourceTracker != null)
            {
                Destroy(resourceTracker);
            }

            go.transform.parent = _spawnPoint.transform;
            UWE.Utils.ZeroTransform(go);
            go.transform.Rotate(0f, 90, 90, Space.Self);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {ReplicatorBuildable.ReplicatorFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {ReplicatorBuildable.ReplicatorFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
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
                _saveData = new ReplicatorDataEntry();
            }

            _saveData.ID = id;
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
            _saveData.TargetItem = _replicatorSlot.GetTargetItem();
            _saveData.Progress = _replicatorSlot.GenerationProgress;
            _saveData.HarvesterSpeed = _replicatorSlot.CurrentHarvesterSpeedMode;
            _saveData.ItemCount = _replicatorSlot.GetCount();
            newSaveData.ReplicatorEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public void SetSpeedMode(HarvesterSpeedModes harvesterSpeed)
        {
            if (_replicatorSlot != null)
            {
                _replicatorSlot.CurrentHarvesterSpeedMode = harvesterSpeed;
                UpdateUI();
            }
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _replicatorSlot == null || _interactionHelper.IsInRange) return;

            base.OnHandHover(hand);

            if (Manager == null)
            {
                var data = new[]
                {
                    AuxPatchers.NotBuildOnBase(),
                    AlterraHub.PowerPerMinute(GetPowerUsage() * 60)
                };
                data.HandHoverPDAHelperEx(GetTechType(), HandReticle.IconType.HandDeny);
                return;
            }


            var data1 = new[]
            {
                AlterraHub.PowerPerMinute(GetPowerUsage() * 60)
            };
            data1.HandHoverPDAHelperEx(GetTechType(), HandReticle.IconType.Progress,
                _replicatorSlot.GetPercentageDone());
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }

    internal class Bobbing : MonoBehaviour
    {
        // User Inputs
        public float amplitude = -0.1f; //0.5f;
        public float frequency = 0.6f; //1f;
        public float rotationsPerMinute = 10.0f;

        // Position Storage Variable
        private Vector3 originalLocalPosition;
        private bool _isRunning;
        public bool Invert { get; set; }

        // Use this for initialization
        private void Start()
        {
            // Store the starting position of the object relative to its parent.
            originalLocalPosition = transform.localPosition;
        }

        public void SetState(bool isRunning)
        {
            _isRunning = isRunning;
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_isRunning) return;
            // Calculate offset
            // Even though Transform.up is relative, it is in world space
            // That means that you can't apply it to localPosition
            //
            // Vector3.up could be used along with localPosition, but that would ignore the object's rotation

            Vector3 globalOffset = transform.up * Mathf.Sin(Time.time * Mathf.PI * frequency) * amplitude;

            // Reset the position to the original
            transform.localPosition = originalLocalPosition;

            // Apply offset
            if (!Invert)
            {
                transform.position += globalOffset;
            }
            else
            {
                transform.position -= globalOffset;
            }

            //transform.Rotate(0,amplitude * rotationsPerMinute * DayNightCycle.main.deltaTime,0);
        }
    }
}