using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Abstract;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.BaseOperator.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.BaseOperator.Mono
{
    internal class BaseOperatorDisplay : AIDisplay
    {
        private Text _clock;
        private bool _useSystemTime;
        private int _lastMinute;
        private DigitalClockFormat _format;
        private string _periodText;
        private BaseOperatorController _mono;
        private Text _baseName;
        private Text _passedDays;
        private Text _baseStrength;
        private BaseHullStrength _baseHullStrengthComponent;
        private Text _temperature;
        private FieldInfo _isLightsOnField;
        private ToggleController _interiorLightToggle;

        private void Update()
        {
            UpdateClock();
            ToGameDays();
            UpdateBaseHullStrength();
        }

        private void UpdateBaseTemperature()
        {
            WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
            _temperature.text = Language.main.GetFormat("ThermalPlantCelsius", main.GetTemperature(_mono.Manager.Habitat.transform.position));
        }

        private void UpdateBaseHullStrength()
        {
            if (_baseHullStrengthComponent == null)
            {
                _baseHullStrengthComponent =
                    _mono.Manager.Habitat.gameObject.GetComponentInChildren<BaseHullStrength>();
            }


            _baseStrength.text = _baseHullStrengthComponent.GetTotalStrength().ToString();
        }

        private void UpdateClock()
        {
            if (_clock != null)
            {
                TimeSpan timeSpan = GetTime();
                if (_lastMinute != timeSpan.Minutes)
                {
                    if (_format == DigitalClockFormat.TWELVE_HOUR)
                    {
                        bool flag;
                        timeSpan = TwentyFourHourToTwelveHourFormat(timeSpan, out flag);
                        _periodText = (flag ? "AM" : "PM");
                    }
                    _lastMinute = timeSpan.Minutes;
                    _clock.text = EncodeMinHourToString(EncodeMinuteAndHour(timeSpan.Minutes, timeSpan.Hours));
                }
            }
        }

        public static int EncodeMinuteAndHour(int minute, int hour)
        {
            return hour * 60 + minute;
        }

        public static string EncodeMinHourToString(int encoded)
        {
            if (encoded >= 0 && encoded < 1440)
            {
                return TimeCache.S_TimeCache[encoded];
            }
            return "Er:rr";
        }

        public static TimeSpan TwentyFourHourToTwelveHourFormat(TimeSpan timeSpan, out bool isMorning)
        {
            int num = timeSpan.Hours;
            isMorning = (num < 12);
            num %= 12;
            if (num == 0)
            {
                num += 12;
            }
            return new TimeSpan(num, timeSpan.Minutes, timeSpan.Seconds);
        }

        protected TimeSpan GetTime()
        {
            if (_useSystemTime)
            {
                DateTime now = DateTime.Now;
                new TimeSpan(now.Hour, now.Minute, now.Second);
                return new TimeSpan(now.Hour, now.Minute, now.Second);
            }
            float dayScalar = DayNightCycle.main.GetDayScalar();
            int hours = Mathf.FloorToInt(dayScalar * 24f);
            int minutes = Mathf.FloorToInt(Mathf.Repeat(dayScalar * 24f * 60f, 60f));
            return new TimeSpan(hours, minutes, 0);
        }

        internal void Setup(BaseOperatorController mono)
        {
            _mono = mono;
            IsLightsOn = true;

            if (FindAllComponents())
            {
                InvokeRepeating(nameof(UpdateBaseTemperature), UnityEngine.Random.value, 10f);
                _isLightsOnField = typeof(SubRoot).GetField("subLightsOn", BindingFlags.Instance | BindingFlags.NonPublic);
            }
        }

        public bool IsLightsOn { get; set; }

        public override void OnButtonClick(string btnName, object tag)
        {
            throw new NotImplementedException();
        }

        private void ToggleLights(bool value)
        {
            _mono.Manager.Habitat.ForceLightingState(value);
        }
        public override bool FindAllComponents()
        {
            try
            {
                //Clock
                _clock = GameObjectHelpers.FindGameObject(gameObject, "Time")?.GetComponent<Text>();

                //Base Name
                _baseName = GameObjectHelpers.FindGameObject(gameObject, "BaseName")?.GetComponent<Text>();
                
                //Days
                _passedDays = GameObjectHelpers.FindGameObject(gameObject, "DaysPassed")?.GetComponent<Text>();
                
                //Base Intergraty
                _baseStrength = GameObjectHelpers.FindGameObject(gameObject, "HullStrength")?.GetComponent<Text>();

                //Temperature
                _temperature = GameObjectHelpers.FindGameObject(gameObject, "Temperature")?.GetComponent<Text>();

                //Interior Light Toggle
                _interiorLightToggle = GameObjectHelpers.FindGameObject(gameObject, "InteriorLightToggle")?.AddComponent<ToggleController>();
                _interiorLightToggle.Initialize();
                _interiorLightToggle.OnToggleSwitched += ToggleLights;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        public void SetBaseName()
        {
            _baseName.text = _mono.Manager?.GetBaseName();
        }

        public void ToGameDays()
        {
            if (_passedDays == null) return;
            _passedDays.text = DayNightCycle.main.GetDayScalar().ToString("N2");
        }

        public enum DigitalClockFormat
        {
            // Token: 0x04000025 RID: 37
            TWELVE_HOUR,
            // Token: 0x04000026 RID: 38
            TWENTY_FOUR_HOUR
        }
    }
}
