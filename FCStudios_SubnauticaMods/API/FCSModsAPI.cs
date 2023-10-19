using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FCS_AlterraHub.API;


public interface IFCSModsAPIPublic
{
    /// <summary>
    /// Adds a mod pack into the FCStudios mod pack to be used in Subnautica
    /// </summary>
    /// <param name="modName">Name of the mod to add</param>
    /// <param name="assembly"><see cref="Assembly"/> to which the mod resides</param>
    /// <param name="assetBundleName">The name of the mod bundle if any</param>
    void RegisterModPack(string modName, Assembly assembly, string assetBundleName, Action saveCallBack, Action loadCallBack);
    /// <summary>
    /// Adds a mod to the mod pack
    /// </summary>
    /// <param name="modPackName">Name of the mod pack to add the mod to.</param>
    /// <param name="modID">The ID of the mod. Will be used in the default name e.g DD001</param>
    /// <param name="mod"><see cref="Spawnable"/> class of the mod.</param>
    void RegisterMod(string modPackName, string modID, ModBase mod);
    bool IsInOreBuildMode();
    string GetModBundleName(string modName, string classID);
    Dictionary<TechType, FCSStoreEntry> GetRegisteredKits();
    void RegisterDevice(FCSDevice fCSDevice);
    void UnRegisterDevice(FCSDevice fCSDevice);
    void CreateStoreEntry(TechType parentTechType, TechType receiveTechType,int returnAmount, decimal cost, StoreCategory energy);
    string GetModID(TechType techType);
    HashSet<FCSDevice> GetRegisteredDevices();
    HabitatManager GetHabitat(FCSDevice device);
    string GetModPackID(TechType techType);
    void AddStoreCategory(string pLUGIN_GUID, string iconName, string pageName, PDAPages pdaPage);
    bool IsRegisteredInBase(string prefabID, out HabitatManager manager);
    HabitatManager GetPlayerHabitat();
}
public interface IFCSModsAPIInternal
{
    void AddPingType(string pingName, PingType pingType);
    FCSModItemSettings GetModSettings(string modName,string classID);
}


/// <summary>
/// An API for FCS devices
/// </summary>
public class FCSModsAPI : IFCSModsAPIPublic, IFCSModsAPIInternal
{
    private static readonly FCSModsAPI singleton = new();

    internal static IFCSModsAPIInternal InternalAPI => singleton;
    public static IFCSModsAPIPublic PublicAPI => singleton;
    
    public void RegisterModPack(string modName, Assembly assembly, string assetBundleName,Action saveCallBack, Action loadCallBack) => ModRegistrationService.Register(modName, assembly,assetBundleName,saveCallBack,loadCallBack);
    
    public void RegisterMod(string modPackName,string modID, ModBase mod) => ModRegistrationService.RegisterMod(modPackName, modID, mod);

    public bool IsInOreBuildMode() => Plugin.Configuration.OreBuildMode;

    public void AddPingType(string pingName, PingType pingType) => PingTypeService.AddPingType(pingName,pingType);

    /// <summary>
    /// Reads the for the provided class id
    /// </summary>
    /// <param name="classID">The class id of the mod you are trying to find the settings for</param>
    /// <returns></returns>
    public FCSModItemSettings GetModSettings(string modName,string classID)
    {
        try
        {
            QuickLogger.Info($"Attempting to load  mod settings for object with ID {classID}.");
            
            var listOfModSettings = ModRegistrationService.GetModPackData(modName).GetSettings();

            if (listOfModSettings is not null)
            {
                QuickLogger.Info("Mod Settings were successfully loaded!");

                if(listOfModSettings is null) { throw new NullReferenceException($"Mod Settings Not Found {classID}"); }

                if (!listOfModSettings.ContainsKey(classID)) { throw new KeyNotFoundException($"Mod Class ID Not Found {classID}"); }


                return listOfModSettings[classID];
            }
        }
        catch (Exception ex)
        {
            QuickLogger.Error(ex.Message);
            QuickLogger.Error(ex.StackTrace);
        }

        return new FCSModItemSettings();
    }

    /// <summary>
    /// Gets the Bunbdle name for this mod.
    /// </summary>
    /// <param name="classID">The class id of the mod you are trying to find the settings for</param>
    /// <returns></returns>
    public string GetModBundleName(string modName, string classID)
    {
        try
        {
            return ModRegistrationService.GetModPackData(modName).GetBundleName();
        }
        catch (System.Exception ex)
        {
            QuickLogger.Error(ex.Message);
            QuickLogger.Error(ex.StackTrace);
        }

        return string.Empty;
    }

    /// <summary>
    /// A Dictionary of all the kits created for the store as <see cref="FCSStoreEntry"/>.
    /// </summary>
    /// <returns>A Dictionary of <see cref="FCSStoreEntry"/></returns>
    public Dictionary<TechType, FCSStoreEntry> GetRegisteredKits()
    {
        return StoreInventoryService.GetRegisteredKits();
    }

    public void RegisterDevice(FCSDevice fcsDevice)
    {
        HabitatService.main.RegisterDevice(fcsDevice);
    }

    public void UnRegisterDevice(FCSDevice fcsDevice)
    {
        HabitatService.main.UnRegisterDevice(fcsDevice);
    }

    public void CreateStoreEntry(TechType parentTechType, TechType receiveTechType, int returnAmount, decimal cost, StoreCategory category)
    {
        StoreInventoryService.CreateStoreEntry(parentTechType, receiveTechType, returnAmount, cost, category);
    }

    public string GetModID(TechType techType)
    {
        return ModRegistrationService.GetModID(techType);
    }

    public HashSet<FCSDevice> GetRegisteredDevices()
    {
        return HabitatService.main.GetRegisteredDevices();
    }

    public HabitatManager GetHabitat(FCSDevice device)
    {
        return HabitatService.main.GetHabitat(device);
    }

    public string GetModPackID(TechType techType)
    {
        return ModRegistrationService.GetModPackID(techType);
    }

    public void AddStoreCategory(string pLUGIN_GUID, string iconName, string pageName, PDAPages pdaPage) => StoreManager.RegisterStoreMod(pLUGIN_GUID, iconName, pageName, pdaPage);

    public bool IsRegisteredInBase(string prefabID, out HabitatManager manager)
    {
        manager = null;

        if(HabitatService.main.IsRegisteredInBase(prefabID, out HabitatManager result))
        {
            manager = result;
            return true;
        }
        return false;
    }

    public HabitatManager GetPlayerHabitat()
    {
        return HabitatService.main.GetPlayersCurrentBase();
    }
}
