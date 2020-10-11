using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;

namespace DataStorageSolutions.Model
{
    internal class FCSOperation
    {
        private FCSConnectableDevice _connectableDevice;
        private string _id;

        internal FCSConnectableDevice ConnectableDevice
        {
            get => _connectableDevice;
            set
            {
                _connectableDevice = value;

                if (Manager == null && value != null)
                {
                    Manager = BaseManager.FindManager(value.GetHabitat());
                }
            }
        }

        internal BaseManager Manager { get; set; }

        [JsonProperty]
        internal string ID
        {
            get => _id;
            set
            {
                _id = value;
            }
        }

        [JsonProperty] internal bool IsTransferAllowed { get; set; }
        [JsonProperty] internal ObservableCollection<TransferOperationData> TransferRequestOperations { get; set; } = new ObservableCollection<TransferOperationData>();
        [JsonProperty] internal ObservableCollection<AutoCraftOperationData> AutoCraftRequestOperations { get; set; } = new ObservableCollection<AutoCraftOperationData>();

        internal void Initialize()
        {
            AutoCraftRequestOperations.CollectionChanged += AutoCraftRequestOperations_CollectionChanged;
            TransferRequestOperations.CollectionChanged += TransferRequestOperations_CollectionChanged;
        }

        internal void Destroy()
        {
            AutoCraftRequestOperations.CollectionChanged -= AutoCraftRequestOperations_CollectionChanged;
            TransferRequestOperations.CollectionChanged -= TransferRequestOperations_CollectionChanged;
        }

        internal bool IsAutoCraftingAllowed { get; set; }

        internal void AddCraftingOperation(IMessageDialogSender sender, TechType techType, int amount)
        {
            
            var autoCrafterController = ((DSSAutoCrafterController) ConnectableDevice.GetController());
            var newRequest = new AutoCraftOperationData{AutoCraftRequestItem = techType,AutoCraftMaxAmount = amount};
            var result = autoCrafterController.AddCraftToQueue(newRequest,out string message);
            if (result)
            {
                AutoCraftRequestOperations.Add(newRequest);
            }
            else
            {
                QuickLogger.Debug(message,true);
                sender.ShowMessageBox(message);
            }
        }

        internal void AddTransferOperation(IMessageDialogSender sender, TechType techType, int amount)
        {
            TransferRequestOperations.Add(new TransferOperationData{FromDeviceId = ConnectableDevice.UnitID,  TransferRequestItem = techType, TransferRequestAmount = amount});
        }

        private void AutoCraftRequestOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            QuickLogger.Debug($"Collection Has been Changed. Method: {e.Action}",true);
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Count > 0)
                {
                    QuickLogger.Debug($"Deqeued Item from {ConnectableDevice.UnitID}");
                    ((DSSAutoCrafterController)ConnectableDevice.GetController()).RemoveQueue((AutoCraftOperationData)e.OldItems[0]);
                }
            }
            
            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Remove) return;
            Manager?.UpdateBaseOperators();
            Manager?.UpdateBaseCrafters();
        }

        private void TransferRequestOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Remove) return;
            QuickLogger.Debug($"Reset Operators",true);
            Manager?.UpdateBaseOperators();
        }

        internal void Load(FCSConnectableDevice connectableDevice)
        {
            ConnectableDevice = connectableDevice;
        }
    }

    internal struct AutoCraftOperationData
    {
        [JsonProperty] internal TechType AutoCraftRequestItem { get; set; }
        [JsonProperty] internal int AutoCraftMaxAmount { get; set; }
        internal TechData TechData { get; set; }

        public static bool operator ==(AutoCraftOperationData per1, AutoCraftOperationData per2)
        {
            return per1.Equals(per2);
        }
        public static bool operator !=(AutoCraftOperationData per1, AutoCraftOperationData per2)
        {
            return !per1.Equals(per2);
        }

        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}
        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
    
    internal struct TransferOperationData
    {
        [JsonProperty] internal string FromDeviceId { get; set; }
        [JsonProperty] internal TechType TransferRequestItem { get; set; }
        [JsonProperty] internal int TransferRequestAmount { get; set; } 
    }
}