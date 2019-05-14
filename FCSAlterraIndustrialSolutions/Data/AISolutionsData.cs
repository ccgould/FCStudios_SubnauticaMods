using FCSAlterraIndustrialSolutions.Handlers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Data
{
    public static partial class AISolutionsData
    {

        public class BiomeItem
        {
            /// <summary>
            /// The speed of the turbine
            /// </summary>
            public float Speed { get; set; }
        }

        public class BiomeOres
        {
            public List<string> AvaliableOres { get; set; }
        }

        public static Quaternion StartingRotation { get; set; }
        private static float _passedTime;

        public static event Action<Quaternion> OnRotationChanged;

        public static void ChangeRotation()
        {
            var magNorth = -Input.compass.magneticHeading;
            StartingRotation = Quaternion.Euler(0, magNorth + RandomNumber.Between(-180, 180), 0);
            OnRotationChanged?.Invoke(StartingRotation);
        }

        public static Vector3 GetStartingRotation(Transform transform)
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

        public static void UpdateTime()
        {
            if (DayNightCycle.main == null) return;
            _passedTime += DayNightCycle.main.deltaTime;
            if (_passedTime >= LoadItems.JetStreamT242Config.DamageCycleInSec)
            {
                ChangeRotation();
                _passedTime = 0.0f;
            }
        }
    }
}
