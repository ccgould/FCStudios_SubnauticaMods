using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
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
        private Text _powerUsagePerSecond;
        private Text _generationTime;
        private HarvesterSpeedButton _speedBTN;
        private FCSToggleButton _lightToggleBTN;
        private List<TechType> _loadedDNASamples;

        internal void Setup(HydroponicHarvesterController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                UpdateUI();
                _isInitialized = true;
            }
        }
       
        public override void OnButtonClick(string btnName, object Tag)
        {
            if (btnName.Equals("BackBTN"))
            {
                GoToHome();
            }

            if(btnName.Equals("SlotButton"))
            {
                if (Tag == null) return;
                var data = (SlotData)Tag;
                QuickLogger.Debug($"Data: {data} | TechType: {data.TechType} | Slot: {data.SlotId} | GrowBed: {_mono?.GrowBedManager}",true);
                _mono.GrowBedManager.TakeItem(data.TechType,data.SlotId);
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                var canvas = gameObject.FindChild("Canvas2");

                //Add ProximityActivate to controller screen visiblity
                var px = canvas.AddComponent<ProximityActivate>();
                px.Initialize(canvas.gameObject, gameObject, 2);

                _dnaSamplePage = GameObjectHelpers.FindGameObject(gameObject, "DNAPicker");
                _homePage = GameObjectHelpers.FindGameObject(gameObject, "Home");

                var backBTNObj = InterfaceHelpers.FindGameObject(gameObject, "Image");
                InterfaceHelpers.CreateButton(backBTNObj, "BackBTN",InterfaceButtonMode.Background,OnButtonClick,Color.gray,Color.white, 5,AuxPatchers.HarvesterBackButton(), AuxPatchers.HarvesterBackButtonDesc());
                
                var speedBTNObj = InterfaceHelpers.FindGameObject(gameObject, "SpeedBTN");
                _speedBTN = speedBTNObj.AddComponent<HarvesterSpeedButton>();
                _speedBTN.GrowBedManager = _mono.GrowBedManager;

                var lightToggleBTNObj = GameObjectHelpers.FindGameObject(gameObject, "LightBTN");
                _lightToggleBTN = lightToggleBTNObj.AddComponent<FCSToggleButton>();
                _lightToggleBTN.BtnName = "LightBTN";
                _lightToggleBTN.ButtonMode = InterfaceButtonMode.Background;
                _lightToggleBTN.TextLineOne = AuxPatchers.HarvesterToggleLight();
                _lightToggleBTN.TextLineTwo = AuxPatchers.HarvesterToggleLightDesc();
                _lightToggleBTN.OnButtonClick += (s, o) =>
                {
                    _mono.EffectsManager.SetBreaker(_lightToggleBTN.IsSelected);
                };

                var hi1 = GameObjectHelpers.FindGameObject(gameObject, "HarvesterScreenItem");
                var slot1 = GameObjectHelpers.FindGameObject(hi1, "Icon").AddComponent<SlotItemTab>();
                slot1.Initialize(this,OnButtonClick,0);

                var hi2 = GameObjectHelpers.FindGameObject(gameObject, "HarvesterScreenItem (1)");
                var slot2 = GameObjectHelpers.FindGameObject(hi2, "Icon").AddComponent<SlotItemTab>();
                slot2.Initialize(this,OnButtonClick,1);

                var hi3 = GameObjectHelpers.FindGameObject(gameObject, "HarvesterScreenItem (2)");
                var slot3 = GameObjectHelpers.FindGameObject(hi3, "Icon").AddComponent<SlotItemTab>();
                slot3.Initialize(this,OnButtonClick,2);

                _landSamplesGrid = InterfaceHelpers.FindGameObject(gameObject, "LandSamplesGrid");
                _aquaticSamplesGrid = InterfaceHelpers.FindGameObject(gameObject, "AquaticSamplesGrid");
                _powerUsagePerSecond =InterfaceHelpers.FindGameObject(gameObject, "PowerUsagePerSecond").GetComponent<Text>();
                _generationTime =InterfaceHelpers.FindGameObject(gameObject, "UnitPerSecond").GetComponent<Text>();

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

        internal void LoadKnownSamples()
        {
            if (_loadedDNASamples == null)
            {
                _loadedDNASamples = new List<TechType>();
            }

            var knownSamples = Mod.GetHydroponicKnownTech();

            if (knownSamples == null) return;

            foreach (var sample in knownSamples)
            {
                if(sample.TechType == TechType.None || _loadedDNASamples.Contains(sample.TechType) || Mod.IsNonePlantableAllowedList.Contains(sample.TechType)) continue;
                var button = GameObject.Instantiate(ModelPrefab.HydroponicDNASamplePrefab).AddComponent<InterfaceButton>();
                var icon = GameObjectHelpers.FindGameObject(button.gameObject, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(sample.TechType);
                button.TextLineOne = Language.main.Get((sample.TechType));
                button.Tag = sample;
                button.HOVER_COLOR = Color.white;
                button.STARTING_COLOR = Color.gray;
                button.InteractionRequirement = InteractionRequirement.None;
                button.OnButtonClick += (s, o) =>
                {
                    _caller.SetIcon((DNASampleData)o);
                    GoToHome();
                };
                _loadedDNASamples.Add(sample.TechType);
                button.gameObject.transform.SetParent(sample.IsLandPlant ? _landSamplesGrid.transform : _aquaticSamplesGrid.transform, false);
            }
        }

        internal void UpdateUI()
        {
            _powerUsagePerSecond.text = AuxPatchers.PowerUsagePerSecondFormat(_mono.GetPowerUsage());
            _generationTime.text = AuxPatchers.GenerationTimeFormat(Convert.ToSingle(_mono.GrowBedManager.GetCurrentSpeedMode()));
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
        
        public void SetSpeedGraphic(SpeedModes speedMode)
        {
            _speedBTN.SetSpeedMode(speedMode);
        }

        public void SetLightGraphicOff()
        {
            _lightToggleBTN.Select();
        }
    }
}
