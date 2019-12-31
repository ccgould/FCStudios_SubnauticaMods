namespace FCSTechFabricator.Interfaces
{
    internal interface IFCSTechFabricatorItem
    {
        TechType TechTypeID { get; set; }
        string ClassID_I { get; set; }
        string FriendlyName_I { get; set; }
    }
}
