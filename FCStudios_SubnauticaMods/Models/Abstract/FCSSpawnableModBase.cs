using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using FCSCommon.Utilities;
using Nautilus.Assets;
using Nautilus.Utility;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract;

public abstract class FCSSpawnableModBase : ModBase,IModBase
{
    protected FCSModItemSettings _settings;
    private string _modName;
    protected readonly string _classID;
    protected readonly string _classIDWithOutKit;
    private readonly string _iconName;
    protected readonly string _friendlyName;
    private readonly string _prefabName;
    private protected GameObject _cachedPrefab;
    protected CustomPrefab _customPrefab;

    public string AssetsFolder { get; }

    protected FCSSpawnableModBase(string modName, string prefabName, string modDir, string classId, string friendlyName,bool isKit = false) : base(friendlyName)
    {
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();

        QuickLogger.Debug($"FCSSpawnable :{AssetsFolder} | {modName}");

        _modName = modName;
        var postFix = isKit ? "_kit" : string.Empty;
        _classID = $"{classId}{postFix}";
        _iconName = classId;
        _classIDWithOutKit = classId;
        _friendlyName = friendlyName;
        _prefabName = prefabName;
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();
        Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName, ModRegistrationService.GetModPackData(modName)?.GetBundleName(), modDir);
    }


    public void Register()
    {

        _customPrefab = new CustomPrefab(PrefabInfo);

        OnStartRegister?.Invoke();

        // Set our prefab to a clone of the Seamoth electrical defense module
        _customPrefab.SetGameObject(GetGameObjectAsync);

        // register the coal to the game
        _customPrefab.Register();

        OnFinishRegister?.Invoke();
    }
      

    public virtual void PatchSMLHelper()
    {
        _settings = FCSModsAPI.InternalAPI.GetModSettings(_modName, _classIDWithOutKit);
        PrefabInfo = PrefabInfo.WithTechType(_classID, FriendlyName, _settings.Description);
        PrefabInfo.WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{_iconName}.png")))
        .WithFileName(_prefabName);
        Register();
    }


    public virtual IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        var prefab = GameObject.Instantiate(Prefab);

        if (_settings.HasGlass)
        {
            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Plugin.ModSettings.ModPackID);
        }

        yield return ModifyPrefab(prefab);

        gameObject.Set(prefab);

        yield break;
    }

    /// <summary>
    /// Changes to the prefab can be applied here.
    /// </summary>
    /// <param name="prefab"></param>>
    /// <returns></returns>
    protected abstract IEnumerator ModifyPrefab(GameObject prefab);
}
