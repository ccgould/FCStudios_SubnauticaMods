using System;
using System.Collections;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.DataCollectors;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.PatreonStatue.Mono
{
    class PatreonStatueController : FcsDevice, IFCSSave<SaveData>
    {


        private PatreonStatueDataEntry _savedData;
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private bool scrollCredits;
        public Text counterText;
        public Text nameSlot;
        public GameObject CreditScreen;
        public GameObject LoadingScreen;
        private float startFadeTime;
        public int DelayAmount = 1; // Second count
        protected float Timer;
        public float _value;
        private int _index;
        private bool _isShowing;
        private int _totalPatreons;
        private Light _light;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraHubDepotTabID, Mod.ModPackID);
            RefreshUI();
            InvokeRepeating(nameof(GetPatreonData),1f,1f);
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
                    
                    _light.color = _savedData.ColorTemplate.EmissionColor.Vector4ToColor();
                    _colorManager.LoadTemplate(_savedData.ColorTemplate);

                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol,string.Empty, AlterraHub.BaseLightsEmissiveController);
            }

            if (nameSlot == null)
            {
                nameSlot = GameObjectHelpers.FindGameObject(gameObject, "NameSlot")?.GetComponent<Text>();
            }

            if (counterText == null)
            {
                counterText = GameObjectHelpers.FindGameObject(gameObject, "amount")?.GetComponent<Text>();
            }

            if (CreditScreen == null)
            {
                CreditScreen = GameObjectHelpers.FindGameObject(gameObject, "CreditScreen");
            }

            if (LoadingScreen == null)
            {
                LoadingScreen = GameObjectHelpers.FindGameObject(gameObject, "LoadingScreen");
            }

            _light = gameObject.GetComponentInChildren<Light>(true);

            var lsq = _light.gameObject.EnsureComponent<LightShadowQuality>();
            lsq.light = _light;
            lsq.qualityPairs = new[]
            {
                new LightShadowQuality.ShadowQualityPair
                {
                    lightShadows = LightShadows.None,
                    qualitySetting = 0
                },
                new LightShadowQuality.ShadowQualityPair
                {
                    lightShadows = LightShadows.Hard,
                    qualitySetting = 2
                }
            };


            var rgL = gameObject.AddComponent<RegistredLightSource>();
            rgL.hostLight = _light;

            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 4f);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"In OnProtoSerialize -  Hub Depot");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new PatreonStatueDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            newSaveData.PatreonStatueEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPatreonStatueEntrySaveData(GetPrefabID());
        }
        
        private void GetPatreonData()
        {
            if(PatreonCollector.GetIsRunning()) return;
            LoadingScreen.SetActive(false);
            CreditScreen.SetActive(true);
            _value = 1f;
            scrollCredits = true;
            _totalPatreons = PatreonCollector.GetTotalPatrons();
            CancelInvoke(nameof(GetPatreonData));
        }

        private void Update()
        {
            if (nameSlot == null) return;
            if (scrollCredits)
            {

                Timer += Time.deltaTime;

                if (Timer >= DelayAmount && !_isShowing)
                {
                    Timer = 0f;
                    _value = Mathf.Clamp(_value - 0.01f, 0.0f, 1.0f);
                    if (_index > _totalPatreons - 1)
                    {
                        _index = 0;
                    }

                    counterText.text = $"{_index + 1}/{_totalPatreons}";
                    StartCoroutine(ShowPatreon(PatreonCollector.GetPatreon(_index)));
                    _index++;
                }
            }
        }

        private IEnumerator ShowPatreon(string patronName)
        {
            _isShowing = true;

            nameSlot.text = patronName;

            nameSlot.color = new Color(1f, 1f, 1f, 0);

            while (nameSlot.color.a < 1)
            {
                nameSlot.color = new Color(1, 1, 1, nameSlot.color.a + .1f);
                yield return null;
            }

            yield return new WaitForSeconds(DelayAmount);

            while (nameSlot.color.a > 0)
            {
                nameSlot.color = new Color(1, 1, 1, nameSlot.color.a - .1f);
                yield return null;
            }

            _isShowing = false;
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            var result = _colorManager.ChangeColor(template);

            if (_light != null)
            {
                _light.color = template.EmissionColor;
            }

            return result;
        }
    }
}
