using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    /// <summary>
    /// This class will be attached to all FCStudios mods and a means of registering the device to the system.
    /// This class should provide important information such as Power, Health, UnitID
    /// </summary>
    [RequireComponent(typeof(PrefabIdentifier))]
    [RequireComponent(typeof(TechTag))]
    public abstract class FcsDevice : MonoBehaviour, IProtoEventListener, IConstructable
    {
        /// <summary>
        /// Inter Process Communication used to communicate with devices
        /// </summary>
        public virtual Action<string> IPCMessage { get; set; }


        public virtual void Awake()
        {

        }

        /// <summary>
        /// Checks to see if the device can store this item and amount
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual bool CanBeStored(int amount, TechType techType)
        {
            return false;
        }

        /// <summary>
        /// The maximum amount of items allowed to transfer to the device through the transceiver
        /// </summary>
        public virtual int MaxItemAllowForTransfer { get; }

        /// <summary>
        /// Gets the storage of a unit if it is accesable.
        /// </summary>
        /// <returns><see cref="FCSStorage"/></returns>
        public virtual IFCSStorage GetStorage()
        {
            return null;
        }

        public virtual Vector3 GetPosition()
        {
            return transform.position;
        }

        protected ColorManager _colorManager;

        /// <summary>
        /// The package which this item belongs to
        /// </summary>
        public virtual string PackageId { get; set; }

        /// <summary>
        /// The prefab id of the base this device is connected to.
        /// </summary>
        public virtual string BaseId { get; set; }

        /// <summary>
        /// Boolean that represents if the device is visible to the network.
        /// </summary>
        public virtual bool IsVisible { get; } = false;

        /// <summary>
        /// Boolean that represents if the device is constructed and ready to operate
        /// </summary>
        public virtual bool IsConstructed { get; set; }

        /// <summary>
        /// Boolean that shows if the device has been full initialized.
        /// </summary>
        public virtual bool IsInitialized { get; set; } = false;

        /// <summary>
        /// The unit identifier that will be unique to this device like a prefab identifier.
        /// </summary>
        public virtual string UnitID { get; set; }

        /// <summary>
        /// Boolean that shows if this device takes power from the base or relay.
        /// </summary>
        public virtual bool DoesTakePower { get; set; } = true;

        /// <summary>
        /// States if the device is in an operational state.
        /// </summary>
        public virtual bool IsOperational { get; }

        /// <summary>
        /// Boolean that represents if the device can take damage.
        /// </summary>
        public virtual bool IsDamageable { get; set; } = false;

        //The base manager for this device
        public BaseManager Manager { get; set; }

        /// <summary>
        /// Bypasses the check for the registration IsConstructed and PrefabID
        /// </summary>
        public virtual bool BypassRegisterCheck { get; } = false;

        /// <summary>
        /// The initializer of this device
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// The prefabID of this device
        /// </summary>
        /// <returns></returns>
        public virtual string GetPrefabID()
        {
            return gameObject.GetComponent<PrefabIdentifier>()?.Id ??
                   gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
        }

        /// <summary>
        /// Gets the TechType of this device
        /// </summary>
        /// <returns></returns>
        public TechType GetTechType()
        {
            return gameObject.GetComponent<TechTag>()?.type ??
                   gameObject.GetComponentInChildren<TechTag>()?.type ??
                   TechType.None;
        }

        public abstract void OnProtoSerialize(ProtobufSerializer serializer);

        public abstract void OnProtoDeserialize(ProtobufSerializer serializer);

        public abstract bool CanDeconstruct(out string reason);

        public abstract void OnConstructedChanged(bool constructed);

        /// <summary>
        /// Changes the body color of the device
        /// </summary>
        /// <param name="color"></param>
        public virtual bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            QuickLogger.ModMessage("I don't have any color changing abilities");
            return false;
        }

        /// <summary>
        /// Gets the body color of the device
        /// </summary>
        /// <param name="color"></param>
        public virtual void GetBodyColor(Color color, ColorTargetMode mode)
        {
            QuickLogger.Debug("This device hasnt set this method");
        }

        /// <summary>
        /// The amount of power this device pulls
        /// </summary>
        /// <returns></returns>
        public virtual float GetPowerUsage()
        {
            return 0f;
        }

        /// <summary>
        /// The amount of power this device produces
        /// </summary>
        /// <returns></returns>
        public virtual float GetPowerProducing()
        {
            return 0f;
        }

        /// <summary>
        /// Gets the amount of power stored within the device
        /// </summary>
        /// <returns></returns>
        public virtual float GetStoredPower()
        {
            return 0f;
        }

        /// <summary>
        /// Returns the max amount of power this device can hold
        /// </summary>
        /// <returns></returns>
        public virtual float GetMaxPower()
        {
            return 0f;
        }

        public virtual void OnDestroy()
        {
            Manager?.UnRegisterDevice(this);
            FCSAlterraHubService.PublicAPI.UnRegisterDevice(this);
        }

        /// <summary>
        /// Refreshes the UI of the device if implemented
        /// </summary>
        public virtual void RefreshUI()
        {

        }

        /// <summary>
        /// Adds items to the device from external sources.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool AddItemToContainer(InventoryItem item)
        {
            return false;
        }

        /// <summary>
        /// Remove items from the device from external sources.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool RemoveItemFromContainer(InventoryItem item)
        {
            return false;
        }


        public virtual bool IsUnderWater()
        {
            return transform.position.y < -1f;
        }

        /// <summary>
        /// Remove items from the device from external sources.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual Pickupable RemoveItemFromContainer(TechType techType)
        {
            return null;
        }

        /// <summary>
        /// Gets the current color of the device
        /// </summary>
        /// <returns></returns>
        public virtual bool CurrentDeviceColor(ColorTargetMode mode, out Color color)
        {
            if (_colorManager != null)
            {
                color = _colorManager.GetColor(mode);
                return true;
            }
            color = Color.white;
            return false;
        }

        /// <summary>
        /// Returns the colorManager of this device
        /// </summary>
        /// <returns></returns>
        public virtual ColorManager GetColorManager()
        {
            return _colorManager;
        }

        /// <summary>
        /// Turns On the device if the device can be turned on
        /// </summary>
        public virtual void TurnOnDevice()
        {
        }

        /// <summary>
        /// Turns off the the device if the device can be turned off
        /// </summary>
        public virtual void TurnOffDevice()
        {

        }

        public virtual bool IsRack { get; } = false;
        public Action<FcsDevice, InventoryItem> OnAddItem { get; set; }
        public Action<FcsDevice, InventoryItem> OnRemoveItem { get; set; }

        public string TabID { get; set; }
        public virtual StorageType StorageType { get; }
        public virtual bool CanBeSeenByTransceiver { get; set; }
        public virtual TechType[] AllowedTransferItems { get; }

        public virtual int GetItemCount(TechType techType)
        {
            return 0;
        }

        public virtual IEnumerable<KeyValuePair<TechType, int>> GetItemsWithin()
        {
            return null;
        }

        public virtual int GetMaxStorage()
        {
            return 0;
        }

        public virtual Pickupable RemoveItemFromDevice(TechType techType)
        {
            return null;
        }

        public virtual Pickupable GetRandomPickupableFromDevice()
        {
            return null;
        }

        public virtual TechType GetRandomTechTypeFromDevice()
        {
            return TechType.None;
        }

        public virtual bool AllowsTransceiverPulling { get; }
    }

    public interface IFCSSave<T>
    {
        void Save(T newSaveData, ProtobufSerializer serializer = null);
    }
}
