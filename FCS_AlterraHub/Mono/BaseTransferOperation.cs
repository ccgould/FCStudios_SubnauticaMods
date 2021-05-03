using System.Collections.Generic;
using FCS_AlterraHub.Registration;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_AlterraHub.Mono
{
    public class BaseTransferOperation
    {
        private FcsDevice _device;
        public string DeviceId { get; set; }
        public List<TechType> TransferItems { get; set; } = new List<TechType>();
        public int MaxAmount { get; set; } = 1;
        public int PullWhenAmountIsAbove { get; set; } = 0;
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

        public bool IsEnabled { get; set; }

        public BaseTransferOperation()
        {
            
        }

        public BaseTransferOperation(BaseTransferOperation operation)
        {
            DeviceId = operation.DeviceId;
            MaxAmount = operation.MaxAmount;
            IsPullOperation = operation.IsPullOperation;
            TransferItems = new List<TechType>(operation.TransferItems);
        }
        
        public bool IsSimilar(BaseTransferOperation operation)
        {
            return operation.DeviceId == DeviceId &&
                   operation.TransferItems == TransferItems &&
                   operation.MaxAmount == MaxAmount &&
                   operation.IsPullOperation;
        }
    }
}