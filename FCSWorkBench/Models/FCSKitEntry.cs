namespace FCSTechFabricator
{
    internal class FCSKitEntry
    {
        public string ClassID { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string ModParent { get; set; }
        public string[] FabricatorSteps { get; set; }
        public string Icon { get; set; }
        public TechType RequiredForUnlock { get; set; } = TechType.None;
    }
}