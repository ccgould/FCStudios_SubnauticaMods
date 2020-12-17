using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
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
        private ColorManager _colorManager;
        private ReplicatorSlot _replicatorSlot;
        private List<TechType> _loadedDNASamples;
        private GameObject _samplesGrid;
        private uGUI_Icon _techTypeIcon;
        private Text _unitPerSecond;
        private Text _powerUsagePerSecond;
        private Text _containerAmount;
        private GameObject _spawnPoint;
        private ReplicatorSpeedButton _speedBTN;
        private const float PowerUsage = 0.85f;
        public override bool IsOperational => IsConstructed && IsInitialized;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.ReplicatorTabID, Mod.ModName);
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
                _colorManager.ChangeColor(_saveData.BodyColor.Vector4ToColor());
                
                _speedBTN.SetSpeedMode(_saveData.Speed);
                _replicatorSlot.CurrentSpeedMode = _saveData.Speed;
                _replicatorSlot.SetItemCount(_saveData.ItemCount);
                
                _replicatorSlot.GenerationProgress = _saveData.Progress;

                if (_saveData.TargetItem != TechType.None)
                {
                    _replicatorSlot.ChangeTargetItem(_saveData.TargetItem);
                    _techTypeIcon.sprite = SpriteManager.Get(_saveData.TargetItem);
                    SpawnModel(_saveData.TargetItem);
                }

                UpdateUI();

                _fromSave = false;
            }
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
                if (sample.TechType == TechType.None || _loadedDNASamples.Contains(sample.TechType) || !sample.IsNonePlantable) continue;
                var button = GameObject.Instantiate(ModelPrefab.HydroponicDNASamplePrefab).AddComponent<InterfaceButton>();
                var icon = GameObjectHelpers.FindGameObject(button.gameObject, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(sample.TechType);
                button.TextLineOne = Language.main.Get((sample.TechType));
                button.Tag = sample.TechType;

                button.OnButtonClick += (s, o) =>
                {
                    var techType = (TechType) o;
                    SpawnModel(techType);
                    _techTypeIcon.sprite = SpriteManager.Get(techType);
                    _replicatorSlot.ChangeTargetItem(techType);
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
            if (!IsOperational || _replicatorSlot == null || !_replicatorSlot.IsOccupied) return 0f;
            switch (_replicatorSlot.CurrentSpeedMode)
            {
                case SpeedModes.Off:
                    return 0;
                case SpeedModes.Max:
                    return PowerUsage * 4;
                case SpeedModes.High:
                    return PowerUsage * 3;
                case SpeedModes.Low:
                    return PowerUsage * 2;
                case SpeedModes.Min:
                    return PowerUsage;
                default:
                    return 0f;
            }
        }

        public override void Initialize()
        {
            var canvas = gameObject.GetComponentInChildren<Canvas>().gameObject;
            var prxy = canvas.EnsureComponent<ProximityActivate>();
            prxy.Initialize(canvas, gameObject, 2);
            _samplesGrid = GameObjectHelpers.FindGameObject(gameObject, "Grid");
            _techTypeIcon = GameObjectHelpers.FindGameObject(gameObject, "CurrentTechTypeIcon").EnsureComponent<uGUI_Icon>();
            _techTypeIcon.sprite = SpriteManager.Get(TechType.None);
            _unitPerSecond = GameObjectHelpers.FindGameObject(gameObject, "UnitPerSecond").GetComponent<Text>();
            _powerUsagePerSecond = GameObjectHelpers.FindGameObject(gameObject, "PowerUsagePerSecond").GetComponent<Text>();
            _containerAmount = GameObjectHelpers.FindGameObject(gameObject, "Amount").GetComponent<Text>();
            _spawnPoint = GameObjectHelpers.FindGameObject(gameObject, "SpawnPnt");
            var bobbing = _spawnPoint.AddComponent<Bobbing>();
            bobbing.SetState(true);

            var itemBTNObj = InterfaceHelpers.FindGameObject(gameObject, "ActiveSampleSlot");
            InterfaceHelpers.CreateButton(itemBTNObj, "ItemBtn", InterfaceButtonMode.Background,
                OnButtonClick, Color.gray, Color.white, 5);

            var clearBTNObj = InterfaceHelpers.FindGameObject(gameObject, "RemoveDNABTN");
            InterfaceHelpers.CreateButton(clearBTNObj, "ClearBtn", InterfaceButtonMode.Background,
                OnButtonClick, Color.gray, Color.white, 5);


            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (_replicatorSlot == null)
            {
                _replicatorSlot = gameObject.AddComponent<ReplicatorSlot>();
                _replicatorSlot.Initialize(this);
            }

            IPCMessage += OnIpcMessage;

            var speedBTNObj = GameObjectHelpers.FindGameObject(gameObject, "SpeedBTN");
            _speedBTN = speedBTNObj.AddComponent<ReplicatorSpeedButton>();
            _speedBTN.ReplicatorController = this;

            LoadKnownSamples();

            UpdateUI();

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial,gameObject,5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private void OnButtonClick(string btnName, object additionalData)
        {
            switch (btnName)
            {
                case "ItemBtn":
                    if (_replicatorSlot.RemoveItem(out TechType techType))
                    {
                        PlayerInteractionHelper.GivePlayerItem(techType);
                    }
                    break;
                case "ClearBtn":
                    Clear();
                    break;
            }
        }

        private void Clear()
        {
            if (_replicatorSlot.TryClear())
            {
                if (_spawnPoint.transform.childCount > 0)
                {
                    foreach (Transform child in _spawnPoint.transform)
                    {
                        Destroy(child);
                    }
                }
            }
        }

        private void OnIpcMessage(string message)
        {
            QuickLogger.Debug($"Recieving Message: {message}",true);
            if (message.Equals("UpdateDNA"))
            {
                LoadKnownSamples();
                QuickLogger.Debug("Loading DNA Samples",true);
            }
        }

        internal void UpdateUI()
        {
            _unitPerSecond.text = AuxPatchers.GenerationTimeMinutesOnlyFormat(Convert.ToSingle(_replicatorSlot.CurrentSpeedMode));
            _powerUsagePerSecond.text = AuxPatchers.PowerUsagePerSecondFormat(GetPowerUsage());
            _containerAmount.text = $"{_replicatorSlot.GetCount()}/{_replicatorSlot.GetMaxCount()}";

        }

        private void SpawnModel(TechType techType)
        {
            if (_spawnPoint.transform.childCount > 0)
            {
                foreach (Transform child in _spawnPoint.transform)
                {
                    Destroy(child);
                }
            }

            if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(techType), out string filepath))
            {
                GameObject prefab = Resources.Load<GameObject>(filepath);

                if (prefab != null)
                {
                    var go = GameObject.Instantiate(prefab);
                    var collider = go.GetComponent<Collider>();
                    if (collider != null)
                    {
                        Destroy(collider);
                    };

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

            }
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.ReplicatorFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.ReplicatorFriendlyName}");
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
            _saveData.BodyColor = _colorManager.GetColor().ColorToVector4();
            _saveData.TargetItem = _replicatorSlot.GetTargetItem();
            _saveData.Progress = _replicatorSlot.GenerationProgress;
            _saveData.Speed = _replicatorSlot.CurrentSpeedMode;
            _saveData.ItemCount = _replicatorSlot.GetCount();
            newSaveData.ReplicatorEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void SetSpeedMode(SpeedModes speed)
        {
            if (_replicatorSlot != null)
            {
                _replicatorSlot.CurrentSpeedMode = speed;
                UpdateUI();
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _replicatorSlot == null) return;
            HandReticle main = HandReticle.main;
            main.SetProgress(_replicatorSlot.GetPercentageDone());
            main.SetIcon(HandReticle.IconType.Progress, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }

    internal class Bobbing : MonoBehaviour
    {
        // User Inputs
        public float amplitude = -0.1f;//0.5f;
        public float frequency = 0.6f;//1f;
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
