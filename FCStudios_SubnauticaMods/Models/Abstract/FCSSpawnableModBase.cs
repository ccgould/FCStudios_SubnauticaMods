using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using FCSCommon.Utilities;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract;

public abstract class FCSSpawnableModBase : ModBase,IModBase
{
    protected GameObject _prefab;
    private FCSModItemSettings _settings;
    private string _modName;

    public string AssetsFolder { get; }

    protected FCSSpawnableModBase(string modName, string prefabName, string modDir, string classId, string friendlyName) : base(friendlyName)
    {
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();

        QuickLogger.Debug($"FCSSpawnable :{AssetsFolder} | {modName}");

        _modName = modName;
        _info = PrefabInfo.WithTechType(classId, friendlyName, "Coal that makes me go yes.");
        _info.WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{classId}.png")));

        _classID = classId;
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();
        _prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName, ModRegistrationService.GetModPackData(modName)?.GetBundleName(), modDir);
    }

    // To access the TechType anywhere in the project
    public static PrefabInfo _info { get; private set; }

    private string _classID;
    private GameObject _cachedPrefab;

    public void Register()
    {
        var prefab = new CustomPrefab(_info);

        // Set our prefab to a clone of the Seamoth electrical defense module
        prefab.SetGameObject(GetGameObjectAsync);



        //prefab.SetRecipe(GetRecipe());

        // register the coal to the game
        prefab.Register();
    }

    

    public virtual void PatchSMLHelper()
    {
        _settings = FCSModsAPI.InternalAPI.GetModSettings(_modName, _classID);
        Register();
    }


    public virtual IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        if (_cachedPrefab != null)
        {
            gameObject.Set(_cachedPrefab);
            yield break;
        }

        var prefab = GameObject.Instantiate(_prefab);

        var pickUp = prefab.AddComponent<Pickupable>();
        pickUp.randomizeRotationWhenDropped = true;
        pickUp.isPickupable = true;

        var rigidBody = prefab.EnsureComponent<Rigidbody>();

        // Make the object drop slowly in water
        var wf = prefab.AddComponent<WorldForces>();
        wf.underwaterGravity = 0;
        wf.underwaterDrag = 10f;
        wf.enabled = true;
        wf.useRigidbody = rigidBody;

        // Set collider
        var collider = prefab.GetComponent<BoxCollider>();

        var placeTool = prefab.AddComponent<PlaceTool>();
        placeTool.allowedInBase = _settings.AllowedInBase;
        placeTool.allowedOnBase = _settings.AllowedOnBase;
        placeTool.allowedOnCeiling = _settings.AllowedOnCeiling;
        placeTool.allowedOnConstructable = _settings.AllowedOnConstructables;
        placeTool.allowedOnGround = _settings.AllowedOnGround;
        placeTool.allowedOnRigidBody = _settings.AllowOnRigidBody;
        placeTool.allowedOnWalls = _settings.AllowedOnWall;
        placeTool.allowedOutside = _settings.AllowedOutside;
        placeTool.rotationEnabled = _settings.RotationEnabled;
        placeTool.enabled = true;
        placeTool.hasAnimations = _settings.HasAnimations;
        placeTool.hasBashAnimation = _settings.HasBashAnimation;
        placeTool.hasFirstUseAnimation = _settings.HasFirstAnimation;
        placeTool.mainCollider = collider;
        placeTool.pickupable = pickUp;
        placeTool.drawTime = _settings.DrawTime;
        placeTool.dropTime = _settings.DropTime;
        placeTool.holsterTime = _settings.HolsterTime;

        //Renderer
        var renderer = prefab.GetComponentInChildren<Renderer>();

        // Update sky applier
        var applier = prefab.GetComponent<SkyApplier>();
        if (applier == null)
            applier = prefab.AddComponent<SkyApplier>();
        applier.renderers = new Renderer[] { renderer };
        applier.anchorSky = Skies.Auto;


        yield return ModifyPrefab(prefab);

        gameObject.Set(prefab);
        _cachedPrefab = prefab;
        yield break;
    }

    /// <summary>
    /// Changes to the prefab can be applied here.
    /// </summary>
    /// <param name="prefab"></param>>
    /// <returns></returns>
    protected abstract IEnumerator ModifyPrefab(GameObject prefab);
}
