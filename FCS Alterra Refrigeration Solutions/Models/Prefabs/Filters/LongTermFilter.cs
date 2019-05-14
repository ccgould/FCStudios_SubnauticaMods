using FCS_Alterra_Refrigeration_Solutions.Configuration;
using FCS_Alterra_Refrigeration_Solutions.Logging;
using FCS_Alterra_Refrigeration_Solutions.Models.Base;
using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Prefabs.Filters
{
    public class LongTermFilter : Craftable
    {
        public LongTermFilter(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {

        }

        public void RegisterItem()
        {
            // Set proper shaders (for crafting animation)
            Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = LoadItems.LongTermFilterPrefab.GetComponentInChildren<Renderer>();
            renderer.material.shader = marmosetUber;


            // Update sky applier
            var applier = LoadItems.LongTermFilterPrefab.GetComponent<SkyApplier>();
            if (applier == null)
                applier = LoadItems.LongTermFilterPrefab.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = LoadItems.LongTermFilterPrefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;


            Log.Info($"Add Component {nameof(Filter)}");

            var filter = LoadItems.LongTermFilterPrefab.AddComponent<Filter>();
            LoadItems.LongTermFilterTechType = this.TechType;

            LoadItems.LongTermFilterPrefab.AddComponent<Rigidbody>();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(LoadItems.LongTermFilterPrefab);

            prefab.name = this.PrefabFileName;

            // Make the object drop slowly in water
            var wf = prefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 1f;
            wf.enabled = true;
            Log.Info($"Set {ClassID} WaterForces");

            // Add fabricating animation
            var fabricatingA = prefab.AddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            Log.Info("GetOrAdd TechTag");

            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            prefab.GetOrAddComponent<TechTag>().type = this.TechType;

            //LoadItems.AllowedFilters.Add(TechType, this);

            Log.Info("GetOrAdd PrefabIdentifier");

            prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

            prefab.GetOrAddComponent<BoxCollider>();


            Log.Info("Made GameObject");

            return prefab;

        }

        public override string AssetsFolder { get; } = $"{Information.ModName}/Assets";

        protected override TechData GetBlueprintRecipe()
        {
            Log.Info($"Creating {FriendlyName} recipe...");
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
            Log.Info($"Created Ingredients for the {FriendlyName}");
            return customFabRecipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;
        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Fabricator;
        public override string IconFileName { get; } = "LongTermFilter.png";
    }
}
