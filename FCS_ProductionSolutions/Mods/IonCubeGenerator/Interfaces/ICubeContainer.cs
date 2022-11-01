namespace FCS_ProductionSolutions.Mods.IonCubeGenerator.Interfaces
{
    internal interface ICubeContainer
    {
        bool IsFull { get; }
        int NumberOfCubes { get; set; }
        void OpenStorage();
    }
}