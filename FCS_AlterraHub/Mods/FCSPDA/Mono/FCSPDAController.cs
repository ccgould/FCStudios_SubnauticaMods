using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers.FCSAlterraHub;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;
using PDAPages = FCS_AlterraHub.Mods.FCSPDA.Enums.PDAPages;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono
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
        }

        public bool Open()
        {
            QuickLogger.Debug("PDA Open : 1");

            Player main = Player.main;

            main.GetPDA().sequence.ForceState(true);

            PlayAppropriateVoiceMessage();

            Screen.TryRemove404Screen();

            Screen.RefreshTeleportationPage();

            CreateScreen();

            FindPDA();

            ChangePDAVisibility(false);

            Screen.AttemptToOpenReturnsDialog();

            Screen.UpdateDisplay();

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
                _screen = Instantiate(AlterraHub.PDAScreenPrefab);
            }
        }

        public void Close()
        {
            IsOpen = false;

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

        internal void SetInstance()
        {
            if (_isInitialized) return;
            CreateScreen();

            Screen = _screen.AddComponent<FCSAlterraHubGUI>();

            Screen.SetInstance(FCSAlterraHubGUISender.PDA);

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
            MaterialHelpers.ApplyEmissionShader(AlterraHub.BasePrimaryCol,gameObject,Color.white,0,0.01f,0.01f);
            MaterialHelpers.ApplySpecShader(AlterraHub.BasePrimaryCol,gameObject,1, 6.15f);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject,Color.cyan);    
            InGameMenuQuitPatcher.AddEventHandlerIfMissing(OnQuit);
            Screen.gameObject.SetActive(false);
            _isInitialized = true;
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

#if SUBNAUTICA
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundOpen);
            if (_pda.screen.activeSelf)
            {
                _pda.screen.SetActive(false);
            }
#else
#endif
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
            if (_timesOpen > 0 && !CardSystem.main.HasBeenRegistered() && !Mod.GamePlaySettings.IsPDAOpenFirstTime &&
                DroneDeliveryService.Main.DetermineIfFixed())
            {
                VoiceNotificationSystem.main.Play("PDA_Account_Instructions_key", 26);
            }

            if (Mod.GamePlaySettings.IsPDAOpenFirstTime && DroneDeliveryService.Main.DetermineIfFixed())
            {
                VoiceNotificationSystem.main.Play("PDA_Instructions_key", 26);
                Mod.GamePlaySettings.IsPDAOpenFirstTime = false;
                _timesOpen++;
                Mod.SaveGamePlaySettings();
            }
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
            QuickLogger.Debug("In Find PDA",true);
            if (PdaCanvas == null)
            {
#if SUBNAUTICA
                PdaCanvas = PDAObj?.GetComponent<PDA>()?.screen?.gameObject?.GetComponent<Canvas>();
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
            Mod.DeepCopySave(CardSystem.main.SaveDetails());
            QuickLogger.Debug("Quitting Purging CardSystem and AlterraHubSave", true);
            CardSystem.main.Purge();
            Mod.PurgeSave();
        }

        internal void Save(SaveData newSaveData)
        {
            Mod.GamePlaySettings.PDAShipmentInfo = Screen.GetShipmentInfo();
            Mod.GamePlaySettings.Rate = Screen.GetRate();
            Mod.GamePlaySettings.AutomaticDebitDeduction = Screen.GetAutomaticDebitDeduction();
        }

        
        internal void LoadFromSave()
        {
            Screen.LoadFromSave(Mod.GamePlaySettings.PDAShipmentInfo);
        }
        
        public static void ForceOpen()
        {
            Player_Patches.ForceOpenPDA = true;
        }

        public static void ForceClose()
        {
            Main.Close();
        }
    }
}