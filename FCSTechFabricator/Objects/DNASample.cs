namespace FCSTechFabricator.Objects
{
    public class DNASample
    {
        public TechType Ingredient { get; }
        public string Description { get; }
        public int Amount { get; } = 1;
        public string ClassID { get; }
        public string Friendly { get; }

        public DNASample(string friendlyName, string classId, TechType techType, int amountToReturn = 1)
        {
            Ingredient = techType;
            Description =
                $"This DNA sample of a {friendlyName} will allow you to clone it multiple times in the appropriate FCS machine";
            Amount = amountToReturn;
            ClassID = $"{classId}_FCSDNA";
            Friendly = $"{friendlyName} DNA Sample";
        }
    }
}
