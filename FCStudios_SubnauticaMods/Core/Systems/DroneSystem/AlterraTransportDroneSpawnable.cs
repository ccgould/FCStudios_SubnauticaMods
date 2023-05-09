using System;
using System.IO;
using FCS_AlterraHub.Core.Helpers;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_AlterraHub.Core.Systems.DroneSystem;

internal class AlterraTransportDroneSpawnable : Spawnable
{
    private Color _color;
    public override string AssetsFolder => Mod.GetAssetPath();

    public AlterraTransportDroneSpawnable() : base("AlterraTransportDrone", "Alterra Transport Drone", "Alterra Transport Drone for transpoarting your alterra kits")
    {
        OnFinishedPatching += () => { Mod.AlterraTransportDroneTechType = TechType; };
    }

    public override GameObject GetGameObject()
    {
        try
        {
            var prefab = UnityEngine.Object.Instantiate(AlterraHub.AlterraHubTransportDronePrefab);

            prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
            prefab.AddComponent<TechTag>().type = TechType;

            var pickUp = prefab.AddComponent<Pickupable>();
            pickUp.isPickupable = false;

            var lw = prefab.AddComponent<LargeWorldEntity>();
            lw.cellLevel = LargeWorldEntity.CellLevel.Far;

            //Renderer
            var renderer = prefab.GetComponentInChildren<Renderer>();

            // Update sky applier
            var applier = prefab.GetComponent<SkyApplier>();
            if (applier == null)
                applier = prefab.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            prefab.AddComponent<DroneController>();
            var immuseToProp = prefab.EnsureComponent<ImmuneToPropulsioncannon>();
            immuseToProp.immuneToRepulsionCannon = true;

            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", PluginInfo.PLUGIN_NAME);

            return prefab;
        }
        catch (Exception e)
        {
            QuickLogger.Error(e.Message);
            return null;
        }
    }

    protected override Sprite GetItemSprite()
    {
        return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"TransportDronePing.png"));
    }
}
