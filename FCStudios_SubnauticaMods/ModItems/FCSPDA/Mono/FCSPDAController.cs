using FCS_AlterraHub;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Patches;
using FCSCommon.Utilities;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;

public class FCSPDAController : MonoBehaviour
{
    public bool isInUse { get; private set; }

    public bool isFocused
    {
        get
        {
            return this.ui != null && this.ui.focused;
        }
    }

    public bool isOpen
    {
        get
        {
            return this.state == PDA.State.Opened;
        }
    }

    public PDA.State state
    {
        get
        {
            if (this.sequence.target)
            {
                if (!this.sequence.active)
                {
                    return PDA.State.Opened;
                }
                return PDA.State.Opening;
            }
            else
            {
                if (!base.gameObject.activeInHierarchy || !this.sequence.active)
                {
                    return PDA.State.Closed;
                }
                return PDA.State.Closing;
            }
        }
    }

    public FCSAlterraHubGUI ui
    {
        get
        {
            if (this._ui == null)
            {   
                GameObject gameObject = Instantiate<GameObject>(this.prefabScreen);
                this._ui = gameObject.GetComponent<FCSAlterraHubGUI>();
                gameObject.GetComponent<uGUI_CanvasScaler>().SetAnchor(this.screenAnchor);
                this._ui.Initialize();
            }
            return this._ui;
        }
    }

    public static float time { get; private set; }

    public static float deltaTime { get; private set; }

    public static float GetDeltaTime()
    {
        return deltaTime;
    }

    public void SetIgnorePDAInput(bool ignore)
    {
        this.ignorePDAInput = ignore;
    }

    public static void PerformUpdate()
    {
        Player main = Player.main;
        FCSPDAController pda = (main is not null) ? Player_Patches.FCSPDA : null;
        if ((main is not null && pda is not null) && pda.isActiveAndEnabled)
        {
            pda.ManagedUpdate();
            return;
        }
        UpdateTime(false);
    }

    private static void UpdateTime(bool pausedByPDA)
    {
        deltaTime = (pausedByPDA ? Time.unscaledDeltaTime : DayNightCycle.main.deltaTime);
        time += deltaTime;
        Shader.SetGlobalFloat(ShaderPropertyID._PDATime, time);
    }

    public static void Deinitialize()
    {
        time = 0f;
    }

    private void ManagedUpdate()
    {
        bool flag = MiscSettings.pdaPause && (this.state == PDA.State.Opened || this.state == PDA.State.Opening || this.state == PDA.State.Closing);
        FreezeTime.Set(FreezeTime.Id.PDA, flag ? this.sequence.t : 0f);
        bool flag2 = FreezeTime.GetTopmostId() == FreezeTime.Id.PDA;
        bool flag3 = flag && flag2;
        UpdateTime(flag3);
        Player.main.playerAnimator.updateMode = (flag3 ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal);
        this.sequence.Update(deltaTime);
        Player main = Player.main;
        if (this.isInUse && this.isFocused && (GameInput.GetButtonDown(GameInput.Button.PDA) || Input.GetKeyDown(Plugin.Configuration.FCSPDAKeyCode)))
        {

            this.Close();
            return;
        }
        if (this.targetWasSet && (this.target == null || (this.target.transform.position - main.transform.position).sqrMagnitude >= this.activeSqrDistance))
        {
            EncyclopediaService.ClearCurrentDevice();
            this.Close();
        }
    }

    public bool Open(PDATab tab = PDATab.None, Transform target = null, OnClose onCloseCallback = null)
    {
        if (this.isInUse || this.ignorePDAInput)
        {
            return false;
        }

        PatchAdditionalPages();

        uGUI.main.quickSlots.SetTarget(null);
        this.prevQuickSlot = Inventory.main.quickSlots.activeSlot;
        bool flag = Inventory.main.ReturnHeld(true);
        Player main = Player.main;
        if (!flag || main.cinematicModeActive)
        {
            return false;
        }
        MainCameraControl.main.SaveLockedVRViewModelAngle();
        Inventory.main.quickSlots.SetSuspendSlotActivation(true);
        this.isInUse = true;
        Player.main.GetPDA().isInUse = true;
        main.armsController.SetUsingPda(true);
        base.gameObject.SetActive(true);
        this.ui.OnOpenPDA(tab);
        this.sequence.Set(timeDraw, true, new SequenceCallback(this.Activated));
        //TODO Create Goal  = GoalManager.main.OnCustomGoalEvent("Open_PDA");
        if (HandReticle.main != null)
        {
            HandReticle.main.RequestCrosshairHide();
        }
        Inventory.main.SetViewModelVis(false);
        this.targetWasSet = (target != null);
        this.target = target;
        this.onCloseCallback = onCloseCallback;
        if (this.targetWasSet)
        {
            this.activeSqrDistance = (target.transform.position - main.transform.position).sqrMagnitude + 1f;
        }
        if (this.audioSnapshotInstance.isValid())
        {
            this.audioSnapshotInstance.start();
        }
        UwePostProcessingManager.OpenPDA();

        ui.TryRemove404Screen();

        ui.RefreshTeleportationPage();

        return true;
    }

    public void Close()
    {
        if (!this.isInUse || this.ignorePDAInput)
        {
            return;
        }
        Player main = Player.main;
        QuickSlots quickSlots = Inventory.main.quickSlots;
        quickSlots.EndAssign(false);
        MainCameraControl.main.ResetLockedVRViewModelAngle();
        Vehicle vehicle = main.GetVehicle();
        if (vehicle != null)
        {
            uGUI.main.quickSlots.SetTarget(vehicle);
        }
        this.targetWasSet = false;
        this.target = null;
        main.armsController.SetUsingPda(false);
        quickSlots.SetSuspendSlotActivation(false);
        this.ui.OnClosePDA();
        if (HandReticle.main != null)
        {
            HandReticle.main.UnrequestCrosshairHide();
        }
        Inventory.main.SetViewModelVis(true);
        this.sequence.Set(timeHolster, false, new SequenceCallback(this.Deactivated));
        if (this.audioSnapshotInstance.isValid())
        {
            this.audioSnapshotInstance.stop(STOP_MODE.ALLOWFADEOUT);
            this.audioSnapshotInstance.release();
        }
        UwePostProcessingManager.ClosePDA();
        if (this.onCloseCallback != null)
        {
            OnClose onClose = this.onCloseCallback;
            this.onCloseCallback = null;
            onClose(this);
        }

        ResetToHome();
    }

    private void ResetToHome()
    {
        if (ui.CurrentPage() == PDAPages.DevicePage || ui.CurrentPage() == PDAPages.DeviceSettings)
            ui.PurgePages();
    }

    public void Activated()
    {
        UWE.Utils.lockCursor = false;
        this.ui.Select(false);
        this.ui.OnPDAOpened();
    }

    public void Deactivated()
    {
        if (!this.ignorePDAInput)
        {
            Inventory.main.quickSlots.Select(this.prevQuickSlot);
        }
        this.ui.OnPDAClosed();
        base.gameObject.SetActive(false);
        this.isInUse = false;
        Player.main.GetPDA().isInUse = false; 
    }

    private const float timeDraw = 0.5f;

    private const float timeHolster = 0.3f;

    public const string pauseId = "PDA";

    [AssertNotNull]
    public GameObject prefabScreen;

    [AssertNotNull]
    public Transform screenAnchor;

    private Sequence sequence = new Sequence(false);


    private int prevQuickSlot = -1;

    private bool targetWasSet;

    private Transform target;

    private OnClose onCloseCallback;

    private float activeSqrDistance;

    private bool ignorePDAInput;

    private FCSAlterraHubGUI _ui;

    private EventInstance audioSnapshotInstance;

    public delegate void OnClose(FCSPDAController pda);

    //=======================================================================//
    public static FCSPDAController Main;
    static Dictionary<TechType, GameObject> additionPages = new();
    private bool _isInitialized;
    private bool _isBeingDestroyed;

    private void Awake()
    {
        Main = this;
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, gameObject, Color.cyan);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, gameObject, 1f);
        EncyclopediaService.OnOpenEncyclopedia += OnOpenEncyclopedia;   
    }

    public FCSAlterraHubGUI GetGUI()
    {
        return ui;
    }

    public static void ForceOpen()
    {
        Player_Patches.ForceOpenPDA = true;
    }

    public static void ForceClose()
    {
        Main.Close();
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
        GamePlayService.Main.SetShipmentInfo(ui.GetShipmentInfo());
    }

    internal void LoadFromSave()    
    {
        ui.LoadFromSave(GamePlayService.Main.GetShipmentInfo());
    }

    /// <summary>
    /// Displays the UI Page in the PDA
    /// </summary>
    /// <param name="id">Id of the UI to display</param>
    /// <param name="fcsDevice">The device to change the settings on.</param>
    public void OpenDeviceUI(TechType id, MonoBehaviour fcsDevice,OnClose onClose = null)
    {
        Open(PDATab.None,fcsDevice.transform,onClose);
        ui.PrepareDevicePage(id, fcsDevice);
    }

    public static void AddAdditionalPage<T>(TechType id, GameObject ui) where T : Component
    {
        ui.EnsureComponent<T>();
        additionPages.Add(id, ui);
    }

    private void PatchAdditionalPages()
    {
        for (int a = additionPages.Count - 1; a >= 0; a--)
        {
            var page = additionPages.ElementAt(a);

            ui.AddAdditionalPage(page.Key, page.Value);
            additionPages.Remove(page.Key);
        }
    }

    internal void SetInstance()
    {
        if (_isInitialized) return;
        InGameMenuQuitPatcher.AddEventHandlerIfMissing(OnQuit);
        _isInitialized = true;
    }

    private void OnOpenEncyclopedia(TechType techType)
    {
        ForceOpen();
        ui.OpenEncyclopedia(techType);
    }

    private void OnOpenEncyclopedia(FCSDevice device)
    {
        ForceOpen();
        ui.OpenEncyclopedia(device);
    }

    private void OnDestroy()
    {
        EncyclopediaService.OnOpenEncyclopedia -= OnOpenEncyclopedia;
        _isBeingDestroyed = true;
    }
}
