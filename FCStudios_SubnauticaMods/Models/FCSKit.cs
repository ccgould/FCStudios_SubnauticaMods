using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Helpers;
using Nautilus.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Models;

public class FCSKit : FCSSpawnableModBase
{
    private string _iconPath;

    public FCSKit(string classID,string friendlyName) : base(PluginInfo.PLUGIN_NAME, "MainConstructionKit", FileSystemHelper.ModDirLocation,$"{classID}_kit", $"{friendlyName} Kit")
    {
        //_iconPath = iconPath;        
    }

    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        var prefab = GameObject.Instantiate(_prefab);

        prefab.name = PrefabInfo.PrefabFileName;

        PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();

        prefabID.ClassId = this.ClassID;

        var rb = prefab.GetComponentInChildren<Rigidbody>();

        var forces = prefab.AddComponent<WorldForces>();
        forces.useRigidbody = rb;
        forces.handleGravity = true;
        forces.handleDrag = true;
        forces.aboveWaterGravity = 9.81f;
        forces.underwaterGravity = 1;
        forces.aboveWaterDrag = 0.1f;
        forces.underwaterDrag = 1;

        var techTag = prefab.EnsureComponent<TechTag>();
        techTag.type = TechType;

        var center = new Vector3(0f, 0.2518765f, 0f);
        var size = new Vector3(0.5021304f, 0.5062426f, 0.5044461f);

        GameObjectHelpers.AddConstructableBounds(prefab, size, center);


        ApplyKitComponents(prefab);
        return base.GetGameObjectAsync(gameObject);
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        yield break;
    }

    private void ApplyKitComponents(GameObject prefab)
    {
        // Set collider
        var collider = prefab.GetComponentInChildren<Collider>();

        // Make the object drop slowly in water
        var wf = prefab.EnsureComponent<WorldForces>();
        wf.underwaterGravity = 0;
        wf.underwaterDrag = 10f;
        wf.enabled = true;

        //// Set proper shaders (for crafting animation)
        //Shader marmosetUber = Shader.Find("MarmosetUBER");
        var renderer = prefab.GetComponentInChildren<Renderer>();
        //renderer.material.shader = marmosetUber;

        // Update sky applier
        var applier = prefab.EnsureComponent<SkyApplier>();
        applier.renderers = new Renderer[] { renderer };
        applier.anchorSky = Skies.Auto;

        // We can pick this item
        var pickupable = prefab.EnsureComponent<Pickupable>();
        pickupable.isPickupable = true;
        pickupable.randomizeRotationWhenDropped = true;

        //Allow this kit to be placed on surfaces in these situations
        var placeTool = prefab.EnsureComponent<PlaceTool>();
        placeTool.allowedInBase = true;
        placeTool.allowedOnBase = false;
        placeTool.allowedOnCeiling = false;
        placeTool.allowedOnConstructable = true;
        placeTool.allowedOnGround = true;
        placeTool.allowedOnRigidBody = true;
        placeTool.allowedOnWalls = false;
        placeTool.allowedOutside = true;
        placeTool.rotationEnabled = true;
        placeTool.enabled = true;
        placeTool.hasAnimations = false;
        placeTool.hasBashAnimation = false;
        placeTool.hasFirstUseAnimation = false;
        placeTool.mainCollider = collider;
        placeTool.pickupable = pickupable;
        placeTool.drawTime = 0.5f;
        placeTool.dropTime = 1;
        placeTool.holsterTime = 0.35f;
        prefab.GetComponentInChildren<Text>().text = FriendlyName;
    }
}
