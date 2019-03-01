using FCSSubnauticaCore.Extensions;
using FCSTerminal.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using UnityEngine;

namespace FCSTerminal.Models.Prefabs
{
    public class ServerPrefab : Craftable
    {
        public ServerPrefab(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {

        }

        public void RegisterItem()
        {


            // Set proper shaders (for crafting animation)
            Shader marmosetUber = Shader.Find("MarmosetUBER");
            var renderer = LoadItems.ServerPrefab.GetComponentInChildren<Renderer>();
            renderer.material.shader = marmosetUber;


            // Update sky applier
            var applier = LoadItems.ServerPrefab.GetComponent<SkyApplier>();
            if (applier == null)
                applier = LoadItems.ServerPrefab.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { renderer };
            applier.anchorSky = Skies.Auto;

            // We can pick this item
            var pickupable = LoadItems.ServerPrefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Make the object drop slowly in water
            var wf = LoadItems.ServerPrefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 20f;
            Log.Info($"Set {ClassID} WaterForces");
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(LoadItems.ServerPrefab);

            prefab.name = FriendlyName;

            CreateBuildableShaders(prefab);
            Log.Info("GetOrAdd TechTag");
            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            prefab.GetOrAddComponent<TechTag>().type = this.TechType;

            Log.Info("GetOrAdd PrefabIdentifier");

            prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

            // Add fabricating animation
            var fabricatingA = prefab.AddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            return prefab;
        }

        public override string AssetsFolder { get; } = @"FCSTerminal/Assets";

        public override string IconFileName { get; } = "Server.png";

        /// <summary>
        /// Creates the shader needed for the building animation
        /// </summary>
        /// <param name="prefab"></param>
        private void CreateBuildableShaders(GameObject prefab)
        {
            //========== Allows the building animation and material colors ==========// 

            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.shader = shader;
            }

            SkyApplier skyApplier = GameObjectExtensions.GetOrAddComponent<SkyApplier>(prefab);
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 
        }


        protected override TechData GetBlueprintRecipe()
        {
            var recipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.WiringKit, 1),
                }
            };

            return recipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;
        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Workbench;
    }
}
