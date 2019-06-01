namespace FCSTechWorkBench.Interfaces
{
    internal interface IFCSTechWorkBenchItem
    {
        TechType TechTypeID { get; set; }
        string ClassID_I { get; set; }
        string FriendlyName_I { get; set; }
    }
}
