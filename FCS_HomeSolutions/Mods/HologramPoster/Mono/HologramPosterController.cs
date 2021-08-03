using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.HologramPoster.Mono
{
    internal class HologramPosterController : FcsDevice, IFCSSave<SaveData>,IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private string _selectedImagePath;
        private HologramDataEntry _savedData;
        private Texture2D _selectedImage;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.HologramPosterTabID, Mod.ModPackID);
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
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetHologramDataEntrySaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            IsInitialized = true;
        }

        internal void LoadImage(Texture2D texture2D)
        {
            MaterialHelpers.ChangeImage(gameObject, texture2D, "_Hologram_Rim_Flicker_Green");
            _selectedImagePath = GetImagePath(texture2D);
            _selectedImage = texture2D;
        }

        private static string GetImagePath(Texture2D texture2D)
        {
            var path = QPatch.Patterns.FirstOrDefault(x => x.Value == texture2D).Key;
            return path;
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

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;

            base.OnHandHover(hand);

            var data = new[]
            {
                "Click to change image!"
            };

            data.HandHoverPDAHelperEx(GetTechType(),HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            ImageSelectorHUD.Main.Show(QPatch.PatternsIcon,_selectedImage,((texture2D, sprite) =>
            {
                LoadImage(texture2D);
            }));
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new HologramDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.SelectedTexturePath = _selectedImagePath;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.HologramDataEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}
