using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCSCommon.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace FCS_AlterraHub.ModItems.Buildables.OreCrusher.Mono;

internal class uGUI_OreCrusher : Page, IuGUIAdditionalPage
{
    private OreCrusherController _sender;
    [SerializeField] private uGUI_Icon[] slots;
    [SerializeField] private Text timerLbl;
    [SerializeField] private Text processingLbl;
    [SerializeField] private Text powerLbl;
    [SerializeField] private Text costLbl;
    [SerializeField] private Text speedLbl;
    [SerializeField] private Slider speedSlider;

    public override void Enter(object arg = null)
    {
        QuickLogger.Debug($"OreCrusher Enter Called {arg is IFCSObject}", true);
        QuickLogger.Debug($"OreCrusher Enter Called {arg?.GetType()}", true);

        OnLoadDisplay();

        base.Enter(arg);

        _sender = arg as OreCrusherController;

        if(_sender is not null )
        {
            _sender.OnProcessingCompleted += OnLoadDisplay;
            FCSPDAController.Main.ui.SetPDAAdditionalLabel(Language.main.Get(_sender.GetTechType()));
        }
        else
        {
            QuickLogger.Error("Ore Crusher Controller null on Enter");
        }
    }

    public override void Exit()
    {
        base.Exit();
        _sender.OnProcessingCompleted -= OnLoadDisplay;
    }

    public void OpenStorage()
    {
        _sender.OpenStorage();
    }

    public IFCSObject GetController() => _sender;

    public override void Awake()
    {
        base.Awake();

        speedSlider.onValueChanged.AddListener((value =>
        {
            var asInt = Mathf.RoundToInt(value);
            var speedMode = (OreConsumerSpeedModes)asInt;
            _sender?.ChangeSpeedMultiplier(speedMode);
            ChangeSpeedLabelText(speedMode.ToString());
        }));
    }

    private void UpdateScreen()
    {
        if (_sender is null) return;

        if (powerLbl != null)
        {
            powerLbl.text = $"{Language.main.Get("POWER")}: {_sender.GetPowerUsage()}";
        }

        if (processingLbl != null)
        {
            processingLbl.text = $"{LanguageService.ProcessingItem()}: {_sender.GetProcessingItemString()}";
        }

        if (costLbl != null)
        {
            costLbl.text = $"{LanguageService.OreValue()}: {_sender.GetOreValue()}";
        }

        if (timerLbl != null)
        {
            timerLbl.text = $"{LanguageService.TimeLeft()}: {_sender.GetTimeLeftString()}s";
        }
    }

    private void OnLoadDisplay()
    {
        if(_sender is null) return;

        ClearItems();
        var oreQueue = _sender.GetOreQueue();
        for (int i = 0; i < oreQueue.Count; i++)
        {
            var slot = slots[i];
            slot.sprite = SpriteManager.Get(oreQueue.ElementAt(i));
            slot.gameObject.SetActive(true);
        }
    }

    private void ClearItems()
    {
        foreach (uGUI_Icon currentItem in slots)
        {
            currentItem.gameObject.SetActive(false);
        }
    }

    private void ChangeSpeedLabelText(string value)
    {
        speedLbl.text = value;
    }

    private void Update()
    {
        UpdateScreen();
    }
}
