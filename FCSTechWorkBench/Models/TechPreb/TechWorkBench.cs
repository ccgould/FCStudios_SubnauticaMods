using FCSTechWorkBench.Configuration;
using FCSTechWorkBench.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using FCSCommon.Extensions;
using UnityEngine;

namespace FCSTechWorkBench.Models.TechPreb
{
    public class TechWorkBench : Buildable
    {
        public TechWorkBench(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        public void Register()
        {
            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.FCSTechWorkBenchPrefab.AddComponent<TechTag>();

            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructible bounds
            LoadItems.FCSTechWorkBenchPrefab.AddComponent<ConstructableBounds>();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                Log.Info("Making GameObject");

                Log.Info("Instantiate GameObject");

                prefab = GameObject.Instantiate(LoadItems.FCSTechWorkBenchPrefab);

                prefab.name = Information.ModName;

                //========== Allows the building animation and material colors ==========// 

                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material.shader = shader;
                }

                //ApplyMaterials(prefab);

                SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                Log.Info("Adding Constructible");

                // Add constructible
                var constructable = prefab.GetOrAddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = prefab.FindChild("model");
                constructable.rotationEnabled = true;
                constructable.techType = TechType;

                Log.Info("GetOrAdd TechTag");

                // Allows the object to be saved into the game 
                //by setting the TechTag and the PrefabIdentifier 
                prefab.GetOrAddComponent<TechTag>().type = this.TechType;

                Log.Info("GetOrAdd PrefabIdentifier");

                prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

                prefab.GetOrAddComponent<BoxCollider>();

                Log.Info("Made GameObject");


            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            Log.Info($"Creating {Information.ModFriendly} recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.JeweledDiskPiece, 1),
                    new Ingredient(TechType.Magnetite, 1)
                }
            };
            Log.Info($"Created Ingredients for FCS {Information.ModFriendly}");
            return customFabRecipe;
        }

        public override string IconFileName { get; } = "Default.png";
        public override string AssetsFolder { get; } = Information.ASSETSFOLDER;
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
    }
}
