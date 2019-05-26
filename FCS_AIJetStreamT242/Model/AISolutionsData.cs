using FCS_AIJetStreamT242.Buildable;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AIJetStreamT242.Model
{
    internal static partial class AISolutionsData
    {

        internal class BiomeItem
        {
            /// <summary>
            /// The speed of the turbine
            /// </summary>
            public float Speed { get; set; }
        }

        internal class BiomeOres
        {
            public List<string> AvaliableOres { get; set; }
        }

        public static Quaternion StartingRotation { get; set; }
        private static float _passedTime;

        public static event Action<Quaternion> OnRotationChanged;

        internal static void ChangeRotation()
        {
            var magNorth = -Input.compass.magneticHeading;
            StartingRotation = Quaternion.Euler(0, magNorth + RandomNumber.Between(-180, 180), 0);
            OnRotationChanged?.Invoke(StartingRotation);
        }

        internal static Vector3 GetStartingRotation(Transform transform)
        {
            Vector3 euler = transform.eulerAngles;
            euler.y = StartingRotation.y;
            return euler;
        }

        internal static Vector3 GetStartingRotation(Transform transform, float yAxis)
        {
            Vector3 euler = transform.eulerAngles;
            euler.y = yAxis;
            return euler;
        }

        internal static void UpdateTime()
        {
            if (DayNightCycle.main == null) return;
            _passedTime += DayNightCycle.main.deltaTime;
            if (_passedTime >= AIJetStreamT242Buildable.JetStreamT242Config.RotationCycleInSec)
            {
                QuickLogger.Debug($"ChangeRotation");
                ChangeRotation();
                _passedTime = 0.0f;
            }
        }
    }
}
