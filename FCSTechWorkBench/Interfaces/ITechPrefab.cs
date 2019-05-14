namespace FCSTechWorkBench.Interfaces
{
    public interface ITechPrefab
    {
        // Property signatures
        string ClassID_I { get; set; }
        TechType TechType_I { get; set; }

        #region Public Methods
        void RegisterItem();
        #endregion
    }
}
