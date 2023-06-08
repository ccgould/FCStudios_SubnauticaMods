using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
using RootMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FCS_AlterraHub.Models.Mono.HabitatManager;

namespace FCS_AlterraHub.Models.Abstract;

/// <summary>
/// This class will be attached to all FCStudios mods and a means of registering the device to the system.
/// This class should provide important information such as Power, Health, UnitID
/// </summary>
[RequireComponent(typeof(PrefabIdentifier))]
[RequireComponent(typeof(TechTag))]
[RequireComponent(typeof(HoverInteraction))]
[RequireComponent(typeof(Constructable))]
[RequireComponent(typeof(LargeWorldEntity))]
public abstract class FCSDevice : MonoBehaviour, IProtoEventListener, IConstructable,IPowerConsumer
{
    
    private Constructable buildable;
    protected bool _runStartUpOnEnable;
    protected bool IsFromSave;
    protected object _savedData;
    private Dictionary<string, DeviceWarning> _warnings = new();
    [SerializeField]
    [Range(0f,1f)]
    protected float energyPerSecond = 0f;

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
    public virtual void Initialize() 
    {
        IsInitialized = true;
    }

    public virtual void Awake() 
    { 
        _colorManager = gameObject.GetComponent<ColorManager>();
        _colorManager?.Initialize(gameObject);
    }

    public virtual void OnEnable()
    {
        
    }

    public abstract void ReadySaveData();

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
        if(string.IsNullOrEmpty(_prefabID))
        {
            _prefabID = gameObject.GetComponent<PrefabIdentifier>()?.Id ??
               gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
        }

        return _prefabID;
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
    public bool IsVisibleInPDA = true;

    public virtual bool BypassConnection { get; } = false;

    protected ColorManager _colorManager;
    private string _prefabID;

    public abstract bool IsDeconstructionObstacle();

    public virtual void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;
        if (constructed)
        {
            if (base.isActiveAndEnabled)
            {
                if (!this.IsInitialized)
                {
                    this.Initialize();
                }

                return;
            }
            _runStartUpOnEnable = true;
        }
    }

    public abstract void OnProtoDeserialize(ProtobufSerializer serializer);

    public abstract void OnProtoSerialize(ProtobufSerializer serializer);

    public abstract bool CanDeconstruct(out string reason);

    public string GetDeviceName()
    {
        return FriendlyName.Equals(string.Empty) ? UnitID : FriendlyName;
    }

    public virtual void Start() 
    {
        FCSModsAPI.PublicAPI.RegisterDevice(this);
    }

    public virtual void OnDestroy()
    {
        FCSModsAPI.PublicAPI.UnRegisterDevice(this);
    }

    /// <summary>
    /// Used for the HoverInteration to display infromation about the device
    /// </summary>
    /// <returns></returns>
    public virtual string[] GetDeviceStats()
    {
        if (!IsOperational())
        {
            return new string[]
            {
                LanguageService.NotConnectedToBaseManager(),
            };
        }

        return null;
    }

    public HabitatManager CachedHabitatManager { get; private set; }

    public virtual bool IsConnectedToBaseManager()
    {
        if(CachedHabitatManager is not null)
        {
            return CachedHabitatManager.HasDevice(BaseManagerBuildable.PatchedTechType);
        }

        if (IsConstructed && FCSModsAPI.PublicAPI.IsRegisteredInBase(GetPrefabID(), out HabitatManager manager))
        {
            if (manager is not null && manager.HasDevice(BaseManagerBuildable.PatchedTechType))
            {
                CachedHabitatManager = manager;
                return true;
            }
        }

        return false;
    }

    public virtual bool IsOperational()
    {
        

        return IsConnectedToBaseManager() && IsConstructed;
    }

    public virtual float GetPowerUsage()
    {
        return 0;
    }

    protected void AddWarning(string warningID, string description, WarningType warningType, FaultType faultType)
    {
        if (_warnings.ContainsKey(warningID)) return;

        _warnings.Add(warningID, new DeviceWarning(GetPrefabID(), warningID, description, warningType, faultType));
    }

    public int GetWarningsCount(FaultType faultType)
    { 
        return _warnings.Count(x=>x.Value.FaultType == faultType); 
    }
}
