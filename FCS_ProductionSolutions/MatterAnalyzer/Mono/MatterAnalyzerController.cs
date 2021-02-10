using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Converters;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

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
        private float _scanTime;
        private TechType _currentTechType;
        private bool _isLandPlant;
        private TechType _pickTechType;
        private float PowerUsage = 0.2125f;
        private Button _insertButton;
        private Button _cancelButton;
        private Button _removeButton;
        private Button _scanButton;
        private Image _percentageBar;
        private Text _percentageTxt;
        private GameObject _timegroup;
        private Text _timeTxt;
        private uGUI_Icon _icon;
        private MessagePop _messagePop;
        private Material _material;
        private List<DNASampleItem> _sampleItems = new List<DNASampleItem>();
        private GridHelperV2 _grid;
        private const float Speed = 0.1f;
        public override bool IsOperational => CheckIfOperational();
        public DumpContainer DumpContainer { get; private set; }
        public Plantable Seed { get; set; }
        public TechType PickTech { get; set; }
        public MotorHandler MotorHandler { get; private set; }
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

            if (_material != null)
            {
                float offset = Time.time * Speed;
                _material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
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

                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor(), ColorTargetMode.Both);
                    _scanTime = _savedData.CurrentScanTime;
                    _maxScanTime = _savedData.CurrentMaxScanTime;
                    _pickTechType = _savedData.PickTechType;
                    _isLandPlant = _savedData.IsLandPlant;
                    _currentTechType = _savedData.CurrentTechType;
                    MotorHandler.SpeedByPass(_savedData.RPM);

                    if (_scanTime > 0)
                    {
                        _isScanning = true;
                    }

                    if (_savedData.CurrentTechType != TechType.None)
                    {
                        OnStorageOnContainerAddItem(this,_savedData.CurrentTechType);
                        ChangeScanButtonState(false);
                        _timegroup.SetActive(true);
                        MotorHandler.StartMotor();
                    }
                }

                _savedData = null;
                _runStartUpOnEnable = false;
            }
        }

        public override void OnDestroy()
        {
            IPCMessage -= OnIpcMessage;
            base.OnDestroy();
        }
        #endregion



        public override void Initialize()
        {
            if (IsInitialized) return;

            IPCMessage += OnIpcMessage;

            foreach (Transform invItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
            {
                var item = invItem.gameObject.EnsureComponent<DNASampleItem>();
                _sampleItems.Add(item);
                item.Initialize();
            }

            _material = MaterialHelpers.GetMaterial(GameObjectHelpers.FindGameObject(gameObject, "CautionTrimRotor"), "fcs01_BD");

            _percentageBar = GameObjectHelpers.FindGameObject(gameObject, "RingFR").GetComponent<Image>();
            
            _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            _icon.gameObject.SetActive(false);

            _percentageTxt = GameObjectHelpers.FindGameObject(gameObject, "percentage").GetComponent<Text>();
            
            _timeTxt = GameObjectHelpers.FindGameObject(gameObject, "time").GetComponent<Text>();

            var homePage = GameObjectHelpers.FindGameObject(gameObject, "Home");

            _messagePop = GameObjectHelpers.FindGameObject(gameObject, "UpdatedDataBase").AddComponent<MessagePop>();
            _messagePop.Initialize();
            _timegroup = GameObjectHelpers.FindGameObject(gameObject, "TimeGroup");
            var databasePage = GameObjectHelpers.FindGameObject(gameObject, "SamplesDatabase");

            var databaseButton = GameObjectHelpers.FindGameObject(gameObject, "DatabaseButton").GetComponent<Button>();
            databaseButton.onClick.AddListener(() =>
            {
                _messagePop.transform.localScale = Vector3.zero;
                homePage.SetActive(false);
                databasePage.SetActive(true);
            });

            _removeButton = GameObjectHelpers.FindGameObject(gameObject, "RemoveButton").GetComponent<Button>();
            _removeButton.onClick.AddListener(() =>
            {
                if (!IsOperational) return;

                var size = CraftData.GetItemSize(_currentTechType);
                if (Inventory.main.HasRoomFor(size.x, size.y))
                {
                    CancelScanning();
                }
                else
                {
                    QuickLogger.ModMessage(AuxPatchers.InventoryFull());
                }
            });

            var returnButton = GameObjectHelpers.FindGameObject(gameObject, "returnButton").GetComponent<Button>();
            returnButton.onClick.AddListener(() =>
            {
                homePage.SetActive(true);
                databasePage.SetActive(false);
            });

            if (MotorHandler == null)
            {
                MotorHandler = GameObjectHelpers.FindGameObject(gameObject, "ChemicalRotor").AddComponent<MotorHandler>();
                MotorHandler.Initialize(200);
                MotorHandler.StopMotor();
            }

            _scanButton = GameObjectHelpers.FindGameObject(gameObject, "ScanButton").GetComponent<Button>();
            _scanButton.onClick.AddListener(StartScanning);

            _cancelButton = GameObjectHelpers.FindGameObject(gameObject, "CancelButton").GetComponent<Button>();
            _cancelButton.onClick.AddListener(() =>
            {
                if (!IsOperational) return;

                var size = CraftData.GetItemSize(_currentTechType);
                if (Inventory.main.HasRoomFor(size.x, size.y))
                {
                    CancelScanning();
                }
                else
                {
                    QuickLogger.ModMessage(AuxPatchers.InventoryFull());
                }
            });

            _insertButton = GameObjectHelpers.FindGameObject(gameObject, "InsertButton").GetComponent<Button>();
            _insertButton.onClick.AddListener(() =>
            {
                if (!IsOperational) return;
                DumpContainer.OpenStorage();
            });

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            var storage = new MatterAnalyzerStorage(this);
            storage.OnContainerAddItem += OnStorageOnContainerAddItem;

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform,Mod.MatterAnalyzerFriendlyName, storage, 4,4);
            }

#if DEBUG
            QuickLogger.Debug($"Initialized Matter Analyzer {GetPrefabID()}");
#endif


            _grid = gameObject.AddComponent<GridHelperV2>();
            _grid.OnLoadDisplay += OnLoadSamplesGrid;
            _grid.Setup(50, gameObject, Color.gray, Color.white, null);
            _grid.DrawPage();
            IsInitialized = true;
        }

        private void OnIpcMessage(string message)
        {
            QuickLogger.Debug($"Recieving Message: {message}", true);

            if (message.Equals("UpdateDNA"))
            {
                RefreshUI();
                QuickLogger.Debug("Loading DNA Samples", true);
            }
        }


        public override void RefreshUI()
        {
            base.RefreshUI();
            _grid.DrawPage();
        }

        private void OnLoadSamplesGrid(DisplayData data)
        {
            try
            {
                var grouped = Mod.GetHydroponicKnownTech();

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _sampleItems[i].Reset();
                }

                var g = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _sampleItems[g++].Set(grouped[i].TechType);
                }

                //_basesGrid.UpdaterPaginator(grouped.Count);
                //_basePaginatorController.ResetCount(_basesGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }
        
        private void ScanItem()
        {
            if (_isScanning)
            {
                _scanTime += DayNightCycle.main.deltaTime;
                _percentage = _scanTime / _maxScanTime;
                _percentageBar.fillAmount = _percentage;
                _percentageTxt.text = $"{_percentage * 100.0f:f0}%";
                _timeTxt.text = TimeConverters.SecondsToMS(_maxScanTime - _scanTime);

                if (_scanTime >= _maxScanTime)
                {
                    _isScanning = false;
                    _timegroup.SetActive(false);
                    Mod.AddHydroponicKnownTech(new DNASampleData
                    {
                        TechType = _currentTechType, 
                        PickType = _pickTechType, 
                        IsLandPlant = _isLandPlant,
                    });
                    CompleteScanning();
                    ChangeInsertState();
                    Reset();
                    _messagePop.Show();
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
            
            if (Mod.IsNonePlantableAllowedList.Contains(techType))
            {
                _pickTechType = techType;
                _currentTechType = techType;
            }
            else
            {
                _pickTechType = PickTech != TechType.None ? PickTech : techType;
                QuickLogger.Debug($"Setting PickType to {_pickTechType}",true);
                _isLandPlant = Seed.aboveWater;
                _currentTechType = techType;
            }

            _icon.sprite = SpriteManager.Get(techType);
            _icon.gameObject.SetActive(true);
            ChangeInsertState(false);
            ChangeScanButtonState();
        }

        private void ChangeScanButtonState(bool isActive = true)
        {
            _scanButton.gameObject.SetActive(isActive);
            _cancelButton.gameObject.SetActive(!isActive);
        }

        private void ChangeInsertState(bool isActive = true)
        {
            _insertButton.gameObject.SetActive(isActive);
            _removeButton.gameObject.SetActive(!isActive);
        }

        private void CancelScanning()
        {
            ChangeInsertState();
            CompleteScanning(true);
            Reset();
            MotorHandler.StopMotor();
        }

        private void CompleteScanning(bool cancel = false)
        {
            DumpContainer.GetItemsContainer().Clear();

            if (cancel)
            {
                PlayerInteractionHelper.GivePlayerItem(_currentTechType);
            }
            MotorHandler.StopMotor();
            _grid.DrawPage();
        }

        private void StartScanning()
        {
            //Reset Percentage
            _percentage = 0f;
            //Set is scanning
            _isScanning = true;
            ChangeScanButtonState(false);
            _timegroup.SetActive(true);
            MotorHandler.StartMotor();
        }

        private void Reset()
        {
            //Reset Percentage
            _percentage = 0f;
            //Reset scan time
            _scanTime = 0f;
            //Set is scanning
            _isScanning = false;
            //Reset Percent text
            _percentageTxt.text = $"0%";
            //Reset Percentage
            _percentageBar.fillAmount = 0f;
            //Reset TechType;
            _currentTechType = TechType.None;
            _pickTechType = TechType.None;
            _maxScanTime = 0;
            _isLandPlant = false;
            _icon.gameObject.SetActive(false);
            _timegroup.SetActive(false);
            _cancelButton.gameObject.SetActive(false);
            _scanButton.gameObject.SetActive(false);
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
            if (_isScanning)
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
            _savedData.RPM = MotorHandler.GetRPM();
            _savedData.CurrentMaxScanTime = _maxScanTime;
            newSaveData.MatterAnalyzerEntries.Add(_savedData);
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

    internal class DNASampleItem : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private bool _initialized;
        private FCSToolTip _toolTip;

        internal void Initialize()
        {
            if(_initialized) return;
            _icon = gameObject.FindChild("Icon").AddComponent<uGUI_Icon>();
            _toolTip = gameObject.AddComponent<FCSToolTip>();
            _toolTip.RequestPermission += () => true;
            _initialized = true;
        }

        internal void Reset()
        {
            gameObject.SetActive(false);
        }

        internal void Set(TechType techType)
        {
            if (!_initialized) return;
            _icon.sprite = SpriteManager.Get(techType);
            _toolTip.TechType = techType;
            gameObject.SetActive(true);
        }
    }
}
