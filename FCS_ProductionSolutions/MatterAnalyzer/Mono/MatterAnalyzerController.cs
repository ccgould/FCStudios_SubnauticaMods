using System;
using System.Collections.Generic;
using System.Data;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace FCS_ProductionSolutions.MatterAnalyzer.Mono
{
    internal class MatterAnalyzerController : FcsDevice, IFCSSave<SaveData>
    {
        private float _maxScanTime;
        private const float MaxScanTimeL = 300;
        private const float MaxScanTimeS = 180;
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private MatterAnalyzerDataEntry _savedData;
        private bool _isScanning;
        private float _percentage;
        private Text _scanStopBTNLBL;
        private float _scanTime;
        private ScannerController _scanController;
        private Image _percentageBar;
        private Text _percentageTxt;
        private bool _reset;
        private LineRenderer[] _lasers;
        private GameObject[] _laserEnds;
        private GameObject _laserObj;
        private readonly float _resetMultiplier = 30f;
        private GameObject _slot;
        private Vector3 _bounds;
        private GameObject _currentSeed;
        private TechType _currentTechType;
        private bool _isLandPlant;
        private TechType _pickTechType;
        private float PowerUsage = 0.2125f;
        public override bool IsOperational => CheckIfOperational();

        private bool CheckIfOperational()
        {
            return IsConstructed && IsInitialized && Manager != null && Manager.HasEnoughPower(PowerUsage);
        }

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.MatterAnalyzerTabID, Mod.ModName);
        }

        private void Update()
        {
            if (!IsOperational) return;

            ScanItem();

            ResetScanner();

            DrawLasers();
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

                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor(), ColorTargetMode.Both);
                    _scanTime = _savedData.CurrentScanTime;
                    _maxScanTime = _savedData.CurrentMaxScanTime;
                    _pickTechType = _savedData.PickTechType;
                    _isLandPlant = _savedData.IsLandPlant;
                    _currentTechType = _savedData.CurrentTechType;
                    _reset = _savedData.Reset;

                    if (_scanTime > 0)
                    {
                        _isScanning = true;
                        _laserObj.SetActive(true);
                    }


                    if (_savedData.CurrentTechType != TechType.None)
                    {
                        OnStorageOnContainerAddItem(this,_savedData.CurrentTechType);
                    }
                }

                _savedData = null;
                _runStartUpOnEnable = false;
            }
        }
        
        #endregion

        public DumpContainer DumpContainer { get; private set; }
        public Plantable Seed { get; set; }
        public TechType PickTech { get; set; }

        public override void Initialize()
        {
            if (IsInitialized) return;

            _laserObj = GameObjectHelpers.FindGameObject(gameObject, "Lasers");
            _laserObj.SetActive(false);
            _lasers = _laserObj.GetComponentsInChildren<LineRenderer>();
           
            var temp = new List<GameObject>();
            for (int i = 0; i < _lasers.Length; i++)
            {
                temp.Add(GameObjectHelpers.FindGameObject(gameObject, $"Laser{i + 1}_End")); 
            }

            _laserEnds = temp.ToArray();
            
            _scanController = GameObjectHelpers.FindGameObject(gameObject, "Scanner_Anim").AddComponent<ScannerController>();
            _percentageBar = GameObjectHelpers.FindGameObject(gameObject, "Percentage").GetComponent<Image>();
            _percentageTxt = GameObjectHelpers.FindGameObject(gameObject, "PercentageTxt").GetComponent<Text>();
            _slot = GameObjectHelpers.FindGameObject(gameObject, "SpawnPNT");
            _bounds = GameObjectHelpers.FindGameObject(gameObject, "Bounds").GetComponent<Collider>().bounds.size;

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            var storage = new MatterAnalyzerStorage(this);
            storage.OnContainerAddItem += OnStorageOnContainerAddItem;

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform,Mod.MatterAnalyzerFriendlyName, storage, 4,4);
            }

            ScanStopButtonInitialization();

#if DEBUG
            QuickLogger.Debug($"Initialized Matter Analyzer {GetPrefabID()}");
#endif

            IsInitialized = true;
        }

        private void DrawLasers()
        {
            if (IsInitialized)
            {
                for (var i = 0; i < _lasers.Length; i++)
                {
                    var lineRenderer = _lasers[i];
                    lineRenderer.SetPosition(0, lineRenderer.gameObject.transform.position);
                    lineRenderer.SetPosition(1, _laserEnds[i].transform.position);
                }
            }
        }

        private void ResetScanner()
        {
            if (_reset && _scanController != null)
            {
                _scanTime -= DayNightCycle.main.deltaTime * _resetMultiplier;
                var resetPercentage = _scanTime / _maxScanTime;
                _scanController.SetPercentage(resetPercentage);
                _percentageTxt.text = $"{(resetPercentage * 100.0f):f2} %";
                _percentageBar.fillAmount = resetPercentage;
                if (_scanTime <= 0)
                {
                    _reset = false;
                    _scanTime = 0f;
                    StopScanning();
                }
            }
        }

        private void ScanItem()
        {
            if (_isScanning && _scanController != null)
            {
                _scanTime += DayNightCycle.main.deltaTime;
                _percentage = _scanTime / _maxScanTime;
                _scanController.SetPercentage(_percentage);
                _percentageBar.fillAmount = _percentage;
                _percentageTxt.text = $"{_percentage * 100.0f:f2}%";

                if (_scanTime >= _maxScanTime)
                {
                    _isScanning = false;
                    _reset = true;

                    Mod.AddHydroponicKnownTech(new DNASampleData
                    {
                        TechType = _currentTechType, 
                        PickType = _pickTechType, 
                        IsLandPlant = _isLandPlant,
                    });
                }
            }
        }

        public override float GetPowerUsage()
        {
            if (_isScanning)
            {
                return PowerUsage;
            }

            return 0;
        }

        private void OnStorageOnContainerAddItem(FcsDevice device, TechType techType)
        {
            if (device == null || techType == TechType.None) return;

            if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(techType), out string filepath))
            {
                GameObject prefab = Resources.Load<GameObject>(filepath);
                
                if (prefab != null)
                {
                    var go = GameObject.Instantiate(prefab);
                    var collider = go.GetComponent<Collider>();
                    if (collider != null) collider.isTrigger = true;

                    var rg = go.GetComponent<Rigidbody>();
                    if (rg != null) rg.isKinematic = true;

                    var gp = go.GetComponent<GasPod>();
                    if (gp != null)Destroy(gp);

                    go.transform.SetParent(_slot.transform, false);
                    //go.transform.localScale = new Vector3(2.30f, 2.30f, 2.30f);
                    go.transform.localPosition = new Vector3(0f, 0.24f, 0f);
                    //go.transform.Rotate(90f, 0, 0, Space.Self);
                    
                    _currentSeed = go;

                }
      
            }

            if (Mod.IsNonePlantableAllowedList.Contains(techType))
            {
                _pickTechType = techType;
                _currentTechType = techType;
            }
            else
            {
                _pickTechType = PickTech != TechType.None ? PickTech : techType;
                _isLandPlant = Seed.aboveWater;
                _currentTechType = techType;
            }
            
            StartScanning();
        }
        
        private void ScanStopButtonInitialization()
        {
            var scanStopBtnObj = GameObjectHelpers.FindGameObject(gameObject, "Add/ScanBTN");

            _scanStopBTNLBL = scanStopBtnObj.GetComponentInChildren<Text>();

            var scanStopBtn = scanStopBtnObj.GetComponent<Button>();

            scanStopBtn.onClick.AddListener(() =>
            {
                if (!IsOperational) return;

                if (_isScanning)
                {
                    var size = CraftData.GetItemSize(_currentTechType);
                    if (Inventory.main.HasRoomFor(size.x,size.y))
                    {
                        CancelScanning();
                    }
                    else
                    {
                        QuickLogger.ModMessage(AuxPatchers.InventoryFull());
                    }
                }
                else
                {
                    DumpContainer.OpenStorage();
                }
            });
        }

        private void CancelScanning()
        {
            _isScanning = false;
            _reset = true;
        }

        private void StopScanning()
        {
            //Reset Percentage
            _percentage = 0f;
            //Reset scan time
            _scanTime = 0f;
            // Change button to Scan
            _scanStopBTNLBL.text = AuxPatchers.Scan();
            //Set is scanning
            _isScanning = false;
            //Reset Percent text
            _percentageTxt.text = $"0.00%";
            //Reset Percentage
            _percentageBar.fillAmount = 0f;
            //Hide lasers
            _laserObj.SetActive(false);
            //Reset TechType;
            _currentTechType = TechType.None;

            DumpContainer.GetItemsContainer().Clear();

            if (_reset)
            {
                PlayerInteractionHelper.GivePlayerItem(_currentSeed.GetComponent<Pickupable>());
            }
            else
            {
                //Destroy
                DestroyImmediate(_currentSeed);
            }


            _pickTechType = TechType.None;
            
            _maxScanTime = 0;
            
            _isLandPlant = false;
        }

        private void StartScanning()
        {
            //Reset Percentage
            _percentage = 0f;
            // Change button to Stop
            _scanStopBTNLBL.text = AuxPatchers.Stop();
            //Set is scanning
            _isScanning = true;
            _laserObj.SetActive(true);
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
            if (_isScanning || _reset)
            {
                reason = AuxPatchers.MatterAnalyzerHasItems();
                return false;
            }
            reason = String.Empty;
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
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new MatterAnalyzerDataEntry();
            }

            _savedData.ID = GetPrefabID();
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.CurrentTechType = _currentTechType;
            _savedData.PickTechType = _pickTechType;
            _savedData.IsLandPlant = _isLandPlant;
            _savedData.CurrentScanTime = _scanTime;
            _savedData.CurrentMaxScanTime = _maxScanTime;
            newSaveData.MatterAnalyzerEntries.Add(_savedData);
            _savedData.Reset = _reset;
            QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetMatterAnalyzerSaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void SetScanTime(Plantable.PlantSize plantSize)
        {
            _maxScanTime = plantSize == Plantable.PlantSize.Large ? MaxScanTimeL : MaxScanTimeS;
            QuickLogger.Debug($"Max Scan Time was set to: {_maxScanTime}",true);

        }
    }
}
