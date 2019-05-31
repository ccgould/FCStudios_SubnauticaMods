namespace FCSCommon.Objects
{
    /// <summary>
    /// Defines the Eatable object 
    /// </summary>
    public class EatableEntities
    {
        public TechType TechType { get; set; }

        public string PrefabID { get; set; }

        public string Name { get; set; }

        public float WaterValue { get; set; }

        public float FoodValue { get; set; }

        public float kDecayRate { get; set; }
        public bool Decomposes { get; set; }
    }
}
