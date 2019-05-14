namespace FCS_Alterra_Refrigeration_Solutions.Models.Base
{
    /// <summary>
    /// A Class that defines an Eatable object in the fridge
    /// </summary>
    public class EatableData
    {
        /// <summary>
        /// The ID of the eatable Object
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the eatable object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The food value of the eatable object
        /// </summary>
        public float FoodValue { get; set; }

        /// <summary>
        /// The water value of the eatable object
        /// </summary>
        public float WaterValue { get; set; }

        /// <summary>
        /// The kDecay rate of the eatable object
        /// </summary>
        public float KDecayRate { get; set; }

        /// <summary>
        /// The TechType of the eatable object
        /// </summary>
        public TechType TechType { get; set; }

        /// <summary>
        /// Boole that declares if the object will decompose
        /// </summary>
        public bool Decomposes { get; set; }
    }
}
