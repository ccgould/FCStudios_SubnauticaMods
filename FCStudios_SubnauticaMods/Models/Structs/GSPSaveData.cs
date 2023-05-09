using FCS_AlterraHub.Core.Services;

namespace FCS_AlterraHub.Models.Structs
{
    internal struct GSPSaveData
    {
        public bool AutomaticDebitDeduction { get; set; }
        public float Rate { get; set; }
        public ShipmentInfo PDAShipmentInfo { get; set; }

        public GSPSaveData()
        {
            
        }

        public GSPSaveData(GamePlayService service)
        {
            AutomaticDebitDeduction = service.GetAutomaticDebitDeduction();
            Rate = service.GetRate();
            PDAShipmentInfo = service.GetShipmentInfo();
        }
    }
}
