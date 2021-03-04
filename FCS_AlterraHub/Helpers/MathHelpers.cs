using System;
using QModManager.API;
using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class MathHelpers
    {
        public static float GetRemainder(float amount, float total)
        {
            return total - amount;
        }

        public static string ToKiloFormat(this int num)
        {
            if (num >= 100000000)
                return (num / 1000000).ToString("#,0M");

            if (num >= 10000000)
                return (num / 1000000).ToString("0.#") + "M";

            if (num >= 100000)
                return (num / 1000).ToString("#,0K");

            if (num >= 10000)
                return (num / 1000).ToString("0.#") + "K";

            return num.ToString("#,0");
        }

        public static double PowerLossOverDistance(double distanceInMeters)
        {
            //waveLengthInMeters = speed of light(m/s) * frequency(MHz)
            double speedOfLight = 3 * Math.Pow(10,8);
            QModServices.Main.AddCriticalMessage($"Speed Of Light: {speedOfLight}");
            double frequencyInGHz = 5 * Math.Pow(10, 9);
            QModServices.Main.AddCriticalMessage($"Frequency: {frequencyInGHz}");
            double waveLengthInMeters = speedOfLight / frequencyInGHz;
            QModServices.Main.AddCriticalMessage($"Wave Length: {waveLengthInMeters}");

            return Math.Pow(4 * Math.PI * distanceInMeters, 2) / Math.Pow(waveLengthInMeters, 2);
        }
    }
}
