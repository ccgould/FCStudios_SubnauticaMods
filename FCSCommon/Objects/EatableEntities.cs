using System;
using System.Collections.Generic;
using System.Text;

namespace FCSCommon.Objects
{
    /// <summary>
    /// Defines the Eatable object 
    /// </summary>
    public class EatableEntities
    {
        public string Name { get; set; }

        public float WaterValue { get; set; }

        public float FoodValue { get; set; }

        public float kDecayRate { get; set; }
    }
}
