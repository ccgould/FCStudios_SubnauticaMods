using System;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class DisplayManager : AIDisplay
    {
        private bool _isInitialized;
        internal HydroponicHarvesterController _mono;
        private GameObject _dnaSamplePage;
        private GameObject _homePage;
        private GameObject _aquaticSamplesGrid;
        private GameObject _landSamplesGrid;
        private SlotItemTab _caller;

        internal void Setup(HydroponicHarvesterController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                _isInitialized = true;
            }
        }
        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName.Equals("BackBTN"))
            {
                GoToHome();
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                _dnaSamplePage = GameObjectHelpers.FindGameObject(gameObject, "DNAPicker");
                _homePage = GameObjectHelpers.FindGameObject(gameObject, "Home");

                var backBTNObj = InterfaceHelpers.FindGameObject(gameObject, "Image");
                InterfaceHelpers.CreateButton(backBTNObj, "BackBTN",InterfaceButtonMode.Background,OnButtonClick,Color.gray,Color.white, 5,AuxPatchers.HarvesterBackButton(), AuxPatchers.HarvesterBackButtonDesc());
                
                var speedBTNObj = InterfaceHelpers.FindGameObject(gameObject, "SpeedBTN");
                var speedBTN = speedBTNObj.AddComponent<HarvesterSpeedButton>();
                speedBTN.GrowBedManager = _mono.GrowBedManager;

                var lightToggleBTNObj = GameObjectHelpers.FindGameObject(gameObject, "LightBTN");
                var lightToggleBTN = lightToggleBTNObj.AddComponent<FCSToggleButton>();
                lightToggleBTN.BtnName = "LightBTN";
                lightToggleBTN.ButtonMode = InterfaceButtonMode.Background;
                lightToggleBTN.TextLineOne = AuxPatchers.HarvesterToggleLight();
                lightToggleBTN.TextLineTwo = AuxPatchers.HarvesterToggleLightDesc();
                lightToggleBTN.OnButtonClick += (s, o) =>
                {
                    _mono.EffectsManager.SetBreaker(lightToggleBTN.IsSelected);
                };

                var slot1 = GameObjectHelpers.FindGameObject(gameObject, "HarvesterScreenItem").AddComponent<SlotItemTab>();
                slot1.Initialize(this,OnButtonClick,0);
                var slot2 = GameObjectHelpers.FindGameObject(gameObject, "HarvesterScreenItem (1)").AddComponent<SlotItemTab>();
                slot2.Initialize(this,OnButtonClick,1);
                var slot3 = GameObjectHelpers.FindGameObject(gameObject, "HarvesterScreenItem (2)").AddComponent<SlotItemTab>();
                slot3.Initialize(this,OnButtonClick,2);

                _landSamplesGrid = InterfaceHelpers.FindGameObject(gameObject, "LandSamplesGrid");
                _aquaticSamplesGrid = InterfaceHelpers.FindGameObject(gameObject, "AquaticSamplesGrid");

                LoadKnownSamples();

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private void LoadKnownSamples()
        {
            var knownSamples = Mod.GetHydroponicKnownTech();
            foreach (var sample in knownSamples)
            {
                var button = GameObject.Instantiate(ModelPrefab.HydroponicDNASamplePrefab).AddComponent<InterfaceButton>();
                var icon = GameObjectHelpers.FindGameObject(button.gameObject, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(sample.Key);
                button.TextLineOne = Language.main.Get((sample.Key));
                button.Tag = sample.Key;
                button.OnButtonClick += (s, o) =>
                {
                    _caller.SetIcon((TechType)o);
                    GoToHome();
                };
                
                button.gameObject.transform.SetParent(sample.Value ? _landSamplesGrid.transform : _aquaticSamplesGrid.transform, false);
            }
        }

        public void OpenDnaSamplesPage(SlotItemTab slotItemTab)
        {
            _caller = slotItemTab;
            _dnaSamplePage.SetActive(true);
            _homePage.SetActive(false);
        }
        public void GoToHome()
        {
            _dnaSamplePage.SetActive(false);
            _homePage.SetActive(true);
        }

        public void ClearDNASample(SlotItemTab tab)
        {
            
        }
    }

    internal class HarvesterSpeedButton : InterfaceButton
    {
        private int _index;
        private Image _icon;
        private Sprite[] _images;
        internal GrowBedManager GrowBedManager { get; set; }

        private void Start()
        {
            _images = new[]
            {
                ModelPrefab.GetSprite("HH icon off"),
                ModelPrefab.GetSprite("HH icon low"),
                ModelPrefab.GetSprite("HH icon medium"),
                ModelPrefab.GetSprite("HH icon high")
            };
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            SwitchImage();
            GrowBedManager.SetSpeedMode(GetMode());
        }

        private void SwitchImage()
        {

            if (_icon == null)
            {
                _icon = gameObject.EnsureComponent<Image>();
            }
            _index += 1;

            if (_index > 3)
            {
                _index = 0;
            }
            _icon.sprite = _images[_index];
           
        }

        public SpeedModes GetMode()
        {
            switch (_index)
            {
                case 0:
                    return SpeedModes.Off;
                case 1:
                    return SpeedModes.Low;
                case 2:
                    return SpeedModes.High;
                case 3:
                    return SpeedModes.Max;
            }

            return SpeedModes.Off;
        }
    }
}
