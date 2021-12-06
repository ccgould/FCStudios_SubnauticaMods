using System;
using FCS_AlterraHub.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.DisplayBoard.Mono
{
    internal class DisplayScreenController : MonoBehaviour
    {
        private Button _settingsBTN;
        private GameObject _informationScreen;
        private GameObject _optionSelection;
        private Text _informationText;
        private Text _titleText;
        private float _temperature;
        private DisplayMode _displayMode;
        private string _customtext;
        private DisplayBoardController _controller;
        private Button _renameBTN;

        internal void Initialize(DisplayBoardController controller)
        {
            _controller = controller;
            _informationScreen = GameObjectHelpers.FindGameObject(gameObject, "InformationScreen");
            _optionSelection = GameObjectHelpers.FindGameObject(gameObject, "OptionSelection");
            _informationText = GameObjectHelpers.FindGameObject(gameObject, "Information")?.GetComponent<Text>();
            _titleText = GameObjectHelpers.FindGameObject(gameObject, "Title")?.GetComponent<Text>();
            
            _settingsBTN = GameObjectHelpers.FindGameObject(gameObject, "SettingsBTN")?.GetComponent<Button>();
            _settingsBTN?.onClick.AddListener((() =>
            {
                HideSettingsButton();
                ShowOptionsScreen();
            }));

            var timeBTN = GameObjectHelpers.FindGameObject(gameObject, "TimeBTN")?.GetComponent<Button>();
            timeBTN?.onClick.AddListener((() =>
            {
                _displayMode = DisplayMode.Time;
                HideOptionsScreen();
                ShowInformationScreen();
            }));

            var hullStrengthBTN = GameObjectHelpers.FindGameObject(gameObject, "HullStrengthBTN")?.GetComponent<Button>();
            hullStrengthBTN?.onClick.AddListener((() =>
            {
                _displayMode = DisplayMode.BaseHullStrength;
                HideOptionsScreen();
                ShowInformationScreen();
            }));

            var temperatureBTN = GameObjectHelpers.FindGameObject(gameObject, "TemperatureBTN")?.GetComponent<Button>();
            temperatureBTN?.onClick.AddListener((() =>
            {
                _displayMode = DisplayMode.Temperature;
                HideOptionsScreen();
                ShowInformationScreen();
                QueryTemperature();
            }));

            var customNameBTN = GameObjectHelpers.FindGameObject(gameObject, "CustomTextBTN")?.GetComponent<Button>();
            customNameBTN?.onClick.AddListener((() =>
            {
                _displayMode = DisplayMode.CustomText;
                HideOptionsScreen();
                ShowInformationScreen();
            })); 

            var biomeBTN = GameObjectHelpers.FindGameObject(gameObject, "BiomeBTN")?.GetComponent<Button>();
            biomeBTN?.onClick.AddListener((() =>
            {
                _displayMode = DisplayMode.Biome;
                HideOptionsScreen();
                ShowInformationScreen();
                UpdateInformationText(CalculateBiome());
            }));

            _renameBTN = GameObjectHelpers.FindGameObject(gameObject, "RenameBTN")?.GetComponent<Button>();
            _renameBTN?.onClick.AddListener((() =>
            {
                uGUI.main.userInput.RequestString("Enter Beacon Name", "Rename", _customtext, 100,
                    (text =>
                    {
                        _customtext = text;
                        UpdateInformationText(text);
                    }));
                
            }));

            var settingsInfBTN = GameObjectHelpers.FindGameObject(gameObject, "InfSettingsBTN")?.GetComponent<Button>();
            settingsInfBTN?.onClick.AddListener((() =>
            {
                HideSettingsButton();
                ShowOptionsScreen(); 
            }));
            
            InvokeRepeating(nameof(QueryTemperature), UnityEngine.Random.value, 10f);
            InvokeRepeating(nameof(GetTime), UnityEngine.Random.value, 1f);
            InvokeRepeating(nameof(GetBaseHullStrength), UnityEngine.Random.value, 1f);
            InvokeRepeating(nameof(GetBiome), UnityEngine.Random.value, 1f);
        }

        private string CalculateBiome()
        {
            return LargeWorld.main ? LargeWorld.main.GetBiome(base.transform.position) : "<Unknown>";
        }        
        
        private void GetBiome()
        {
            if (_displayMode == DisplayMode.Biome && string.IsNullOrWhiteSpace(_informationText.text))
            {
                UpdateInformationText(CalculateBiome());
            }
        }

        private void UpdateInformationText(string text)
        {
            _informationText.text = text;
        }

        private void GetTime()
        {
            if (_displayMode == DisplayMode.Time)
            {
                UpdateInformationText(WorldHelpers.GetGameTimeFormat());
            }
        }        
        
        private void GetBaseHullStrength()
        {
            if (_displayMode == DisplayMode.BaseHullStrength)
            {
                UpdateInformationText(_controller?.Manager?.GetBaseHullStrength());
            }
        }

        private void Update()
        {

        }

        private void ShowOptionsScreen()
        {
            _optionSelection?.SetActive(true);
        }

        private void HideOptionsScreen()
        {
            _optionSelection?.SetActive(false);
        }

        private void ShowInformationScreen()
        {
            SetTitle();
            _informationText.color = Color.white;
            _informationText.text = string.Empty;
            _informationScreen?.SetActive(true);
            _renameBTN.gameObject.SetActive(_displayMode == DisplayMode.CustomText);
        }
        
        private void QueryTemperature()
        {
            if (_displayMode == DisplayMode.Temperature)
            {
                WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
                if (main)
                {
                    _temperature = Mathf.Max(_temperature, main.GetTemperature(transform.position));
                    var temp = Language.main.GetFormat("ThermalPlantCelsius", _temperature);

                    _informationText.text =  string.IsNullOrWhiteSpace(temp) ? "LOADING" : temp;
                    float value = _temperature / 100f;
                    //_informationText.material.SetFloat(ShaderPropertyID._Amount, value);
                    if (_temperature < 25f)
                    {
                        _informationText.color = Color.red;
                        _informationText.color = Color.red;
                        return;
                    }
                    float num = (_temperature - 25f) / 75f;
                    Color color;
                    if (num < 0.3f)
                    {
                        color = UWE.Utils.LerpColor(Color.red, Color.yellow, num / 0.4f);
                    }
                    else
                    {
                        color = UWE.Utils.LerpColor(Color.yellow, Color.green, (num - 0.4f) / 0.6f);
                    }
                    _informationText.color = color;
                }
                else
                {
                    _informationText.color = Color.white;
                    _informationText.text = "LOADING";
                }
            }
        }

        private void SetTitle()
        {
            switch (_displayMode)
            {
                case DisplayMode.Time:
                    _titleText.text = "TIME";
                    break;
                case DisplayMode.CustomText:
                    _titleText.text = string.Empty;
                    break;
                case DisplayMode.BaseHullStrength:
                    _titleText.text = "HULL STRENGTH";
                    break;
                case DisplayMode.Temperature:
                    _titleText.text = "TEMPERATURE";
                    break;
                case DisplayMode.Biome:
                    _titleText.text = "BIOME";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HideInformationScreen()
        {
            _informationScreen?.SetActive(false);
        }

        private void HideSettingsButton()
        {
            _settingsBTN.gameObject.SetActive(false);
        }

        private void ShowSettingsButton()
        {
            _settingsBTN.gameObject.SetActive(true);
        }

        public Tuple<string,DisplayMode> Save()
        {
            return new(_customtext, _displayMode);
        }

        public void Load(Tuple<string, DisplayMode> savedDataDisplay1)
        {
            _customtext = savedDataDisplay1.Item1;
            _displayMode = savedDataDisplay1.Item2;

            if(_displayMode != DisplayMode.None)
            {
                HideSettingsButton();
                HideOptionsScreen();
                ShowInformationScreen();

                if (savedDataDisplay1.Item2 == DisplayMode.CustomText)
                {
                    UpdateInformationText(_customtext);
                }
            }
        }
    }
}