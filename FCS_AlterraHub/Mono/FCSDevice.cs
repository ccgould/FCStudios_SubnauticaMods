using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    /// <summary>
    /// This class will be attached to all FCStudios mods ad a means of registering the device to the system.
    /// This class should provide important information such as Power, Health, UnitID
    /// </summary>
    [RequireComponent(typeof(PrefabIdentifier))]
    [RequireComponent(typeof(TechTag))]
    public abstract class FcsDevice : MonoBehaviour, IProtoEventListener, IConstructable
    {

        /// <summary>
        /// Boolean that represents if the device is visible to the network.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Boolean that represents if the device is constructed and ready to operate
        /// </summary>
        public bool IsConstructed { get; set; }

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
        /// Boolean that represents if the device can take damage.
        /// </summary>
        public virtual bool IsDamageable { get; set; } = false;

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
        public virtual void ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            QuickLogger.ModMessage("I don't have any color changing abilities");
        }

        /// <summary>
        /// Gets the body color of the device
        /// </summary>
        /// <param name="color"></param>
        public virtual void GetBodyColor(Color color, ColorTargetMode mode)
        {
            QuickLogger.Debug("This device hasnt set this method");
        }
    }

    public interface IFCSSave<T>
    {
        void Save(T newSaveData);
    }
}
