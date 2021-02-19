using System.Collections.Generic;
using FCS_AlterraHub.Registration;
using Oculus.Newtonsoft.Json;

namespace FCS_AlterraHub.Mono
{
    public class BaseTransferOperation
    {
        private FcsDevice _device;
        public string DeviceId { get; set; }
        public List<TechType> TransferItems { get; set; } = new List<TechType>();
        public int Amount { get; set; } = 1;
        public bool IsPullOperation { get; set; }
        public bool IsBeingEdited { get; set; }


        [JsonIgnore]
        public FcsDevice Device
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

        public BaseTransferOperation()
        {
            
        }

        public BaseTransferOperation(BaseTransferOperation operation)
        {
            DeviceId = operation.DeviceId;
            Amount = operation.Amount;
            IsPullOperation = operation.IsPullOperation;
            TransferItems = new List<TechType>(operation.TransferItems);
        }
        
        public bool IsSimilar(BaseTransferOperation operation)
        {
            return operation.DeviceId == DeviceId &&
                   operation.TransferItems == TransferItems &&
                   operation.Amount == Amount &&
                   operation.IsPullOperation;
        }
    }
}