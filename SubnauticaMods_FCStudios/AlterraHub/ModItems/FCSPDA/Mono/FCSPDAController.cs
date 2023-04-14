using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Patches;
using FCSCommon.Utilities;
using FMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono
{
    public class FCSPDAController : MonoBehaviour
    {
        private PDA _pda;
        private int prevQuickSlot;
        private Sequence sequence = new(false);
        private GameObject _inputDummy;
        private uGUI_InputGroup _ui;

        private bool _isBeingDestroyed;

        private bool _depthState;


        private bool _isInitialized;
        private Canvas _canvas;

        public GameObject PDAObj { get; set; }
        public float cameraFieldOfView = 62f;
        public float cameraFieldOfViewAtFourThree = 66f;
        public Canvas PdaCanvas { get; set; }
        internal bool IsOpen { get; private set; }
        public Action OnClose { get; set; }
        public Channel AudioTrack { get; set; }
        public bool isFocused => this.ui != null && this.ui.focused;
        public uGUI_InputGroup ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = _canvas.gameObject.GetComponentInChildren<Canvas>(true).gameObject.AddComponent<uGUI_InputGroup>();
                }
                return _ui;
            }
        }

        #region SINGLETON PATTERN
        private List<MeshRenderer> _pdaMeshes = new();

        private Transform _pdaAnchor;
        private uGUI_CanvasScaler _canvasScalar;
        public static FCSPDAController Main;

        private int _timesOpen;

        internal FCSAlterraHubGUI Screen;
        private bool _goToEncyclopedia;
        private GameObject _screen;

        #endregion

        private void Awake()
        {
            if (Main == null)
            {
                Main = this;
                DontDestroyOnLoad(this);
            }
            else if (Main != null)
            {
                Destroy(gameObject);
                return;
            }

            EncyclopediaService.OnOpenEncyclopedia += OnOpenEncyclopedia;
        }

        private void OnOpenEncyclopedia(TechType techType)
        {
            ForceOpen();
            Screen.OpenEncyclopedia(techType);
        }

        public bool Open()
        {
            Player main = Player.main;

            QuickLogger.Debug("PDA Open : 1");

            main.GetPDA().sequence.ForceState(true);

            PlayAppropriateVoiceMessage();

            Screen.TryRemove404Screen();

            Screen.RefreshTeleportationPage();

            FindPDA();

            ChangePDAVisibility(false);

            //TODO I dont know why i did this
            //Screen.AttemptToOpenReturnsDialog();

            Screen.UpdateDisplay();

            PatchAdditionalPages();

            DOFOperations();

            SetPDAInUse(true);

            if (!DetemineIfInCinematicMode(main)) return false;

            SetRequiredParametersToOpenPDA();

            QuickLogger.Debug("FCS PDA Is Open", true);
            return true;
        }

        internal void CreateScreen()
        {
            if (_screen == null)
            {
               _screen = Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("uGUI_PDAScreen"));
                Screen = _screen.AddComponent<FCSAlterraHubGUI>();
                Screen.SetInstance(FCSAlterraHubGUISender.PDA);
            }
        }

        public FCSAlterraHubGUI GetGUI()
        {
            if(_FCSPDAUI is null)
            {
                _FCSPDAUI =  _screen.GetComponent<FCSAlterraHubGUI>();
            }
            return _FCSPDAUI;
        }

        public void Close()
        {
            IsOpen = false;

            ResetToHome();

            ChangePDAVisibility(true);
            gameObject.SetActive(false);
            SetPDAInUse(false);
            Player main = Player.main;
            main.GetPDA().sequence.ForceState(false);
            MainCameraControl.main.ResetLockedVRViewModelAngle();
            Screen.gameObject.SetActive(false);
            Vehicle vehicle = main.GetVehicle();
            if (vehicle != null)
            {
                uGUI.main.quickSlots.SetTarget(vehicle);
            }

            Screen.CloseAccountPage();


#if SUBNAUTICA_STABLE
            MainGameController.Instance.PerformGarbageAndAssetCollection();
#else
            MainGameController.Instance.PerformIncrementalGarbageCollection();
#endif
            HandReticle.main?.UnrequestCrosshairHide();
            Inventory.main.SetViewModelVis(true);


            //sequence.Set(0.5f, false, Deactivated);

            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", false);
            ui.Deselect(null);
            UwePostProcessingManager.ClosePDA();
#if SUBNAUTICA
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundClose);
#else
#endif
            UwePostProcessingManager.ToggleDof(_depthState);
            QuickLogger.Debug("FCS PDA Is Closed", true);
        }

        private void ResetToHome()
        {
            if (Screen.CurrentPage() == PDAPages.DevicePage || Screen.CurrentPage() == PDAPages.DeviceSettings)
                Screen.PurgePages();
        }

        internal void SetInstance()
        {
            if (_isInitialized) return;
                      
            _pdaAnchor = GameObjectHelpers.FindGameObject(gameObject, "ScreenAnchor").transform;

            _canvasScalar = Screen.gameObject.AddComponent<uGUI_CanvasScaler>();
            var raycaster = Screen.gameObject.AddComponent<uGUI_GraphicRaycaster>();
            raycaster.guiCameraSpace = true;
            raycaster.ignoreReversedGraphics = false;

            _canvasScalar.mode = uGUI_CanvasScaler.Mode.Inversed;
            _canvasScalar.vrMode = uGUI_CanvasScaler.Mode.Inversed;
            _canvasScalar.SetAnchor(_pdaAnchor.transform);


            _canvas = Screen.GetComponentInChildren<Canvas>();
            _canvas.sortingLayerName = "PDA";
            _canvas.sortingLayerID = 1479780821;
            //MaterialHelpers.ApplyEmissionShader(AlterraHub.BasePrimaryCol, gameObject, Color.white, 0, 0.01f, 0.01f);
            //MaterialHelpers.ApplySpecShader(AlterraHub.BasePrimaryCol, gameObject, 1, 6.15f);
            //MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            InGameMenuQuitPatcher.AddEventHandlerIfMissing(OnQuit);
            Screen.gameObject.SetActive(false);
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            EncyclopediaService.OnOpenEncyclopedia -= OnOpenEncyclopedia;
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

            if (IsOpen && this.isFocused && (GameInput.GetButtonDown(GameInput.Button.PDA) || Input.GetKeyDown(FCS_AlterraHub.Main.Configuration.FCSPDAKeyCode)))
            {
                this.Close();
                return;
            }

            FPSInputModule.current.EscapeMenu();
        }

        private void SetRequiredParametersToOpenPDA()
        {
            MainCameraControl.main.SaveLockedVRViewModelAngle();
            IsOpen = true;
            gameObject.SetActive(true);
            sequence.Set(0.5f, true, Activated);
            UWE.Utils.lockCursor = false;
            HandReticle.main?.RequestCrosshairHide();

            if (HandReticle.main?.hideCount > 1)
            {
                QuickLogger.Debug("Fixing Hide Count", true);
                while (HandReticle.main.hideCount > 1)
                {
                    HandReticle.main?.UnrequestCrosshairHide();
                }
            }

            Inventory.main.SetViewModelVis(false);
            Screen.gameObject.SetActive(true);
            UwePostProcessingManager.OpenPDA();
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", true);

            //#if SUBNAUTICA
            //            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundOpen);
            //            if (_pda.screen.activeSelf)
            //            {
            //                _pda.screen.SetActive(false);
            //            }
            //#else
            //#endif
        }

        private bool DetemineIfInCinematicMode(Player main)
        {
            var flag = InventorySlotHandler();

            if (!flag || main.cinematicModeActive)
            {
                return false;
            }
            return true;
        }

        private void SetPDAInUse(bool isInUse)
        {
            _pda.isInUse = isInUse;
        }

        private void PlayAppropriateVoiceMessage()
        {
            //if (_timesOpen > 0 && !CardSystem.main.HasBeenRegistered() && !Mod.GamePlaySettings.IsPDAOpenFirstTime &&
            //    DroneDeliveryService.Main.DetermineIfFixed())
            //{
            //    VoiceNotificationSystem.main.Play("PDA_Account_Instructions_key", 26);
            //}

            //if (Mod.GamePlaySettings.IsPDAOpenFirstTime && DroneDeliveryService.Main.DetermineIfFixed())
            //{
            //    VoiceNotificationSystem.main.Play("PDA_Instructions_key", 26);
            //    Mod.GamePlaySettings.IsPDAOpenFirstTime = false;
            //    _timesOpen++;
            //    Mod.SaveGamePlaySettings();
            //}
        }

        private bool InventorySlotHandler()
        {
            uGUI.main.quickSlots.SetTarget(null);
            prevQuickSlot = Inventory.main.quickSlots.activeSlot;
            bool flag = Inventory.main.ReturnHeld();
            return flag;
        }

        private void DOFOperations()
        {
            _depthState = UwePostProcessingManager.GetDofEnabled();

            UwePostProcessingManager.ToggleDof(false);
        }

        private void ChangePDAVisibility(bool value)
        {
            _pda.gameObject.SetActive(!value);
            foreach (var meshRenderer in _pdaMeshes)
            {
                meshRenderer.enabled = value;
            }
        }

        public void Activated()
        {
            ui.Select();
        }

        public void Deactivated()
        {
            SNCameraRoot.main.SetFov(0f);
            OnClose?.Invoke();
            gameObject.SetActive(false);

            if (!_goToEncyclopedia)
            {
                Inventory.main.quickSlots.Select(prevQuickSlot);
            }
            else
            {
                Player.main.GetPDA().Open(PDATab.Encyclopedia);
                _goToEncyclopedia = false;
            }
        }

        private void FindPDA()
        {
            QuickLogger.Debug("In Find PDA", true);
            if (PdaCanvas == null)
            {
#if SUBNAUTICA
                PdaCanvas = PDAObj?.GetComponent<PDA>()?._ui?.gameObject?.GetComponent<Canvas>();
#else
#endif
                Player main = Player.main;
                _pda = main.GetPDA();
            }

            foreach (MeshRenderer meshRenderer in _pda.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                _pdaMeshes.Add(meshRenderer);
            }
        }

        private void OnQuit()
        {
            //Mod.DeepCopySave(CardSystem.main.SaveDetails());
            QuickLogger.Debug("Quitting Purging CardSystem and AlterraHubSave", true);
            AccountService.main.Purge();
            //Mod.PurgeSave();
        }

        internal void Save()
        {
            GamePlayService.Main.SetShipmentInfo(Screen.GetShipmentInfo());
        }

        internal void LoadFromSave()
        {
            Screen.LoadFromSave(GamePlayService.Main.GetShipmentInfo());
        }

        public static void ForceOpen()
        {
            Player_Patches.ForceOpenPDA = true;
        }

        public static void ForceClose()
        {
            Main.Close();
        }

        static Dictionary<TechType,GameObject> additionPages = new();
        private FCSAlterraHubGUI _FCSPDAUI;

        public static void AddAdditionalPage<T>(TechType id,GameObject ui) where T : Component
        {
            ui.EnsureComponent<T>();
            additionPages.Add(id,ui); 
        }

        private void PatchAdditionalPages()
        {
            for (int a = additionPages.Count - 1; a >= 0; a--)
            {
                var page = additionPages.ElementAt(a);

                Screen.AddAdditionalPage(page.Key,page.Value);
                additionPages.Remove(page.Key);
            }
        }

        /// <summary>
        /// Displays the UI Page in the PDA
        /// </summary>
        /// <param name="id">Id of the UI to display</param>
        /// <param name="fcsDevice">The device to change the settings on.</param>
        public void OpenDeviceUI(TechType id, FCSDevice fcsDevice)
        {
            Open();
            Screen.PrepareDevicePage(id, fcsDevice);
        }
    }
}
