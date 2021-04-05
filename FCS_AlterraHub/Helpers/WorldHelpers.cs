using System;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Helpers
{
    public static class WorldHelpers
    {
        private static bool _useSystemTime;
        private static int _lastMinute;
        private static DigitalClockFormat _format;
        private static string _previousTime;


        public static string GetGameTimeFormat()
        {
            string period = string.Empty;
            TimeSpan timeSpan = GetTime();
            if (_lastMinute != timeSpan.Minutes)
            {
                if (_format == DigitalClockFormat.TWELVE_HOUR)
                {
                    bool flag;
                    timeSpan = TwentyFourHourToTwelveHourFormat(timeSpan, out flag);
                    period = (flag ? "AM" : "PM");
                }
                _lastMinute = timeSpan.Minutes;
                _previousTime = $"{EncodeMinHourToString(EncodeMinuteAndHour(timeSpan.Minutes, timeSpan.Hours))} {period}";
            }

            return _previousTime;
        }

        public static void SetFormat(DigitalClockFormat format)
        {
            _format = format;
        }

        public static string EncodeMinHourToString(int encoded)
        {
            if (encoded >= 0 && encoded < 1440)
            {
                return TimeCache.S_TimeCache[encoded];
            }
            return "Er:rr";
        }

        public static int EncodeMinuteAndHour(int minute, int hour)
        {
            return hour * 60 + minute;
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

        private static TimeSpan GetTime()
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

        public enum DigitalClockFormat
        {
            TWELVE_HOUR,
            TWENTY_FOUR_HOUR
        }

        public static bool CheckIfInRange(FcsDevice currentDevice,FcsDevice device,float range)
        {
            if (currentDevice == null || device == null || !device.gameObject.activeSelf || !currentDevice.IsConstructed) return false;
            float distance = Vector3.Distance(currentDevice.gameObject.transform.position, device.gameObject.transform.position);
            return distance <= range;
        }

        public static float GetDistance(FcsDevice currentDevice, FcsDevice device)
        {
            if (currentDevice == null || device == null ) return 0f;
            return Vector3.Distance(currentDevice.gameObject.transform.position, device.gameObject.transform.position);
        }

        public static bool CheckIfInRange(GameObject mainGameObject, GameObject gameObject, float range)
        {
            return Vector3.Distance(mainGameObject.transform.position, gameObject.transform.position) <= range;
        }
    }
}
