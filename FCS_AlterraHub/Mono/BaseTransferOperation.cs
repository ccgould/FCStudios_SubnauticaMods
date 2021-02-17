using FCS_AlterraHub.Registration;
using Oculus.Newtonsoft.Json;

namespace FCS_AlterraHub.Mono
{
    public class BaseTransferOperation
    {
        private FcsDevice _device;
        public string DeviceId { get; set; }
        public TechType TransferItem { get; set; }
        public int Amount { get; set; }
        public bool IsPullOperation { get; set; }

        [JsonIgnore] public FcsDevice Device
        {
            get
            {
                if (_device == null && !string.IsNullOrWhiteSpace(DeviceId))
                {
                    _device = FCSAlterraHubService.PublicAPI.FindDevice(DeviceId).Value;
                }
                return _device;
            }
        }

        public bool IsSimilar(BaseTransferOperation operation)
        {
            return operation.DeviceId == DeviceId &&
                   operation.TransferItem == TransferItem &&
                   operation.Amount == Amount &&
                   operation.IsPullOperation;
        }
    }
}