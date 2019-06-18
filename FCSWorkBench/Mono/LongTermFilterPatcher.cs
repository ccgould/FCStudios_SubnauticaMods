using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechWorkBench.Abstract_Classes;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using UnityEngine;

namespace FCSTechWorkBench.Mono
{
    public partial class LongTermFilterBuildable : FCSTechWorkBenchItem
    {
        private TechGroup GroupForPDA = TechGroup.Resources;
        private TechCategory CategoryForPDA = TechCategory.Electronics;

        public LongTermFilterBuildable() : base("LongTermFilter_ARS", "Long Term Filter")
        {

        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(this._prefab);
            prefab.name = this.PrefabFileName;

            var filter = prefab.AddComponent<WorkBenchFilter>();
            filter.FilterType = FilterTypes.LongTermFilter;
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
                    new Ingredient(TechType.FiberMesh, 1),
                    new Ingredient(TechType.Bleach, 1),
                    new Ingredient(TechType.Silicone, 1),
                    new Ingredient(TechType.Aerogel, 1),
                    new Ingredient(TechType.Gold, 1),
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
                    //Create a new TechType
                    this.TechType = TechTypeHandler.AddTechType(ClassID, PrefabFileName, "Filter", new Atlas.Sprite(QPatch.Bundle.LoadAsset<Texture2D>($"AlterraLogo.png")));


                    CraftDataHandler.SetTechData(TechType, GetBlueprintRecipe());

                    CraftDataHandler.AddToGroup(this.GroupForPDA, this.CategoryForPDA, this.TechType);

                    ClassID_I = this.ClassID;

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
                QuickLogger.Error("Failed to get the LongTermPrefab");
            }
        }

    }
}
