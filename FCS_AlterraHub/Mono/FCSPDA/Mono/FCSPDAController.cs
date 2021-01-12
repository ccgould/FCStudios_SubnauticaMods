using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    internal class FCSPDAController : MonoBehaviour
    {
        private PDA _pda;
        private int prevQuickSlot;
        public GameObject PDAObj { get; set; }
        private GameObject Mesh { get; set; }
        private Sequence sequence = new Sequence(false);
        public Canvas PdaCanvas { get; set; }
        internal bool IsOpen { get; private set; }
        private BaseManager _currentBase;

        public float cameraFieldOfView = 62f;
        public float cameraFieldOfViewAtFourThree = 66f;
        private GameObject _inputDummy;
        private uGUI_InputGroup _ui;
        private Text _clock;
        private bool _isBeingDestroyed;
        private List<DSSInventoryItem> _inventoryButtons = new List<DSSInventoryItem>();
        private List<DSSBaseItem> _baseButtons = new List<DSSBaseItem>();
        private GridHelperV2 _inventoyGrid;
        private GridHelperV2 _basesGrid;
        private GameObject _home;
        private GameObject _bases;
        private GameObject _baseInventory;
        private Text _baseNameLBL;
        private Text _currentBiome;
        private Text _accountName;
        private Text _accountBalance;


        public bool isFocused => this.ui != null && this.ui.focused;

        public uGUI_InputGroup ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = gameObject.GetComponentInChildren<Canvas>(true).gameObject.AddComponent<uGUI_InputGroup>();
                }
                return _ui;
            }
        }


        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }
        private void Update()
        {
            sequence.Update();
            if (sequence.active)
            {
                float b = (SNCameraRoot.main.mainCamera.aspect > 1.5f) ? cameraFieldOfView : cameraFieldOfViewAtFourThree;
                SNCameraRoot.main.SetFov(Mathf.Lerp(MiscSettings.fieldOfView, b, sequence.t));
            }
            

            if (!ui.selected && IsOpen && AvatarInputHandler.main.IsEnabled())
            {
                ui.Select(false);
            }

            if (IsOpen && this.isFocused && (GameInput.GetButtonDown(GameInput.Button.PDA) || Input.GetKeyDown(QPatch.Configuration.FCSPDAKeyCode)))
            {
                this.Close();
                return;
            }

            if (_clock != null)
            {
                _clock.text = WorldHelpers.GetGameTimeFormat();
            }

            FPSInputModule.current.EscapeMenu();
        }

        private void Start()
        {
            _home = GameObjectHelpers.FindGameObject(gameObject, "Home");
            _bases = GameObjectHelpers.FindGameObject(gameObject, "Bases");
            _baseInventory = GameObjectHelpers.FindGameObject(gameObject, "BasesInventory");
            _baseNameLBL = GameObjectHelpers.FindGameObject(gameObject, "BaseNameLBL").GetComponent<Text>();
            _currentBiome = GameObjectHelpers.FindGameObject(gameObject, "BiomeLBL").GetComponent<Text>();
            _accountName = GameObjectHelpers.FindGameObject(gameObject, "UserName").GetComponent<Text>();
            _accountBalance = GameObjectHelpers.FindGameObject(gameObject, "AccountBalance").GetComponent<Text>();
            
            foreach (Transform invItem in GameObjectHelpers.FindGameObject(_baseInventory, "Grid").transform)
            {
                var invButton = invItem.gameObject.EnsureComponent<DSSInventoryItem>();
                invButton.ButtonMode = InterfaceButtonMode.HoverImage;
                invButton.BtnName = "InventoryBTN";
                invButton.OnButtonClick += OnButtonClick;
                _inventoryButtons.Add(invButton);
            }
            
            foreach (Transform baseItem in GameObjectHelpers.FindGameObject(_bases, "Grid").transform)
            {
                var baseButton = baseItem.gameObject.EnsureComponent<DSSBaseItem>();
                baseButton.ButtonMode = InterfaceButtonMode.HoverImage;
                baseButton.BtnName = "BaseBTN";
                baseButton.OnButtonClick += OnButtonClick;
                _baseButtons.Add(baseButton);
            }

            var baseInventoryBackButton = GameObjectHelpers.FindGameObject(_baseInventory,"ReturnBTN").GetComponent<Button>();
            baseInventoryBackButton.onClick.AddListener(() =>
            {
                _bases.SetActive(true);
                _home.SetActive(false);
                _baseInventory.SetActive(false);
                UpdateDisplay();
            });

            var homeButton = GameObjectHelpers.FindGameObject(_baseInventory,"HomeBTN").GetComponent<Button>();
            homeButton.onClick.AddListener(() =>
            {
                _bases.SetActive(false);
                _home.SetActive(true);
                _baseInventory.SetActive(false);
            });

            var basesBackButton = GameObjectHelpers.FindGameObject(_bases,"ReturnBTN").GetComponent<Button>();
            basesBackButton.onClick.AddListener(() =>
            {
                _bases.SetActive(false);
                _home.SetActive(true);
                _baseInventory.SetActive(false);
            });


            var basesButton = GameObjectHelpers.FindGameObject(gameObject, "BasesButton").GetComponent<Button>();
            basesButton.onClick.AddListener(() =>
            {
                _bases.SetActive(true);
                _home.SetActive(false);
                _baseInventory.SetActive(false);
            });

            var encyclopediaButton = GameObjectHelpers.FindGameObject(gameObject, "EncyclopediaButton").GetComponent<Button>();
            encyclopediaButton.onClick.AddListener(() =>
            {
                QuickLogger.ModMessage("Page Not Implemented");
            });

            var settingsButton = GameObjectHelpers.FindGameObject(gameObject, "SettingsButton").GetComponent<Button>();
            settingsButton.onClick.AddListener(() =>
            {
                QuickLogger.ModMessage("Page Not Implemented");
            });

            var messagesButton = GameObjectHelpers.FindGameObject(gameObject, "MessagesButton").GetComponent<Button>();
            messagesButton.onClick.AddListener(() =>
            {
                QuickLogger.ModMessage("Page Not Implemented");
            });

            var contactsButton = GameObjectHelpers.FindGameObject(gameObject, "ContactsButton").GetComponent<Button>();
            contactsButton.onClick.AddListener(() =>
            {
                QuickLogger.ModMessage("Page Not Implemented");
            });

            var fuanaButton = GameObjectHelpers.FindGameObject(gameObject, "FuanaButton").GetComponent<Button>();
            fuanaButton.onClick.AddListener(() =>
            {
                QuickLogger.ModMessage("Page Not Implemented");
            });


            _basesGrid = gameObject.AddComponent<GridHelperV2>();
            _basesGrid.OnLoadDisplay += OnLoadBasesGrid;
            _basesGrid.Setup(10, gameObject, Color.gray, Color.white, OnButtonClick);

            _inventoyGrid = gameObject.AddComponent<GridHelperV2>();
            _inventoyGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoyGrid.Setup(15, gameObject, Color.gray, Color.white, OnButtonClick);

            _clock = GameObjectHelpers.FindGameObject(gameObject, "Clock").GetComponent<Text>();

            InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
        }

        private void UpdateDisplay()
        {
            if (_inventoyGrid == null || _basesGrid == null || _currentBiome == null || _accountName == null) return;
            _basesGrid.DrawPage();
            _inventoyGrid.DrawPage();
            _currentBiome.text = Player.main.GetBiomeString();
            _accountName.text = CardSystem.main.GetUserName();
            _accountBalance.text = $"{CardSystem.main.GetAccountBalance():N0}";
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "InventoryBTN":
                    var techType = (TechType) arg2;
                    if (PlayerInteractionHelper.CanPlayerHold(techType))
                    {
                        var pickup = _currentBase?.TakeItem(techType);
                        PlayerInteractionHelper.GivePlayerItem(pickup);
                        UpdateDisplay();
                    }
                    else
                    {
                        QuickLogger.ModMessage(Buildables.AlterraHub.InventoryFull());
                    }
                    break;

                case "BaseBTN":
                    _bases.SetActive(false);
                    _home.SetActive(false);
                    _baseInventory.SetActive(true);
                    _currentBase = (BaseManager) arg2;
                    _baseNameLBL.text = _currentBase.GetBaseName();
                    UpdateDisplay();
                    break;
            }
        }

        internal void OnEnable()
        {
            QuickLogger.Debug($"FCS PDA: Active and Enabled {isActiveAndEnabled}",true);
        }

        internal void Open()
        {
            FindPDA();
            _pda.isInUse = true;
            uGUI.main.quickSlots.SetTarget(null);
            prevQuickSlot = Inventory.main.quickSlots.activeSlot;
            bool flag = Inventory.main.ReturnHeld();
            Player main = Player.main;
            if (!flag || main.cinematicModeActive)
            {
                return;
            }
            
            MainCameraControl.main.SaveLockedVRViewModelAngle();
            IsOpen = true;
            gameObject.SetActive(true);
            sequence.Set(0.5f, true, Activated);
            UWE.Utils.lockCursor = false;
            if (HandReticle.main != null)
            {
                HandReticle.main.RequestCrosshairHide();
            }
            Inventory.main.SetViewModelVis(false);
            UwePostProcessingManager.OpenPDA();
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", true);
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundOpen);

            QuickLogger.Debug("FCS PDA Is Open", true);
        }

        private void FindMesh()
        {
            if (Mesh == null)
            {
                Mesh = PDAObj.FindChild("Mesh");
            }
        }

        internal void Close()
        {
            IsOpen = false;
            _pda.isInUse = false;
            Player main = Player.main;
            MainCameraControl.main.ResetLockedVRViewModelAngle();
            Vehicle vehicle = main.GetVehicle();
            if (vehicle != null)
            {
                uGUI.main.quickSlots.SetTarget(vehicle);
            }

            MainGameController.Instance.PerformGarbageCollection();
            if (HandReticle.main != null)
            {
                HandReticle.main.UnrequestCrosshairHide();
            }
            Inventory.main.SetViewModelVis(true);
            sequence.Set(0.5f, false, Deactivated);
            
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", false);
            ui.Deselect(null);
            UwePostProcessingManager.ClosePDA();
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundClose);
            QuickLogger.Debug("FCS PDA Is Closed", true);
        }

        public void Activated()
        {
            ui.Select();
        }

        public void Deactivated()
        {
            Inventory.main.quickSlots.Select(prevQuickSlot);
            gameObject.SetActive(false);
            SNCameraRoot.main.SetFov(0f);
        }

        private void FindPDA()
        {
            QuickLogger.Debug("In Find PDA");
            if (PdaCanvas == null)
            {
                QuickLogger.Debug("1");
                PdaCanvas = PDAObj?.GetComponent<PDA>()?.screen?.gameObject?.GetComponent<Canvas>();
                QuickLogger.Debug("2");
                Player main = Player.main;
                QuickLogger.Debug("3");
                _pda = main.GetPDA();
                QuickLogger.Debug("4");

            }
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _currentBase == null) return;

                var grouped = _currentBase.GetItemsWithin().OrderBy(x => x.Key).ToList();

                //if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                //{
                //    grouped = grouped.Where(p => Language.main.Get(p.Key).StartsWith(_currentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                //}

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[i].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadBasesGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                var grouped = BaseManager.Managers.OrderBy(x => x.GetBaseName()).ToList();

                //if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                //{
                //    grouped = grouped.Where(p => Language.main.Get(p.Key).StartsWith(_currentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                //}

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    _baseButtons[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _baseButtons[i].Set(grouped[i]);
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }
    }

    internal class DSSInventoryItem : InterfaceButton
    {
        private uGUI_Icon _icon;
        private Text _amount;

        private void Initialize()
        {
            if (_icon == null)
            {
                _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            }

            if (_amount == null)
            {
                _amount = gameObject.FindChild("Text").EnsureComponent<Text>();
            }
        }

        internal void Set(TechType techType, int amount)
        {
            Initialize();
            Tag = techType;
            _amount.text = amount.ToString();
            _icon.sprite = SpriteManager.Get(techType);
            Show();
        }

        internal void Reset()
        {
            Initialize();
            _amount.text = "";
            _icon.sprite = SpriteManager.Get(TechType.None);
            Tag = null;
            Hide();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }
    }

    internal class DSSBaseItem : InterfaceButton
    {
        private GameObject _hubIcon;
        private GameObject _cyclopsIcon;
        private Text _baseName;

        private void Initialize()
        {
            _hubIcon = GameObjectHelpers.FindGameObject(gameObject, "Habitat");
            _cyclopsIcon = GameObjectHelpers.FindGameObject(gameObject, "Cyclops");

            if (_baseName == null)
            {
                _baseName = gameObject.FindChild("Text").EnsureComponent<Text>();
            }
        }

        internal void Set(BaseManager baseManager)
        {
            Initialize();
            Tag = baseManager;
            _baseName.text = baseManager.GetBaseName();

            if (baseManager.Habitat.isCyclops)
            {
                _cyclopsIcon.SetActive(true);
                _hubIcon.SetActive(false);
            }
            else
            {
                _cyclopsIcon.SetActive(false);
                _hubIcon.SetActive(true);
            }
            Show();
        }

        internal void Reset()
        {
            Initialize();
            _baseName.text = "";
            _cyclopsIcon.SetActive(false);
            _hubIcon.SetActive(true);
            Tag = null;
            Hide();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
