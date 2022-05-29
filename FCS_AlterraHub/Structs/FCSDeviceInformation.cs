namespace FCS_AlterraHub.Structs
{
    public struct FCSDeviceInformation
    {
        public string UnitID { get; set; }
        public string Status { get; set; }
        public bool IsOperational { get; set; }
        public bool IsVisibleToNetwork { get; set; }
        public string PrefabID { get; set; }
    }
}
