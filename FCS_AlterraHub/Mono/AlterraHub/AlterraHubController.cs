using System;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Converters;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class AlterraHubController : FcsDevice
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private AlterraHubDataEntry _savedData;


        internal HubTrigger AlterraHubTrigger { get; set; }
        internal AlterraHubDisplay DisplayManager { get; private set; }
        public MotorHandler MotorHandler { get; private set; }

        #region Unity Methods

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    DisplayManager.Load(_savedData);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {
            FCSAlterraHubService.PublicAPI.RemoveDeviceFromGlobal(GetPrefabID());
        }

        #endregion
        
        public override void Initialize()
        {
            if(IsInitialized) return;

            if (AlterraHubTrigger == null)
            {
                AlterraHubTrigger = GameObjectHelpers.FindGameObject(gameObject, "Trigger").AddComponent<HubTrigger>();
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<AlterraHubDisplay>();
                DisplayManager.Setup(this);
            }

            if (MotorHandler == null)
            {
                MotorHandler = GameObjectHelpers.FindGameObject(gameObject, "RoundSignDisplay01").AddComponent<MotorHandler>();
                MotorHandler.Initialize(30);
                //TODO Control motor based off power handler
                MotorHandler.Start();
            }

            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.ModID);

            LoadStore();

            var inputField = gameObject.GetComponentsInChildren<InputField>(true);
            if (inputField != null && inputField.Length > 0)
            {
                foreach (InputField field in inputField)
                {
                    field.gameObject.AddComponent<CustomInputField>();
                }
            }

            IsInitialized = true;
        }
        
        private void onBuyBTNClick(CartDropDownHandler dropDownHandler)
        {
            //TODO Show CheckOut
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        internal bool MakeAPurchase(CartDropDownHandler cart,bool giveToPlayer = true)
        {
            if (giveToPlayer)
            {
                var totalCash = cart.GetTotal();
                if (CardSystem.main.HasEnough(totalCash))
                {
                    CardSystem.main.RemoveFinances(totalCash);
                    foreach (CartItem item in cart.GetItems())
                    {
                        var invType = item.ReceiveTechType.ToInventoryItem();
                        PlayerInteractionHelper.GivePlayerItem(invType);
                    }
                }

                return true;
            }
            else
            {
                //TODO send to DSS
            }

            return false;
        }

        internal void ReturnItem(string cardNumber, InventoryItem item)
        {
            var result = StoreInventorySystem.ItemReturn(cardNumber, item);
            
            if (result)
            {
                //TODO Send message about transaction completed
                Destroy(item.item.gameObject);
            }
        }

        internal string GetAccountAmount(string cardNumber)
        {
            var amount = CardSystem.main.GetAccountBalance(cardNumber);
            return Converters.FloatToMoney("C",amount);
        }

        private void LoadStore()
        {
            var panelGroup = DisplayManager.GetPanelGroup();
            
            foreach (PanelHelper panelHelper in panelGroup.PanelHelpers)
            {
                foreach (var storeItem in FCSAlterraHubService.PublicAPI.GetRegisteredKits())
                {
                    if (panelHelper.StoreCategory == storeItem.Value.StoreCategory)
                    {
                        var item = storeItem.Value;
                        StoreInventorySystem.AddNewStoreItem(item.TechType,item.Cost);
                        panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(item.TechType, item.ReceiveTechType, item.StoreCategory, item.Cost, AddToCardCallBack));
                    }
                }

                foreach (CustomStoreItem customStoreItem in QPatch.Configuration.AdditionalStoreItems)
                {
                    if (panelHelper.StoreCategory == customStoreItem.Category)
                    {
                        QuickLogger.Info($"Item: {customStoreItem.TechType} || Category: {customStoreItem.Category} || Cost: {customStoreItem.Cost}");
                        StoreInventorySystem.AddNewStoreItem(customStoreItem.TechType, customStoreItem.Cost);
                        panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(customStoreItem.TechType, customStoreItem.ReturnItemTechType, customStoreItem.Category, customStoreItem.Cost, AddToCardCallBack));
                    }
                }

            }
        }

        private void AddToCardCallBack(TechType techType,TechType receiveTechType)
        {
            DisplayManager.onItemAddedToCart?.Invoke(techType, receiveTechType);
        }

        public void Save(SaveData newSaveData)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraHubDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.CartItems = DisplayManager.SaveCartItems();
            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);

            //_savedData.BodyColor = ColorManager.GetMaskColor().ColorToVector4();
            newSaveData.AlterraHubEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetAlterraHubSaveData(GetPrefabID());
        }

        public bool IsPlayerInRange()
        {
            return AlterraHubTrigger.IsPlayerInRange;
        }
    }

    internal class HubTrigger : MonoBehaviour
    {
        private AlterraHubController _mono;
        internal bool IsPlayerInRange;


        internal Action<bool> onTriggered { get; set; }

        internal void Initialize(AlterraHubController mono)
        {
            _mono = mono;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = true;
            onTriggered?.Invoke(true);

        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.layer != 19 || IsPlayerInRange) return;
            onTriggered?.Invoke(true);
            IsPlayerInRange = true;
        }


        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = false;
            onTriggered?.Invoke(false);
            
        }
    }
}
