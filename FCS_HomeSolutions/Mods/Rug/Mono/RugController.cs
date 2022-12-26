using System;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.PaintTool.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Rug.Mono
{
    internal class RugController : FcsDevice,IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private CurtainDataEntry _savedData;
        private string _selectedImagePath;
        private Texture2D _selectedImage;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "R", Mod.ModPackID);
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

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
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
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.SelectedTexturePath = _selectedImagePath;
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
            if(!IsConstructed) return;

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
                    AlterraHub.HolsterPaintTool()
                };
                
                data.HandHoverPDAHelperEx(GetTechType());
            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override bool OverrideCustomUseText(out string message)
        {

            message = AuxPatchers.RugInteractionFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary));
            return true;
        }
    }
}
