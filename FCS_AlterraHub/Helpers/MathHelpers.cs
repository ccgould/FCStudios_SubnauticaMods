using System;
using System.Collections.Generic;
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

        public static double PowerLossOverDistance(double distanceInMeters, double powerTransmitted)
        {

            //waveLengthInMeters = speed of light(m/s) * frequency(MHz)
            double speedOfLight = 3 * Math.Pow(10, 8);
            //QModServices.Main.AddCriticalMessage($"Speed Of Light: {speedOfLight}");
            double frequencyInGHz = 5 * Math.Pow(10, 9);
            //QModServices.Main.AddCriticalMessage($"Frequency: {frequencyInGHz}");
            double waveLengthInMeters = speedOfLight / frequencyInGHz;
            //QModServices.Main.AddCriticalMessage($"Wave Length: {waveLengthInMeters}");

            return Math.Pow(4 * Math.PI * distanceInMeters, 2) / Math.Pow(waveLengthInMeters, 2);
        }

        public static void ShuffleAll<T>(IList<T> list, int n = -1)
        {
            int count = list.Count;
            if (count == 0)
            {
                return;
            }

            if (n < 0 || n >= count - 1)
            {
                n = -1;
            }

            while (n < count - 1)
            {
                n++;
                int num = UnityEngine.Random.Range(n, count);
                if (n != num)
                {
                    T value = list[n];
                    list[n] = list[num];
                    list[num] = value;
                }
            }
        }

        public static decimal PercentageOfNumber(decimal percentage, decimal amount)
        {
            //Equation: Y = P% * X
            //Equation: Y = percentage * amount

            var p = percentage / 100;
            var y = p * amount;

            return y;
        }
    }
}
