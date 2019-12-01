namespace FCSTechFabricator
{
    public class FCSKitEntry
    {
        public string ClassID { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string ModParent { get; set; }
        public string[] FabricatorSteps { get; set; }
        public string Icon { get; set; }
        public TechType RequiredForUnlock { get; set; }

        //public FCSKitEntry(string classID,string friendlyName,string description, string modParent, string[] fabricatorSteps, string icon, TechType requiredForUnlock)
        //{
        //    ClassID = classID;
        //    FriendlyName = friendlyName;
        //    Description = description;
        //    ModParent = modParent;
        //    FabricatorSteps = fabricatorSteps;
        //    Icon = icon;
        //    RequiredForUnlock = requiredForUnlock;
        //}
    }
}