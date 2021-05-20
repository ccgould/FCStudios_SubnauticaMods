using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class AlterraHubController
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private AlterraHubDataEntry _savedData;
        private bool _cursorLockCached;
        private GameObject _inputDummy;
        private bool _isInRange;
        private GameObject _screenBlock;
        private DumpContainerSimplified _dumpContainer;
        private MotorHandler _motorHandler;
        private GameObject _playerBody;
        private bool _isInUse;
        private Transform _cameraParent;

        internal AlterraHubDisplay DisplayManager { get; private set; }
        private MessageBoxHandler messageBoxHandler => MessageBoxHandler.main;


        #region Unity Methods

        private void IpcMessage(string message)
        {
           if (message.Equals("ActivateGoal"))
           {
               var panelGroup = DisplayManager.GetPanelGroup();
               foreach (PanelHelper panelHelper in panelGroup.PanelHelpers)
               {
                   panelHelper.ActivateStoreItem(BaseManager.ActivateGoalTechType);
               }
           }

           if (message.Equals("ErrorLoadingAccount"))
           {
               MessageBoxHandler.main.Show(Buildables.AlterraHub.ErrorLoadingAccount(), FCSMessageButton.OK);
           }
        }

        #endregion


        public  void Initialize()
        {
           if(IsInitialized) return;

           MessageBoxHandler.main.ObjectRoot = gameObject;

           if (_dumpContainer == null)
           {
               _dumpContainer = gameObject.AddComponent<DumpContainerSimplified>();
               _dumpContainer.Initialize(transform, "Item Return", this);
           }

           if (DisplayManager == null)
           {
               DisplayManager = gameObject.AddComponent<AlterraHubDisplay>();
               DisplayManager.Setup(this);
               DisplayManager.OnReturnButtonClicked += () =>
               {
                   _dumpContainer.OpenStorage();
               };
           }

           if (_motorHandler == null)
           {
               _motorHandler = GameObjectHelpers.FindGameObject(gameObject, "RoundSignDisplay01").AddComponent<MotorHandler>();
               _motorHandler.Initialize(30);
               _motorHandler.StartMotor();
           }

           if (_colorManager == null)
           {
               _colorManager = gameObject.AddComponent<ColorManager>();
               _colorManager.Initialize(gameObject, Buildables.AlterraHub.BodyMaterial);
           }
            
           _screenBlock = GameObjectHelpers.FindGameObject(gameObject, "Blocker");

           LoadStore();

           InGameMenuQuitPatcher.AddEventHandlerIfMissing(OnQuit);


           IsInitialized = true;
        }

        private void OnQuit()
        {
           Mod.DeepCopySave(CardSystem.main.SaveDetails());
           QuickLogger.Debug("Quitting Purging CardSystem and AlterraHubSave",true);
           CardSystem.main.Purge();
           Mod.PurgeSave();
        }

        public FCSHUD HUD { get; set; }

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
                       for (int i = 0; i < item.ReturnAmount; i++)
                       {
                           QuickLogger.Debug($"{item.ReceiveTechType}", true);
                           PlayerInteractionHelper.GivePlayerItem(item.ReceiveTechType);
                       }
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

        private void LoadStore()
        {
           var panelGroup = DisplayManager.GetPanelGroup();
            
           foreach (PanelHelper panelHelper in panelGroup.PanelHelpers)
           {
               QuickLogger.Debug($"Loading Panel: {panelHelper.StoreCategory}:");
               foreach (var storeItem in FCSAlterraHubService.PublicAPI.GetRegisteredKits())
               {
                   QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.Key)} to Panel: {panelHelper.StoreCategory}:");

                   if (panelHelper.StoreCategory == storeItem.Value.StoreCategory)
                   {
                       StoreInventorySystem.AddNewStoreItem(storeItem.Value);
                       panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(storeItem.Value, AddToCardCallBack,IsInUse));
                       QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.Key)} to Panel: {panelHelper.StoreCategory}:");
                   }
               }

               foreach (FCSStoreEntry storeItem in QPatch.Configuration.AdditionalStoreItems)
               {
                   if (panelHelper.StoreCategory == storeItem.StoreCategory)
                   {
                       QuickLogger.Info($"Item: {storeItem.TechType} || Category: {storeItem.StoreCategory} || Cost: {storeItem.Cost}");
                       StoreInventorySystem.AddNewStoreItem(storeItem);
                       panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(storeItem, AddToCardCallBack, IsInUse));
                   }
               }
           }
        }

        private bool IsInUse()
        {
           return _isInUse;
        }

        private void AddToCardCallBack(TechType techType,TechType receiveTechType,int returnAmount)
        {
           DisplayManager.onItemAddedToCart?.Invoke(techType, receiveTechType,returnAmount);
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
           CardSystem.main.AddFinances(StoreInventorySystem.GetPrice(item.item.GetTechType(),true));
           Destroy(item.item.gameObject);
           return true;
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
           return StoreInventorySystem.StoreHasItem(techType,true);
        }

        public bool IsAllowedToAdd(Pickupable inventoryItem, bool verbose)
        {
           return false;
        }
    }
}
