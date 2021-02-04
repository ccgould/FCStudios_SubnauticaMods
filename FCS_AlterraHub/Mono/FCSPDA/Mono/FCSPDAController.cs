using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers.Quests;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using FMODUnity;
using rail;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    internal class FCSPDAController : MonoBehaviour, IFCSDisplay
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
        private GridHelperV2 _inventoryGrid;
        private GridHelperV2 _basesGrid;
        private GameObject _home;
        private GameObject _bases;
        private GameObject _baseInventory;
        private Text _baseNameLBL;
        private Text _currentBiome;
        private Text _accountName;
        private Text _accountBalance;
        private bool _goToEncyclopedia;
        public PDAMissionController MissionController;
        private GameObject _missionPage;
        private List<Button> _padHomeButtons = new List<Button>();
        private GameObject _messagesPage;
        public MessagesController MessagesController;
        private bool _addtempMessage = true;
        private PaginatorController _inventoryPaginatorController;
        private PaginatorController _basePaginatorController;
        private bool _depthState;

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
            QuestManager.Instance.OnMissionAdded += quest =>
            {
                MissionController.UpdateQuest(quest);
            };


            _home = GameObjectHelpers.FindGameObject(gameObject, "Home");
            _missionPage = GameObjectHelpers.FindGameObject(gameObject, "Missions");
            
            _bases = GameObjectHelpers.FindGameObject(gameObject, "Bases");
            _baseInventory = GameObjectHelpers.FindGameObject(gameObject, "BasesInventory");
            _baseNameLBL = GameObjectHelpers.FindGameObject(gameObject, "BaseNameLBL").GetComponent<Text>();
            _currentBiome = GameObjectHelpers.FindGameObject(gameObject, "BiomeLBL").GetComponent<Text>();
            _accountName = GameObjectHelpers.FindGameObject(gameObject, "UserName").GetComponent<Text>();
            _accountBalance = GameObjectHelpers.FindGameObject(gameObject, "AccountBalance").GetComponent<Text>();
            var vpb = GameObjectHelpers.FindGameObject(gameObject, "Progress").AddComponent<VideoProgressBar>();
            vpb.VideoPlayer = gameObject.GetComponentInChildren<VideoPlayer>(); ;
            var stopButton = GameObjectHelpers.FindGameObject(gameObject, "StopButton").GetComponent<Button>();
            stopButton.onClick.AddListener((() => { vpb.Stop();}));
            var canvas = gameObject.GetComponentInChildren<Canvas>();

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

            var baseBTNObj = GameObjectHelpers.FindGameObject(gameObject, "BasesButton");
            var baseBTNToolTip =baseBTNObj.AddComponent<FCSToolTip>();
            baseBTNToolTip.RequestPermission += () => true;
            baseBTNToolTip.Tooltip = "Allows you to pull items from online bases requires connection chip";
            var basesButton = baseBTNObj.GetComponent<Button>();
            basesButton.onClick.AddListener(() =>
            {
                _bases.SetActive(true);
                _home.SetActive(false);
                _baseInventory.SetActive(false);
            });

            var encyclopObj = GameObjectHelpers.FindGameObject(gameObject, "EncyclopediaButton");
            var encyclopToolTip = encyclopObj.AddComponent<FCSToolTip>();
            encyclopToolTip.Tooltip = "Opens the PDA to the encyclopedia tab";
            encyclopToolTip.RequestPermission += () => true;
            var encyclopediaButton = encyclopObj.GetComponent<Button>();
            encyclopediaButton.onClick.AddListener(() =>
            {
                Close();
                _goToEncyclopedia = true;
            });

            var settingsButton = GameObjectHelpers.FindGameObject(gameObject, "SettingsButton").GetComponent<Button>();
            settingsButton.onClick.AddListener(() =>
            {
                QuickLogger.ModMessage("Page Not Implemented");
            });
            
            var messagesButton = GameObjectHelpers.FindGameObject(gameObject, "MessagesButton").GetComponent<Button>();
            messagesButton.onClick.AddListener(() =>
            {
                _home.SetActive(false);
                _messagesPage.SetActive(true);
            });

            var contactsButton = GameObjectHelpers.FindGameObject(gameObject, "ContactsButton").GetComponent<Button>();
            contactsButton.onClick.AddListener(() =>
            {
                QuickLogger.ModMessage("Page Not Implemented");
            });

            var messagesBackBTN = GameObjectHelpers.FindGameObject(gameObject, "MessagesBackBTN").GetComponent<Button>();
            messagesBackBTN.onClick.AddListener(() =>
            {
                _home.SetActive(true);
                _messagesPage.SetActive(false);
            });

            var missionsBackBTN = GameObjectHelpers.FindGameObject(gameObject, "MissionsBackBTN").GetComponent<Button>();
            missionsBackBTN.onClick.AddListener(() =>
            {
                _home.SetActive(true);
                _missionPage.SetActive(false);
            });

            var missionsButton = GameObjectHelpers.FindGameObject(gameObject, "MissionsButton").GetComponent<Button>();
            missionsButton.onClick.AddListener(() =>
            {
                _home.SetActive(false);
                _missionPage.SetActive(true);
            });


            _basesGrid = gameObject.AddComponent<GridHelperV2>();
            _basesGrid.OnLoadDisplay += OnLoadBasesGrid;
            _basesGrid.Setup(10, gameObject, Color.gray, Color.white, OnButtonClick);

            _inventoryGrid = gameObject.AddComponent<GridHelperV2>();
            _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoryGrid.Setup(15, gameObject, Color.gray, Color.white, OnButtonClick);

            _clock = GameObjectHelpers.FindGameObject(gameObject, "Clock").GetComponent<Text>();

            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseEmissiveDecalsController, gameObject,
                Color.cyan);

            _basePaginatorController = GameObjectHelpers.FindGameObject(gameObject, "BasePaginator").AddComponent<PaginatorController>();
            _basePaginatorController.Initialize(this);

            _inventoryPaginatorController = GameObjectHelpers.FindGameObject(gameObject, "InventoryPaginator").AddComponent<PaginatorController>();
            _inventoryPaginatorController.Initialize(this);

            _addtempMessage = false;

            InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
        }

        private void CreateMessagesController()
        {
            _messagesPage = GameObjectHelpers.FindGameObject(gameObject, "Messages");
            MessagesController = _messagesPage.AddComponent<MessagesController>();
            MessagesController.Initialize(this);
        }

        internal void CreateMissionController()
        {
            CreateMessagesController();

            MissionController = gameObject?.EnsureComponent<PDAMissionController>();
            if (MissionController == null)
            {
                QuickLogger.Error<FCSPDAController>("Mission Controller is null", true);
                return;
            }
            MissionController.Initialize();
            if (QuestManager.Instance == null)
            {
                QuickLogger.Error<FCSPDAController>("Quest Manager Instance is null",true);
                return;
            }

            if (MessagesController == null)
            {
                QuickLogger.Error<FCSPDAController>("Messages Controller is null", true);
                return;
            }

        }

        private void UpdateDisplay()
        {
            if (_inventoryGrid == null || _basesGrid == null || _currentBiome == null || _accountName == null) return;
            _basesGrid.DrawPage();
            _inventoryGrid.DrawPage();
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
            _depthState = UwePostProcessingManager.GetDofEnabled();

            UwePostProcessingManager.ToggleDof(false);

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
            if (_pda.screen.activeSelf)
            {
                _pda.screen.SetActive(false);
            }
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
            UwePostProcessingManager.ToggleDof(_depthState);
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
            OnClose?.Invoke();
            if (_goToEncyclopedia)
            {
                Player.main.GetPDA().Open(PDATab.Encyclopedia);
                _goToEncyclopedia = false;
            }
        }

        public Action OnClose { get; set; }
        public Channel AudioTrack { get; set; }

        private void FindPDA()
        {
            QuickLogger.Debug("In Find PDA");
            if (PdaCanvas == null)
            {
                PdaCanvas = PDAObj?.GetComponent<PDA>()?.screen?.gameObject?.GetComponent<Canvas>();
                Player main = Player.main;
                _pda = main.GetPDA();
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

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                var g = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[g++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

                _inventoryGrid.UpdaterPaginator(grouped.Count);
                _inventoryPaginatorController.ResetCount(_inventoryGrid.GetMaxPages());
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

                var grouped = BaseManager.Managers.Where(x => x != null && !x.GetBaseName().Equals("Cyclops 0")).ToList();

                //if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                //{
                //    grouped = grouped.Where(p => Language.main.Get(p.Key).StartsWith(_currentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                //}

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _baseButtons[i].Reset();
                }

                var g = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {

                    _baseButtons[g++].Set(grouped[i]);
                }

                _basesGrid.UpdaterPaginator(grouped.Count);
                _basePaginatorController.ResetCount(_basesGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        public void GoToPage(int index)
        {
 
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            if (sender == _inventoryPaginatorController)
            {
                _inventoryGrid.DrawPage(index);
            }
            else if(sender == _basePaginatorController)
            {
                QuickLogger.Debug($"Refreshing Base Grid going to page {index} | Controller {sender.gameObject.name}",true);
                _basesGrid.DrawPage(index);
            }
        }
    }

    internal class MessagesController : MonoBehaviour
    {
        private bool _initialized;
        private GameObject _messageList;
        private readonly List<AudioMessage> _messages = new List<AudioMessage>();
        private Text _messageCounter;
        private FCSPDAController _pdaController;

#if SUBNAUTICA
        private static FMOD.System FMOD_System => RuntimeManager.LowlevelSystem;
#else
        private static System FMOD_System => RuntimeManager.CoreSystem;
#endif

        internal void Initialize(FCSPDAController fcsPdaController)
        {
            if (_initialized) return;
            _messageList = GameObjectHelpers.FindGameObject(gameObject, "Messageslist");
            _pdaController = fcsPdaController;
            _messageCounter = GameObjectHelpers.FindGameObject(fcsPdaController.gameObject, "MessagesCounter").GetComponent<Text>();
            InvokeRepeating(nameof(UpdateMessageCounter), 1, 1);
            _initialized = true;
        }
        
        private void UpdateMessageCounter()
        {
            _messageCounter.text = _messages.Count(x => !x.HasBeenPlayed).ToString();
        }

        internal void AddNewMessage(string description,string from, string audioClipName, bool hasBeenPlayed = false)
        {
            var message = new AudioMessage(description, audioClipName) {HasBeenPlayed = hasBeenPlayed};
            _messages.Add(message);
            if (!hasBeenPlayed)
                uGUI_PowerIndicator_Initialize_Patch.MissionHUD.ShowNewMessagePopUp(from);
            RefreshUI();
        }

        private void RefreshUI()
        {
            for (int i = _messageList.transform.childCount - 1; i > 0; i--)
            {
                Destroy(_messageList.transform.GetChild(i).gameObject);
            }

            foreach (AudioMessage message in _messages)
            {
                var prefab = Instantiate(Buildables.AlterraHub.PDAEntryPrefab);
                var messageController = prefab.AddComponent<MessagePDAEntryController>();
                messageController.Initialize(message,this);
                prefab.transform.SetParent(_messageList.transform, false);
            }
        }

        public void PlayAudioTrack(string trackName)
        {
            if (string.IsNullOrEmpty(trackName))
            {
                QuickLogger.Debug("Track returned null",true);
                return;
            }

            CurrentTrackSound.isPlaying(out bool isPlaying);

            if (isPlaying)
            {
                CurrentTrackSound.stop();
            }

             _pdaController.AudioTrack = AudioUtils.PlaySound(QuestManager.Instance.FindAudioClip(trackName), SoundChannel.Voice);
        }

        public Channel CurrentTrackSound { get; set; }
    }

    internal class MessagePDAEntryController : MonoBehaviour
    {
        private bool _isInitialized;
       
        internal void Initialize(AudioMessage message, MessagesController messagesController)
        {
            if(_isInitialized) return;
            GetComponentInChildren<Text>().text = message.Description;
            GetComponentInChildren<Button>().onClick.AddListener((() =>
            {
                messagesController.PlayAudioTrack(message.AudioClipName);
                message.HasBeenPlayed = true;
                QuestManager.Instance.CreateStarterMission();

            }));
            _isInitialized = true;
        }
    }

    internal class AudioMessage    
    {
        public string Description { get; set; }
        public string AudioClipName { get; set; }
        public bool HasBeenPlayed { get; set; }
        public AudioMessage(string description,string audioClipName)
        {
            AudioClipName = audioClipName;
            Description = description;
        }
    }

    public struct FCSEncyclopediaData
    {
        public string Title { get; set; }
        public Atlas.Sprite Image { get; set; }
        public string Body { get; set; }
        public string VideoLink { get; set; }
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

    public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        internal VideoPlayer VideoPlayer { get; set; }
        [SerializeField]
        private Camera camera;

        private Image progress;
        private void Awake()
        {
            camera = Player.main.viewModelCamera;
            progress = GetComponent<Image>();
        }

        void Update()
        {
            if(VideoPlayer == null) return;
            
            VideoPlayer.playbackSpeed = Mathf.Approximately(DayNightCycle.main.deltaTime, 0f) ? 0 : 1;
            
            if (VideoPlayer.frameCount > 0)
                progress.fillAmount = (float)VideoPlayer.frame / (float)VideoPlayer.frameCount;
        }

        public void OnDrag(PointerEventData eventData)
        {
            TrySkip(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TrySkip(eventData);
        }

        private void TrySkip(PointerEventData eventData)
        {
            if(progress == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(progress.rectTransform, eventData.position, camera,
                out var localPoint))
            {
                float pct = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax,localPoint.x);
                SkipToPercent(pct);
            }
        }

        private void SkipToPercent(float pct)
        {
            if (VideoPlayer == null) return;
            var frame = VideoPlayer.frameCount * pct;
            VideoPlayer.frame = (long)frame;
        }

        public void Stop()
        {
            SkipToPercent(0);
            progress.fillAmount = 0;
        }
    }
}