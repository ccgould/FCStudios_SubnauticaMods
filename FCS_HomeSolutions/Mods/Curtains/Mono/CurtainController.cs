using System;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.PaintTool.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
#if SUBNAUTICA
 using Sprite = Atlas.Sprite;
#endif

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
        private Texture2D _selectedImage;
        //private CurtainDialog _dialog;
        //private InterfaceInteration _interactionChecker;

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

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);

                    if (_savedData.SelectedTexturePath != null && Main.Patterns.ContainsKey(_savedData.SelectedTexturePath))
                    {
                        LoadImage(Main.Patterns[_savedData.SelectedTexturePath]);
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
            var path = Main.Patterns.FirstOrDefault(x => x.Value == texture2D).Key;
            MaterialHelpers.SetTexture(ModelPrefab.CurtainDecalMaterial, gameObject, texture2D);
            _selectedImagePath = path;
            _selectedImage = texture2D;
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

            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);


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
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
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

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsConstructed) return;

            base.OnHandHover(hand);

            if (hand.IsTool() && hand.GetTool() is PaintToolController)
            {
                if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                {
                    ImageSelectorHUD.Main.Show(Main.PatternsIcon, _selectedImage, ((texture2D, sprite) =>
                    {
                        LoadImage(texture2D);
                    }));
                }
            }
            else
            {
                var data = new[]
                {
                    AlterraHub.HolsterPaintTool(),
                    "Click to open or close"
                };

                data.HandHoverPDAHelperEx(GetTechType());
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsConstructed) return;
            ToggleCurtainState();
        }

        private void ToggleCurtainState()
        {
            _animationHandler.SetBoolHash(_isOpen, !_animationHandler.GetBoolHash(_isOpen));
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override bool OverrideCustomUseText(out string message)
        {

            message = AuxPatchers.CurtainInteractionFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary));
            return true;
        }
    }

    internal class CurtainItem : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
    {
        private Texture2D _texture;
        private GameObject _ring;

        internal void Initialize(Texture2D texture,Sprite sprite)
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
