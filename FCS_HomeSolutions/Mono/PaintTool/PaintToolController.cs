using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mono.PaintTool
{
    internal class PaintToolController: PlayerTool,IProtoEventListener, IFCSSave<SaveData>
    {
        private BoxCollider _collider;
        private float _range = 10f;
        private Image _colorRing;
        private Text _amountLbl;
        private Color _currentColor;
        private int _paintCanFillAmount;
        private int _currentIndex;
        private GameObject _liquid;
        private bool _isFromSave;
        private PaintToolDataEntry _savedData;
        private string _prefabID;
        private Text _mode;
        private Text _currentIndexLBL;
        private Text _colorNameLbl;
        private Image _selectedColor;
        private ColorTargetMode _colorTargetMode = ColorTargetMode.Primary;
        private Text _totalColorsLBL;
        private int _numberColorTargetModeTypes;
        private int _painterModeIndex = 1;
        private int _layerMask;
        private TechType _validFuelTechType;
        internal bool IsInitialized { get; set; }

        public override string animToolName => TechType.Scanner.AsString(true);

        private void Initialize()
        {
            if (IsInitialized) return;
            _validFuelTechType = Mod.PaintCanClassID.ToTechType();
            _numberColorTargetModeTypes = System.Enum.GetValues(typeof(ColorTargetMode)).Length;
            _collider = gameObject.GetComponent<BoxCollider>();
            _colorRing = GameObjectHelpers.FindGameObject(gameObject, "ColorFill").GetComponent<Image>();
            _amountLbl = GameObjectHelpers.FindGameObject(gameObject, "Amount").GetComponent<Text>();
            _selectedColor = GameObjectHelpers.FindGameObject(gameObject, "SelectedColor").GetComponent<Image>();
            _colorNameLbl = GameObjectHelpers.FindGameObject(gameObject, "ColorName").GetComponent<Text>();
            _currentIndexLBL = GameObjectHelpers.FindGameObject(gameObject, "CurrentIndex").GetComponent<Text>();
            _totalColorsLBL = GameObjectHelpers.FindGameObject(gameObject, "TotalColors").GetComponent<Text>();
            _mode = GameObjectHelpers.FindGameObject(gameObject, "Mode").GetComponent<Text>();
            _liquid = GameObjectHelpers.FindGameObject(gameObject, "liquid");
            _layerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Trigger"));
            Mod.RegisterPaintTool(this);
            ChangeColor(Color.black);
            RefreshUI();
            IsInitialized = true;
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
                _currentColor = _savedData.Color.Vector4ToColor();
                _colorTargetMode = _savedData.ColorTargetMode == 0 ? ColorTargetMode.Primary : _savedData.ColorTargetMode;
                _painterModeIndex = (int) _colorTargetMode;

                ChangeColor(_currentColor);
                _currentIndex = ColorList.GetColorIndex(_currentColor);
                RefreshUI();
                _isFromSave = false;
            }
        }

        private void Update()
        {
            if (base.isDrawn && Input.GetKeyDown(KeyCode.UpArrow))
            {
                _painterModeIndex += 1;
                
                if (_painterModeIndex <= _numberColorTargetModeTypes)
                {
                    _colorTargetMode = (ColorTargetMode)_painterModeIndex;
                }
                else
                {
                    _painterModeIndex = 1;
                    _colorTargetMode = (ColorTargetMode)_painterModeIndex;
                }
            }
            else if (base.isDrawn && Input.GetKeyDown(KeyCode.DownArrow))
            {
                _painterModeIndex -= 1;

                if (_painterModeIndex >= 1)
                {
                    _colorTargetMode = (ColorTargetMode)_painterModeIndex;
                }
                else
                {
                    _painterModeIndex = _numberColorTargetModeTypes;
                    _colorTargetMode = (ColorTargetMode)_painterModeIndex;
                }
            }
            else if (base.isDrawn && Input.GetKeyDown(KeyCode.RightArrow))
            {
                _currentIndex += 1;

                if (_currentIndex > ColorList.GetCount() - 1)
                {
                    _currentIndex = 0;
                }

                ChangeColor(ColorList.GetColor(_currentIndex));
            }
            else if (base.isDrawn && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _currentIndex -= 1;

                if (_currentIndex < 0)
                {
                    _currentIndex = ColorList.GetCount() - 1;
                }

                ChangeColor(ColorList.GetColor(_currentIndex));
            }
            RefreshUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Mod.UnRegisterPaintTool(this);
        }

        private void ChangeColor(Color color)
        {
            _currentColor = color;
            MaterialHelpers.ChangeMaterialColor(ModelPrefab.BodyMaterial, _liquid, color);
        }

        private void Reload()
        {
            if (_paintCanFillAmount >= 100) return;
            _paintCanFillAmount = 100;
            RefreshUI();
        }

        private void Paint()
        {

            if (Physics.Raycast(Player.main.camRoot.transform.position, MainCamera.camera.transform.forward, out var hit, _range,_layerMask, QueryTriggerInteraction.Ignore))
            {
                QuickLogger.Debug($"Painter Hit: {hit.transform.gameObject.name}",true);
                //var fcsDevice = hit.transform?.parent?.gameObject?.transform?.parent?.gameObject?.transform?.parent?.GetComponent<FcsDevice>();
                var fcsDevice = hit.transform.GetComponentInParent<FcsDevice>();
                if (fcsDevice != null)
                {
                    var result = fcsDevice.ChangeBodyColor(_currentColor, _colorTargetMode);

                    if (result)
                    {
                        if (GameModeUtils.RequiresPower())
                        {
                            _paintCanFillAmount -= 1;
                        }
                        RefreshUI();
                    }
                }
            };
        }

        private void RefreshUI()
        {
            if (!IsInitialized) return;
            _currentIndexLBL.text = (_currentIndex + 1).ToString();
            _totalColorsLBL.text = ColorList.GetCount().ToString();

            var currentColorVec4 = _currentColor.ColorToVector4();
            if (ColorList.Contains(currentColorVec4))
            {
                _colorNameLbl.text = ColorList.GetName(currentColorVec4);
            }
            else
            {
                ResetColor();
            }

            _selectedColor.color = _currentColor;
            _mode.text = ((int) _colorTargetMode).ToString();
            _colorRing.color = _currentColor;
            _colorRing.fillAmount = _paintCanFillAmount /100f;
            _amountLbl.text = _paintCanFillAmount.ToString();
        }

        private void ResetColor()
        {
            ChangeColor(Color.black);
            _currentIndex = 0;
            _currentColor = Color.black;
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
            _savedData.Color = _currentColor.ColorToVector4();
            _savedData.Amount = _paintCanFillAmount;
            _savedData.ColorTargetMode = _colorTargetMode;
            newSaveData.PaintToolEntries.Add(_savedData);
        }
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPaintToolDataEntrySaveData(GetPrefabID());
        }

        public override bool OnAltDown()
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
    }
}