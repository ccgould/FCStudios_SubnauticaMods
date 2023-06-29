using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models;
using FCSCommon.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.Spawnables.PaintTool.Items;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using Nautilus.Json;
using static FCS_AlterraHub.Configuation.SaveData;
using FCS_AlterraHub.Models.Interfaces;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono;
internal class PaintToolController : PlayerTool, IFCSObject
{
    [SerializeField] private Image _primaryColorRing;
    [SerializeField] private Text _amountLbl;
    [SerializeField] private GameObject paintCanGameObject;
    [SerializeField] private Image _secondaryColorRing;
    [SerializeField] private Image _emissionColorRing;
    [SerializeField] private Text _mode;
    [SerializeField] private Text _currentIndexLBL;
    [SerializeField] private Text _colorNameLbl;
    [SerializeField] private Text _totalColorsLBL;

    public override string animToolName => TechType.Scanner.AsString(true);
    private List<ColorTemplate> _currentTemplates => ColorManager.colorTemplates;
    private ColorTargetMode _colorTargetMode = ColorTargetMode.Primary;
    private int _paintCanFillAmount;
    private string _prefabID;
    private TechType _validFuelTechType;
    private ColorTemplate _currentTemplate;
    private int _currentTemplateIndex;
    private const float Range = 10f;

    public override void Awake()
    {
        base.Awake();
        QuickLogger.Debug($"Paint Tool Awake {GetPrefabID()}", true);
        _validFuelTechType = PaintCanSpawnable.PatchedTechType;
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, gameObject, Color.cyan);
    }

    private void Start()
    {
        QuickLogger.Debug("Paint Tool Start", true);

        Plugin.AlterraHubSaveData.OnStartedSaving += OnStartedSaving;

        if (Plugin.AlterraHubSaveData.paintTools is null || Plugin.AlterraHubSaveData.paintTools.Count <= 0)
        {
            QuickLogger.Debug("No Save Data Was Loaded for Painter so I forced a load",true);
            Plugin.AlterraHubSaveData.Load();
        }

        if (Plugin.AlterraHubSaveData.paintTools.TryGetValue(GetPrefabID(), out PaintToolDataEntry data))
        {
            QuickLogger.Debug($"Loading Save for paint tool. Amount: {data.Amount} | Index: {data.CurrentTemplateIndex}", true);
            _paintCanFillAmount = data.Amount;
            _currentTemplateIndex = data.CurrentTemplateIndex;
            ChangeColor(ColorManager.colorTemplates[data.CurrentTemplateIndex]);
            RefreshUI();
        }
        else
        {
            ChangeColor(ColorManager.colorTemplates[0]);
            RefreshUI();
        }
    }

    private void OnStartedSaving(object sender, JsonFileEventArgs e)
    {
        AlterraHubSaveData data = e.Instance as AlterraHubSaveData;
        if (data.paintTools.ContainsKey(GetPrefabID()))
        {
            var save = data.paintTools[GetPrefabID()];
            save.Amount = _paintCanFillAmount;
            save.CurrentTemplateIndex = _currentTemplateIndex;
        }
        else
        {
            data.paintTools.Add(GetPrefabID(), new PaintToolDataEntry
            {
                CurrentTemplateIndex = _currentTemplateIndex,
                Amount = _paintCanFillAmount
            });
        }
    }

    public void OnTemplateChanged(ColorTemplate template, int index)
    {
        QuickLogger.Debug($"P Color: {template.PrimaryColor} | S Color: {template.SecondaryColor} | E Color: {template.EmissionColor}", true);
        _currentTemplateIndex = index;
        ChangeColor(template);
        RefreshUI();
    }   
        

    private new void OnDestroy()
    {
        QuickLogger.Debug($"Destroying Prefab: {GetPrefabID()}");
        Plugin.AlterraHubSaveData.OnStartedSaving -= OnStartedSaving;
    }

    private void OnEnable()
    {
        QuickLogger.Debug("Paint Tool Enable", true);
    }

    private void Update()
    {
        //TODO use the % operator to replace this watch unity videoplayer  tut in your youTube lib
        if (isDrawn && Input.GetKeyDown(Plugin.Configuration.PaintToolSelectColorForwardKeyCode))
        {
            _currentTemplateIndex += 1;

            if (_currentTemplateIndex > _currentTemplates.Count - 1)
            {
                _currentTemplateIndex = 0;
            }

            ChangeColor(_currentTemplates.ElementAt(_currentTemplateIndex));
        }
        else if (isDrawn && Input.GetKeyDown(Plugin.Configuration.PaintToolSelectColorBackKeyCode))
        {
            _currentTemplateIndex -= 1;

            if (_currentTemplateIndex < 0)
            {
                _currentTemplateIndex = _currentTemplates.Count - 1;
            }

            ChangeColor(_currentTemplates.ElementAt(_currentTemplateIndex));
        }
        else if (isDrawn && Input.GetKeyDown(Plugin.Configuration.PaintToolColorSampleKeyCode))
        {
            var device = GetFCSDeviceFromTarget();
            if (device != null)
            {
                var index = GetCurrentSelectedTemplateIndex();
                _currentTemplates[index] = device.GetBodyColor();
                ChangeColor(device.GetBodyColor());
            }
        }
        RefreshUI();
    }

    private void ChangeColor(ColorTemplate template)
    {
        if (template == null) return;
        _currentTemplate = template;
        MaterialHelpers.ChangeMaterialColor(ModPrefabService.BasePrimaryCol, paintCanGameObject, Color.white ,template.PrimaryColor);
    }

    private void Reload()
    {
        if (_paintCanFillAmount >= 100) return;
        _paintCanFillAmount = 100;
        RefreshUI();
    }

    private void Paint()
    {
        var fcsDevice = GetFCSDeviceFromTarget();
        if (fcsDevice != null)
        {
            var result = fcsDevice.ChangeBodyColor(_currentTemplate);

            if (result)
            {
                if (GameModeUtils.RequiresPower() && _colorTargetMode != ColorTargetMode.Emission)
                {
                    _paintCanFillAmount -= 1;
                }
                RefreshUI();
            }
        }
    }

    private FCSDevice GetFCSDeviceFromTarget()
    {
        Vector3 vector = default;
        GameObject go = null;
        FCSDevice fcsDevice = null;
        UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, Range, ref go, ref vector, false);

        if (go != null)
        {
            //var objRoot = UWE.Utils.GetEntityRoot(go);
            //QuickLogger.Debug($"Painter Hit: {objRoot.name} || Layer: {go.layer}", true);
            fcsDevice = go.GetComponentInParent<FCSDevice>();
        }
        return fcsDevice;
    }

    private void RefreshUI()
    {

        _currentIndexLBL.text = (_currentTemplateIndex + 1).ToString();
        _totalColorsLBL.text = _currentTemplates.Count.ToString();
        _mode.text = ((int)_colorTargetMode).ToString();
        _primaryColorRing.color = _currentTemplate.PrimaryColor;
        _secondaryColorRing.color = _currentTemplate.SecondaryColor;
        _emissionColorRing.color = _currentTemplate.EmissionColor;
        _primaryColorRing.fillAmount = _paintCanFillAmount / 100f;
        _amountLbl.text = _paintCanFillAmount.ToString();
    }

    public string GetPrefabID()
    {
        if (string.IsNullOrEmpty(_prefabID))
        {
            _prefabID = gameObject.GetComponent<PrefabIdentifier>()?.Id;
        }

        return _prefabID;
    }

    public override bool OnAltDown()
    {
        //if (_ignoreDefaultControls) return false;
        FCSPDAController.Main.OpenDeviceUI(PaintToolSpawnable.PatchedTechType, this);
        //uGUI_PaintToolColorPicker.Main.Open(this, (template, index) =>
        //{
        //    QuickLogger.Debug($"P Color: {template.PrimaryColor} | S Color: {template.SecondaryColor} | E Color: {template.EmissionColor}", true);
        //    _currentTemplate = template;
        //    _currentTemplateIndex = index;
        //});

        return true;
    }

    public override bool OnReloadDown()
    {
        if (Inventory.main.container.Contains(_validFuelTechType))
        {
            var item = Inventory.main.container.RemoveItem(_validFuelTechType);
            Destroy(item.gameObject);
            Reload();
        }
        return true;
    }

    public override bool OnAltUp()
    {
        return true;
    }

    public override bool OnRightHandDown()
    {
        if (_paintCanFillAmount > 0 || !GameModeUtils.RequiresPower())
        {
            Paint();
        }

        return true;
    }

    public override string GetCustomUseText()
    {
        if (GetFCSDeviceFromTarget() != null && GetFCSDeviceFromTarget().OverrideCustomUseText(out string message))
        {
            return CreateCustomUseMessage(message);
        }
        return CreateCustomUseMessage();
    }

    private string CreateCustomUseMessage(string message = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = $"Change Template: {GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary)} | Color Sample {Plugin.Configuration.PaintToolColorSampleKeyCode}";
        }
        return $"Press Change Colors ({Plugin.Configuration.PaintToolSelectColorBackKeyCode})/({Plugin.Configuration.PaintToolSelectColorForwardKeyCode}) | Use Paint Can: {GameInput.GetBindingName(GameInput.Button.Reload, GameInput.BindingSet.Primary)} | {message}";
    }

    public List<ColorTemplate> GetTemplates()
    {
        return _currentTemplates;
    }

    public int GetCurrentSelectedTemplateIndex()
    {
        return _currentTemplateIndex;
    }
    public void UpdateTemplates(int templateIndex, ColorTemplate colorTemplate)
    {
        if (_currentTemplates.Count < templateIndex + 1)
        {
            var amountToCreate = templateIndex + 1 - _currentTemplates.Count;
            for (int i = 0; i < amountToCreate; i++)
            {
                _currentTemplates.Add(new ColorTemplate());
            }
        }
        _currentTemplates[templateIndex] = colorTemplate;
        RefreshUI();
    }

    public TechType GetTechType()
    {
        return gameObject.GetComponent<TechTag>()?.type ??
               gameObject.GetComponentInChildren<TechTag>()?.type ??
               TechType.None;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
