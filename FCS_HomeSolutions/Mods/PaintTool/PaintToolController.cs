using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.PaintTool.Mono;
using FCS_HomeSolutions.Mods.PaintTool.Spawnable;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.PaintTool
{
    internal class PaintToolController: PlayerTool,IProtoEventListener, IFCSSave<SaveData>
    {
        private BoxCollider _collider;
        private const float Range = 10f;
        private Image _primaryColorRing;
        private Text _amountLbl;
        private int _paintCanFillAmount;
        private bool _isFromSave;
        private PaintToolDataEntry _savedData;
        private string _prefabID;
        private Text _mode;
        private Text _currentIndexLBL;
        private Text _colorNameLbl;
        private ColorTargetMode _colorTargetMode = ColorTargetMode.Primary;
        private Text _totalColorsLBL;
        private TechType _validFuelTechType;
        private ColorTemplate _currentTemplate;
        private List<ColorTemplate> _currentTemplates = new();
        private int _currentTemplateIndex;
        private Image _secondaryColorRing;
        private Image _emissionColorRing;
        internal bool IsInitialized { get; set; }

        public override string animToolName => TechType.Scanner.AsString(true);

        private void Initialize()
        {
            if (IsInitialized) return;

            _currentTemplates = new List<ColorTemplate>
            {
                new ColorTemplate
                {
                    PrimaryColor = Color.white,
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.cyan
                },
                new ColorTemplate
                {
                    PrimaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.red
                },
                new ColorTemplate
                {
                    PrimaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.magenta
                },
                new ColorTemplate
                {
                    PrimaryColor = Color.white,
                    SecondaryColor = new Color(0.8941177f,0.6784314f,0.01960784f,1f),
                    EmissionColor = Color.cyan
                },
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
            };

            _validFuelTechType = PaintCanSpawnable.PaintCanClassID.ToTechType();

            _collider = gameObject.GetComponent<BoxCollider>();
            _primaryColorRing = GameObjectHelpers.FindGameObject(gameObject, "PrimaryColorFill").GetComponent<Image>();
            _secondaryColorRing = GameObjectHelpers.FindGameObject(gameObject, "SecondaryColorFill").GetComponent<Image>();
            _emissionColorRing = GameObjectHelpers.FindGameObject(gameObject, "EmissionColorFill").GetComponent<Image>();
            _amountLbl = GameObjectHelpers.FindGameObject(gameObject, "Amount").GetComponent<Text>();
            
            _colorNameLbl = GameObjectHelpers.FindGameObject(gameObject, "ColorName").GetComponent<Text>();
            _currentIndexLBL = GameObjectHelpers.FindGameObject(gameObject, "CurrentIndex").GetComponent<Text>();
            _totalColorsLBL = GameObjectHelpers.FindGameObject(gameObject, "TotalColors").GetComponent<Text>();
            _mode = GameObjectHelpers.FindGameObject(gameObject, "Mode").GetComponent<Text>();

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

            Mod.RegisterPaintTool(this);
            ChangeColor(new ColorTemplate());
            RefreshUI();
            IsInitialized = true;
        }

        private new void OnDestroy()
        {
            Mod.UnRegisterPaintTool(this);
        }

        private void OnEnable()
        {
            GetPrefabID();
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

                _paintCanFillAmount = _savedData.Amount;
                _currentTemplate = _savedData.ColorTemplate.ToColorTemplate();
                if(_savedData.ColorTemplates != null)
                    _currentTemplates = _savedData.ColorTemplates.ToListOfColorTemplates();
                _currentTemplateIndex = _savedData.CurrentTemplateIndex;

                ChangeColor(_currentTemplates.ElementAt(_currentTemplateIndex));

                RefreshUI();
                _isFromSave = false;
            }
        }

        private void Update()
        {
            if (uGUI_PaintToolColorPicker.Main != null && !uGUI_PaintToolColorPicker.Main.IsOpen())
            {
                //TODO use the % operator to replace this watch unity videoplayer  tut in your youTube lib
                if (base.isDrawn && Input.GetKeyDown(QPatch.Configuration.PaintToolSelectColorForwardKeyCode))
                {
                    _currentTemplateIndex += 1;

                    if (_currentTemplateIndex > _currentTemplates.Count - 1)
                    {
                        _currentTemplateIndex = 0;
                    }

                    ChangeColor(_currentTemplates.ElementAt(_currentTemplateIndex));
                }
                else if (base.isDrawn && Input.GetKeyDown(QPatch.Configuration.PaintToolSelectColorBackKeyCode))
                {
                    _currentTemplateIndex -= 1;

                    if (_currentTemplateIndex < 0)
                    {
                        _currentTemplateIndex = _currentTemplates.Count - 1;
                    }

                    ChangeColor(_currentTemplates.ElementAt(_currentTemplateIndex));
                }
                RefreshUI();
            }
        }
        
        private void ChangeColor(ColorTemplate template)
        {
            if (template == null) return;
            _currentTemplate = template;
            MaterialHelpers.ChangeMaterialColor(AlterraHub.BasePrimaryCol, gameObject, template.PrimaryColor);
        }

        private void Reload()
        {
            if (_paintCanFillAmount >= 100) return;
            _paintCanFillAmount = 100;
            RefreshUI();
        }

        private void Paint()
        {
            Vector3 vector = default;
            GameObject go = null;
            UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, Range, ref go, ref vector, false);
            QuickLogger.Debug($"Painter Hit: {go?.name} || Layer: {go?.layer}",true);
            var fcsDevice = go?.GetComponentInParent<FcsDevice>();
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

        private void RefreshUI()
        {
            if (!IsInitialized) return;
            _currentIndexLBL.text = (_currentTemplateIndex + 1).ToString();
            _totalColorsLBL.text = _currentTemplates.Count.ToString();
            _mode.text = ((int) _colorTargetMode).ToString();
            _primaryColorRing.color = _currentTemplate.PrimaryColor;
            _secondaryColorRing.color = _currentTemplate.SecondaryColor;
            _emissionColorRing.color = _currentTemplate.EmissionColor;
            _primaryColorRing.fillAmount = _paintCanFillAmount /100f;
            _amountLbl.text = _paintCanFillAmount.ToString();
        }
        
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"In OnProtoSerialize: Painter Tool");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public string GetPrefabID()
        {
            if (string.IsNullOrEmpty(_prefabID))
            {
                _prefabID = gameObject.GetComponent<PrefabIdentifier>()?.Id ??
                            gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
            }

            return _prefabID;
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer=null)
        {
            QuickLogger.Debug($"Paint Tool Save Entry{GetPrefabID()}", true);

            if (!IsInitialized) return;

            if (_savedData == null)
            {
                _savedData = new PaintToolDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _currentTemplate?.ToColorTemplate() ?? new ColorTemplateSave();
            _savedData.ColorTemplates = _currentTemplates.ToListOfColorTemplatesSaves();
            _savedData.Amount = _paintCanFillAmount;
            _savedData.CurrentTemplateIndex = _currentTemplateIndex;

            newSaveData.PaintToolEntries.Add(_savedData);
        }
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPaintToolDataEntrySaveData(GetPrefabID());
        }

        public override bool OnAltDown()
        {

            uGUI_PaintToolColorPicker.Main.Open(this,(template,index) =>
            {
                QuickLogger.Debug($"P Color: {template.PrimaryColor} | S Color: {template.SecondaryColor} | E Color: {template.EmissionColor}",true);
                _currentTemplate = template;
                _currentTemplateIndex = index;
            });

            return true;
        }

        private string GetTargetModeString(ColorTargetMode mode)
        {
            switch (mode)
            {
                case ColorTargetMode.Primary:
                    return "Primary Color PaintMode";
                case ColorTargetMode.Secondary:
                    return "Secondary Color Paint Mode";
                case ColorTargetMode.Both:
                    return "Both Colors Paint Mode";
                case ColorTargetMode.Emission:
                    return "Light Color Paint Mode";
            }

            return $"[GetTargetModeString]: {AlterraHub.ErrorHasOccured()}";
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

        public override void OnHolster()
        {
            _collider.isTrigger = false;
        }

        public override bool OnRightHandUp()
        {
            return true;
        }
        
        public override void OnDraw(Player p)
        {
            _collider.isTrigger = true;
            base.OnDraw(p);
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
            return $"Press Change Colors ({QPatch.Configuration.PaintToolSelectColorBackKeyCode})/({QPatch.Configuration.PaintToolSelectColorForwardKeyCode}) | Use Paint Can: {GameInput.GetBindingName(GameInput.Button.Reload, GameInput.BindingSet.Primary)} | Change Template: {GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary)}";
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
                var amountToCreate = templateIndex + 1 -  _currentTemplates.Count;
                for (int i = 0; i < amountToCreate; i++)
                {
                    _currentTemplates.Add(new ColorTemplate());
                }
            }
            _currentTemplates[templateIndex] = colorTemplate;
            RefreshUI();
        }
    }
}