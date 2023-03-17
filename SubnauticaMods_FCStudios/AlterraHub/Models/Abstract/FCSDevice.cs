using FCS_AlterraHub.API;
using SMLHelper.Assets;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract
{
    /// <summary>
    /// This class will be attached to all FCStudios mods and a means of registering the device to the system.
    /// This class should provide important information such as Power, Health, UnitID
    /// </summary>
    [RequireComponent(typeof(PrefabIdentifier))]
    [RequireComponent(typeof(TechTag))]
    public abstract class FCSDevice : MonoBehaviour, IProtoEventListener, IConstructable
    {
        private Constructable buildable;
        protected bool _runStartUpOnEnable;

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
        public virtual string UnitID { get; set; } = string.Empty;

        /// <summary>
        /// The firendly name of this device.
        /// </summary>
        public virtual string FriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// The initializer of this device
        /// </summary>
        public virtual void Initialize() { }


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

        /// <summary>
        /// The prefabID of this device
        /// </summary>
        /// <returns></returns>
        public virtual string GetPrefabID()
        {
            return gameObject.GetComponent<PrefabIdentifier>()?.Id ??
                   gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
        }

        public virtual Vector3 GetPosition()
        {
            return transform.position;
        }

        public Constructable Buildable
        {
            get
            {
                if (buildable == null)
                {
                    buildable = GetComponentInParent<Constructable>() ?? GetComponent<Constructable>();
                }

                return buildable;
            }
        }

        /// <summary>
        /// If true allows this device to be seen in the Base devices list in the FCSPDA"/>
        /// </summary>
        public bool IsVisibleInPDA;

        public abstract bool IsDeconstructionObstacle();

        public abstract void OnConstructedChanged(bool constructed);

        public abstract void OnProtoDeserialize(ProtobufSerializer serializer);

        public abstract void OnProtoSerialize(ProtobufSerializer serializer);

        public abstract bool CanDeconstruct(out string reason);

        public string GetDeviceName()
        {
            return FriendlyName.Equals(string.Empty) ? UnitID : FriendlyName;
        }

        public virtual void Start() 
        {
            FCSModsAPI.PublicAPI.RegisterDevice(this, GetTechType());
        }
    }
}
