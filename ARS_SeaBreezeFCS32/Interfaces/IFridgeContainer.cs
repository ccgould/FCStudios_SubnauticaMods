namespace ARS_SeaBreezeFCS32.Interfaces
{
    internal interface IFridgeContainer
    {
        bool IsFull { get; }
        int NumberOfItems { get; set; }
        void OpenStorage();
    }
}
