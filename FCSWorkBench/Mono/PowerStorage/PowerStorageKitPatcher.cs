using FCSCommon.Utilities;
using FCSTechWorkBench.Abstract_Classes;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace FCSTechWorkBench.Mono.PowerStorage
{
    public partial class PowerStorageKitBuildable : FCSTechWorkBenchItem
    {
        private TechGroup GroupForPDA = TechGroup.Resources;
        private TechCategory CategoryForPDA = TechCategory.AdvancedMaterials;


        public PowerStorageKitBuildable() : base("PowerStorageKit_PS", "Power Storage Kit")
        {

        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(this._prefab);
            prefab.name = this.PrefabFileName;

            var filter = prefab.AddComponent<Freon>();
            return prefab;
        }

        private TechData GetBlueprintRecipe()
        {
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Battery, 1),
                    new Ingredient(TechType.AcidMushroom, 6),
                    new Ingredient(TechType.Titanium, 7),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Quartz, 1),
                    new Ingredient(TechType.Salt, 6),
                }
            };
            return customFabRecipe;
        }

        public override void Register()
        {
            if (GetPrefabs())
            {
                if (this.IsRegistered == false)
                {

                    ClassID_I = this.ClassID;

                    //Create a new TechType
                    this.TechType = TechTypeHandler.AddTechType(ClassID, PrefabFileName, "A kit that allows you to build one Power Storage Unit", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/FCSTechWorkBench/Assets/{ClassID}.png")));

                    CraftDataHandler.SetTechData(TechType, GetBlueprintRecipe());

                    CraftDataHandler.AddToGroup(this.GroupForPDA, this.CategoryForPDA, this.TechType);


                    QuickLogger.Debug($"Class Id = {ClassID_I}");

                    FriendlyName_I = this.PrefabFileName;

                    TechTypeID = TechType;

                    // Make the object drop slowly in water
                    var wf = _prefab.AddComponent<WorldForces>();
                    wf.underwaterGravity = 0;
                    wf.underwaterDrag = 20f;
                    wf.enabled = true;

                    // Add fabricating animation
                    var fabricatingA = _prefab.AddComponent<VFXFabricating>();
                    fabricatingA.localMinY = -0.1f;
                    fabricatingA.localMaxY = 0.6f;
                    fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
                    fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
                    fabricatingA.scaleFactor = 1.0f;

                    // Set proper shaders (for crafting animation)
                    Shader marmosetUber = Shader.Find("MarmosetUBER");
                    var renderer = _prefab.GetComponentInChildren<Renderer>();
                    renderer.material.shader = marmosetUber;

                    // Update sky applier
                    var applier = _prefab.GetComponent<SkyApplier>();
                    if (applier == null)
                        applier = _prefab.AddComponent<SkyApplier>();
                    applier.renderers = new Renderer[] { renderer };
                    applier.anchorSky = Skies.Auto;

                    // We can pick this item
                    var pickupable = _prefab.AddComponent<Pickupable>();
                    pickupable.isPickupable = true;
                    pickupable.randomizeRotationWhenDropped = true;

                    PrefabIdentifier prefabID = _prefab.AddComponent<PrefabIdentifier>();
                    prefabID.ClassId = this.ClassID;

                    var techTag = this._prefab.AddComponent<TechTag>();
                    techTag.type = this.TechType;

                    PrefabHandler.RegisterPrefab(this);

                    this.IsRegistered = true;
                }
            }
            else
            {
                QuickLogger.Error("Failed to get the Kit Prefab");
            }
        }

    }
}
