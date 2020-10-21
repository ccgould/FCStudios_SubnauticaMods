using System.Linq;
using FCS_AlterraHomeSolutions.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.Buildables;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHomeSolutions.Mono.PaintTool
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
        internal bool IsInitialized { get; set; }

        public override string animToolName => TechType.Scanner.AsString(true);

        private void Initialize()
        {
            if (IsInitialized) return;
            _collider = gameObject.GetComponent<BoxCollider>();
            _colorRing = GameObjectHelpers.FindGameObject(gameObject, "ColorFill").GetComponent<Image>();
            _amountLbl = GameObjectHelpers.FindGameObject(gameObject, "Amount").GetComponent<Text>();
            _liquid = GameObjectHelpers.FindGameObject(gameObject, "liquid");
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
                ChangeColor(_currentColor);
                _isFromSave = false;
            }
        }

        private void Update()
        {
            if(base.isDrawn && Input.GetKeyDown(KeyCode.RightArrow))
            {
                _currentIndex++;
                if (_currentIndex > ColorList.Colors.Count - 1)
                {
                    _currentIndex = ColorList.Colors.Count - 1;
                }

                ChangeColor(ColorList.Colors.ElementAt(_currentIndex).Vector4ToColor());
            }

            if (base.isDrawn && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _currentIndex--;
                if (_currentIndex < 0)
                {
                    _currentIndex = 0;
                }

                ChangeColor(ColorList.Colors.ElementAt(_currentIndex).Vector4ToColor());
            }
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
            RefreshUI();
        }

        private void Reload()
        {
            if (_paintCanFillAmount >= 100) return;
            _paintCanFillAmount = 100;
            RefreshUI();
        }

        private void Paint()
        {
            if (Physics.Raycast(Player.main.camRoot.transform.position, Player.main.camRoot.transform.forward, out var hit, _range))
            {
                var fcsDevice = hit.transform.parent.gameObject.transform.parent.gameObject.transform.parent.GetComponent<FcsDevice>();
                fcsDevice?.ChangeBodyColor(_currentColor);
                _paintCanFillAmount -= 1;
                RefreshUI();
            };
        }

        private void RefreshUI()
        {
            _colorRing.color = _currentColor;
            _colorRing.fillAmount = _paintCanFillAmount /100f;
            _amountLbl.text = _paintCanFillAmount.ToString();
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"In OnProtoSerialize: Painter Tool");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
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

        public void Save(SaveData newSaveData)
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
            

            //_savedData.BodyColor = ColorManager.GetMaskColor().ColorToVector4();
            newSaveData.PaintToolEntries.Add(_savedData);
        }
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPaintToolDataEntrySaveData(GetPrefabID());
        }

        public override bool OnAltDown()
        {
            if (Inventory.main.container.Contains(TechType.BloodOil))
            {
                var item = Inventory.main.container.RemoveItem(TechType.BloodOil);
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
            Initialize();
            _collider.isTrigger = false;
        }

        public override bool OnRightHandUp()
        {
            return true;
        }
        
        public override void OnDraw(Player p)
        {
            Initialize();
            _collider.isTrigger = true;
            base.OnDraw(p);
        }

        public override bool OnRightHandDown()
        {
            if (_paintCanFillAmount > 0)
            {
                Paint();
            }
            return true;
        }
    }
}