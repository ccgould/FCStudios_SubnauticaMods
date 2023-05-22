using FCS_AlterraHub.API;
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
    private FCSModItemSettings _settings;
    private string _modName;
    protected readonly string _classID;
    private readonly string _iconName;
    protected readonly string _friendlyName;
    private readonly string _prefabName;
    private protected GameObject _cachedPrefab;


    public string AssetsFolder { get; }

    protected FCSSpawnableModBase(string modName, string prefabName, string modDir, string classId, string friendlyName) : base(friendlyName)
    {
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();

        QuickLogger.Debug($"FCSSpawnable :{AssetsFolder} | {modName}");

        _modName = modName;
        _classID = $"{classId}_kit";
        _iconName = classId;
        _friendlyName = friendlyName;
        _prefabName = prefabName;
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();
        Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName, ModRegistrationService.GetModPackData(modName)?.GetBundleName(), modDir);
    }


    public void Register()
    {
        OnStartRegister?.Invoke();
        var prefab = new CustomPrefab(PrefabInfo);

        // Set our prefab to a clone of the Seamoth electrical defense module
        prefab.SetGameObject(GetGameObjectAsync);

        // register the coal to the game
        prefab.Register();

        OnFinishRegister?.Invoke();
    }
      

    public virtual void PatchSMLHelper()
    {
        _settings = FCSModsAPI.InternalAPI.GetModSettings(_modName, _classID);
        PrefabInfo = PrefabInfo.WithTechType(_classID, FriendlyName, _settings.Description);
        PrefabInfo.WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{_iconName}.png")))
        .WithFileName(_prefabName);
        Register();
    }


    public virtual IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        //if (_cachedPrefab != null)
        //{
        //    gameObject.Set(_cachedPrefab);
        //    yield break;
        //}

        var prefab = GameObject.Instantiate(Prefab);

        var placeTool = prefab.GetComponent<PlaceTool>();
        if(placeTool != null )
        {
            placeTool.allowedInBase = _settings.AllowedInBase;
            placeTool.allowedOnBase = _settings.AllowedOnBase;
            placeTool.allowedOnCeiling = _settings.AllowedOnCeiling;
            placeTool.allowedOnConstructable = _settings.AllowedOnConstructables;
            placeTool.allowedOnGround = _settings.AllowedOnGround;
            placeTool.allowedOnRigidBody = _settings.AllowOnRigidBody;
            placeTool.allowedOnWalls = _settings.AllowedOnWall;
            placeTool.allowedOutside = _settings.AllowedOutside;
            placeTool.rotationEnabled = _settings.RotationEnabled;
            placeTool.hasAnimations = _settings.HasAnimations;
            placeTool.hasBashAnimation = _settings.HasBashAnimation;
            placeTool.hasFirstUseAnimation = _settings.HasFirstAnimation;
            placeTool.drawTime = _settings.DrawTime;
            placeTool.dropTime = _settings.DropTime;
            placeTool.holsterTime = _settings.HolsterTime;
        }
               

        yield return ModifyPrefab(prefab);

        gameObject.Set(prefab);
        //_cachedPrefab = prefab;
        yield break;
    }

    /// <summary>
    /// Changes to the prefab can be applied here.
    /// </summary>
    /// <param name="prefab"></param>>
    /// <returns></returns>
    protected abstract IEnumerator ModifyPrefab(GameObject prefab);
}
