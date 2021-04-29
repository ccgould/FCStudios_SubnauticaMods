using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.ObjectPooler;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.TrashRecycler.Mono
{
    internal class TrashRecyclerController : FcsDevice, IFCSSave<SaveData>, IFCSDumpContainer
    {
        private bool _runStartUpOnEnable;
        private TrashRecyclerDataEntry _savedData;
        private bool _isFromSave;
        private DumpContainerSimplified _dumpContainer;
        private Recycler _recycler;
        private float _timeLeft;
        private ObjectPooler _pooler;
        private GridHelperPooled _gridHelper;
        private GameObject _homePage;
        private GameObject _inventoryPage;
        private Text _storageLabel;
        private Text _statusLabel;
        private Text _percentageLabel;
        private bool _isRecycling;
        private ProtobufSerializer _serializer;
        private const string RecyclerPoolTag = "recycleItem";
        private const float MaxRecycleTime = 30f;

        public override bool IsOperational => OperationalCheck();

        private bool OperationalCheck()
        {
            if (Manager == null && _isFromSave) return true;
            return IsInitialized && IsConstructed && Manager.HasEnoughPower(GetPowerUsage());
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.RecyclerTabID, Mod.ModName);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override IFCSStorage GetStorage()
        {
            return _recycler?.GetStorage();
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
                    QuickLogger.Debug("Trying to load recycler");
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }
                    _recycler.Load(_savedData, _serializer);
                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                    _timeLeft = _savedData.CurrentTime;
                    StartCoroutine(_dumpContainer.RestoreItems(_serializer, _savedData.DropStorage,true));
                    RefreshUI();
                    _isFromSave = false;
                }
                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetTrashRecyclerSaveData(GetPrefabID());
            BaseId = _savedData.BaseID;
        }

        private void Update()
        {
            if (!IsOperational || _recycler == null || !_isRecycling || _percentageLabel == null) return;

            _timeLeft += DayNightCycle.main.deltaTime;

            _percentageLabel.text = $"({Mathf.RoundToInt((_timeLeft / MaxRecycleTime) * 100)}%)";

            if (_timeLeft >= MaxRecycleTime)
            {
                _recycler?.Recycle();
                _timeLeft = 0;
                _isRecycling = false;

                if (_recycler.HasItems())
                {
                    TryStartRecycling();
                }
                else
                {
                    _percentageLabel.text = Language.main.Get("SubmarineEditDone");
                }
            }
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            _homePage = GameObjectHelpers.FindGameObject(gameObject, "Home");
            _inventoryPage = GameObjectHelpers.FindGameObject(gameObject, "Inventory");
            _storageLabel = GameObjectHelpers.FindGameObject(gameObject, "StorageLabel")?.GetComponent<Text>();
            _statusLabel = GameObjectHelpers.FindGameObject(gameObject, "StatusLabel")?.GetComponent<Text>();
            _statusLabel.text = Mod.RecyclerFriendly;
            _percentageLabel = GameObjectHelpers.FindGameObject(gameObject, "Percentage")?.GetComponent<Text>();
            
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform, AuxPatchers.TrashRecyclerDumpLabel(), this);
                _dumpContainer.OnDumpContainerClosed += TryStartRecycling;
            }

            if (_recycler == null)
            {
                _recycler = gameObject.AddComponent<Recycler>();
                _recycler.Initialize(this, 100);

                _storageLabel.text = AuxPatchers.TrashRecyclerStorageFormat(0, _recycler.MaxStorage);

                _recycler.OnContainerUpdated += () =>
                {
                    if (!_recycler.HasItems())
                    {
                        _statusLabel.text = Mod.RecyclerFriendly;
                        _percentageLabel.text = AuxPatchers.Waiting();
                    }
                    _storageLabel.text = AuxPatchers.TrashRecyclerStorageFormat(_recycler.GetCount(), _recycler.MaxStorage);
                };
            }

            if (_pooler == null)
            {
                _pooler = gameObject.AddComponent<ObjectPooler>();
                _pooler.AddPool(RecyclerPoolTag, 12, ModelPrefab.TrashRecyclerItemPrefab);
                _pooler.Initialize();
            }

            if (_gridHelper == null)
            {
                _gridHelper = gameObject.AddComponent<GridHelperPooled>();
                _gridHelper.OnLoadDisplay += OnLoadDisplay;
                _gridHelper.Setup(12, _pooler, GameObjectHelpers.FindGameObject(gameObject, "Inventory"), OnButtonClicked);
            }

            InterfaceHelpers.CreateButton(GameObjectHelpers.FindGameObject(gameObject, "OpenBTN"),
                "InventoryBTN", InterfaceButtonMode.Background, OnButtonClicked, Color.gray, Color.white, 5,
                "Open Inventory");

            InterfaceHelpers.CreateButton(GameObjectHelpers.FindGameObject(gameObject, "AddBTN"),
                "AddBTN", InterfaceButtonMode.Background, OnButtonClicked, Color.gray, Color.white, 5,
                "Add to item to recycler");

            IsInitialized = true;
            QuickLogger.Debug($"Initialization Complete: {GetPrefabID()} | ID {UnitID}", true);
        }

        internal void TryStartRecycling()
        {
            QuickLogger.Debug($"Has Items {_recycler.HasItems()} || Is Operational: {IsOperational} || Is Recycling: {_isRecycling}",true);
            if (_recycler.HasItems() && IsOperational && !_isRecycling)
            {
                _statusLabel.text = AuxPatchers.TrashRecyclerStatusFormat(Language.main.Get(_recycler.GetCurrentItem()));
                _isRecycling = true;
            }
        }

        private void OnLoadDisplay(DisplayDataPooled data)
        {
            data.Pool.Reset(RecyclerPoolTag);

            var grouped = _recycler.GetStorage().GetItemsWithin().OrderBy(x => x.Key).ToList();

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            QuickLogger.Debug($"Load Items: {grouped.Count} SP:{data.StartPosition} EP:{data.EndPosition}", true);

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                GameObject buttonPrefab = data.Pool.SpawnFromPool(RecyclerPoolTag, data.ItemsGrid);

                QuickLogger.Debug($"Button Prefab: {buttonPrefab}");

                if (buttonPrefab == null || data.ItemsGrid == null)
                {
                    return;
                }

                var item = buttonPrefab.EnsureComponent<TrashCollectorItem>();
                item.BtnName = "ItemBTN";
                item.ButtonMode = InterfaceButtonMode.Background;
                item.OnButtonClick -= OnButtonClicked;
                item.OnButtonClick += OnButtonClicked;
                item?.UpdateItem(grouped.ElementAt(i), _recycler);
            }
            _storageLabel.text = AuxPatchers.TrashRecyclerStorageFormat(_recycler.GetCount(), _recycler.MaxStorage);
            _gridHelper.UpdaterPaginator(grouped.Count);
        }

        private void OnButtonClicked(string btnName, object btnTag)
        {
            switch (btnName)
            {
                case "HomeBTN":
                    _homePage.SetActive(true);
                    _inventoryPage.SetActive(false);
                    break;

                case "InventoryBTN":
                    _homePage.SetActive(false);
                    _inventoryPage.SetActive(true);
                    break;
                case "AddBTN":
                    _dumpContainer.OpenStorage();
                    break;
                case "ItemBTN":
                    FCS_AlterraHub.Helpers.PlayerInteractionHelper.GivePlayerItem(_recycler.RemoveItem((TechType)btnTag));
                    break;
            }
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
            _serializer = serializer;
            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_recycler.HasItems())
            {
                reason = AuxPatchers.ModNotEmptyFormat(Mod.RecyclerFriendly);
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = true;
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Initialize();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new TrashRecyclerDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.BaseID = Manager.BaseID;
            _savedData.Storage = _recycler.Save(serializer,_savedData);
            _savedData.CurrentTime = _timeLeft;
            _savedData.IsRecycling = _isRecycling;
            _savedData.DropStorage = _dumpContainer.Save(serializer);
            newSaveData.TrashRecyclerEntries.Add(_savedData);

            QuickLogger.Debug($"Saved HoverPad ID {_savedData.Id}", true);
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            _recycler.AddItem(item);
            return true;
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            return false;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return _recycler.IsAllowedToAdd(pickupable);
        }

        public Recycler GetRecycler()
        {
            return _recycler;
        }

        public override void RefreshUI()
        {
            _gridHelper.DrawPage();
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override float GetPowerUsage()
        {
            return _isRecycling ? 0.85f : 0f;
        }

        public int GetFreeSpace()
        {
            return _recycler.GetStorage().GetFreeSpace();
        }
    }
}
