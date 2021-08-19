using System;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_HomeSolutions.Mods.Curtains.Mono
{
    internal class CurtainController : FcsDevice,IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private CurtainDataEntry _savedData;
        private AnimationManager _animationHandler;
        private int _isOpen;
        private string _selectedImagePath;
        private CurtainDialog _dialog;
        private InterfaceInteration _interactionChecker;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "C", Mod.ModPackID);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
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

                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                    if (_savedData.SelectedTexturePath != null && QPatch.Patterns.ContainsKey(_savedData.SelectedTexturePath))
                    {
                        LoadImage(QPatch.Patterns[_savedData.SelectedTexturePath]);
                    }

                    if (_savedData.IsOpen)
                    {
                        ToggleCurtainState();
                    }

                }

                _runStartUpOnEnable = false;
            }
        }

        internal void LoadImage(Texture2D texture2D)
        {
            var path = QPatch.Patterns.FirstOrDefault(x => x.Value == texture2D).Key;
            MaterialHelpers.SetTexture(ModelPrefab.CurtainDecalMaterial, gameObject, texture2D);
            _selectedImagePath = path;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetCurtainDataEntrySaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_animationHandler == null)
            {
                _isOpen = Animator.StringToHash("IsOpen");
                _animationHandler = gameObject.AddComponent<AnimationManager>();
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            if (_dialog == null)
            {
                var dialog = GameObjectHelpers.FindGameObject(gameObject, "HUD");
                _interactionChecker = dialog.AddComponent<InterfaceInteration>();
                _dialog = dialog.AddComponent<CurtainDialog>();
                _dialog.Initialize(this);
            }
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.cyan);


            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new CurtainDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.SelectedTexturePath = _selectedImagePath;
            _savedData.IsOpen = _animationHandler.GetBoolHash(_isOpen);
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.CurtainEntries.Add(_savedData);
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;
            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            if(!IsConstructed ||_interactionChecker.IsInRange) return;
            HandReticle main = HandReticle.main;
            main.SetInteractText("Click to open",AuxPatchers.CurtainInteractionFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary)));
            main.SetIcon(HandReticle.IconType.Hand);

            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
            {

                if (_dialog.IsOpen())
                {
                    _dialog.Hide();
                }
                else
                {
                    _dialog.Show();
                }
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsConstructed || _interactionChecker.IsInRange) return;
            ToggleCurtainState();
        }

        private void ToggleCurtainState()
        {
            _animationHandler.SetBoolHash(_isOpen, !_animationHandler.GetBoolHash(_isOpen));
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }

    internal class CurtainItem : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
    {
        private Texture2D _texture;
        private GameObject _ring;

        internal void Initialize(Texture2D texture,Atlas.Sprite sprite)
        {
            var icon = gameObject.AddComponent<uGUI_Icon>();
            icon.sprite = sprite;
            _texture = texture;
            _ring = gameObject.FindChild("Ring");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _ring.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _ring.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(_texture);
        }

        public Action<Texture2D> OnClick { get; set; }
    }

    internal class CurtainDialog : MonoBehaviour
    {
        private GridHelper _gridHelper;
        private CurtainController _mono;

        internal void Initialize(CurtainController mono)
        {
            _mono = mono;
            _gridHelper = gameObject.AddComponent<GridHelper>();
            if(FindAllComponents())
            {
                _gridHelper.DrawPage();
            }
        }

        private bool FindAllComponents()
        {
            try
            {
                var close = InterfaceHelpers.FindGameObject(gameObject, "CloseBTN");
                InterfaceHelpers.CreateButton(close, "CloseBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, Color.cyan, 5);

                _gridHelper.Setup(21,ModelPrefab.TemplateItem,gameObject,Color.gray, Color.white, OnButtonClick);
                _gridHelper.OnLoadDisplay += OnLoadDisplay;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private void OnLoadDisplay(DisplayData data)
        {
            _gridHelper.ClearPage();

            var grouped = QPatch.PatternsIcon;

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            if (data.ItemsGrid?.transform == null)
            {
                QuickLogger.Debug("Grid returned null canceling operation");
                return;
            }

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                GameObject buttonPrefab = GameObject.Instantiate(data.ItemsPrefab);

                if (buttonPrefab == null || data.ItemsGrid == null)
                {
                    if (buttonPrefab != null)
                    {
                        QuickLogger.Debug("Destroying Tab", true);
                        Destroy(buttonPrefab);
                    }
                    return;
                }
                buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                var item = buttonPrefab.gameObject.AddComponent<CurtainItem>();
                item.Initialize(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                item.OnClick += texture2D =>
                {
                    _mono.LoadImage(texture2D);
                };
            }

            _gridHelper.UpdaterPaginator(grouped.Count);
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "CloseBTN":
                    Hide();
                    break;
            }
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }
    }

    internal class InterfaceInteration : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsInRange { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsInRange = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsInRange = false;
        }

        private void OnDisable()
        {
            IsInRange = false;
        }
    }



}
