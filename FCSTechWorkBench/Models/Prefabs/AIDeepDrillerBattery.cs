using FCSCommon.Components;
using FCSCommon.Extensions;
using FCSTechWorkBench.Configuration;
using FCSTechWorkBench.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Controllers;
using UnityEngine;

namespace FCSTechWorkBench.Models.Prefabs
{
    /// <summary>
    /// This defind a AI Deep Driller Battery this battery lasts for 1 in game  week (8400)sec 
    /// </summary>
    public class AIDeepDrillerBattery : ModPrefab
    {

        private readonly string _friendlyName;
        private readonly string _description;

        public AIDeepDrillerBattery(string classId, string friendlyName, string description) : base(classId, friendlyName)
        {
            _friendlyName = friendlyName;
            _description = description;
        }

        public void Register()
        {
            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.FCSAIDeepDrillerBatteryPrefab.AddComponent<TechTag>();


            // We can pick this item
            var pickupable = LoadItems.FCSAIDeepDrillerBatteryPrefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Make the object drop slowly in water
            var wf = LoadItems.FCSAIDeepDrillerBatteryPrefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 20f;
            Log.Info($"Set {Information.AIDeepDrillerBatteryFriendly} WaterForces");

            //Create a new TechType
            this.TechType = TechTypeHandler.AddTechType(ClassID, _friendlyName, _description, ImageUtils.LoadSpriteFromFile(Path.Combine(Information.ASSETSFOLDER, $"{Information.AIDeepDrillerBatteryName}.png")));

            techTag.type = TechType;

            // Link the TechData to the TechType for the recipe
            CraftDataHandler.SetTechData(TechType, GetBlueprintRecipe());

            CraftDataHandler.AddToGroup(TechGroup.Miscellaneous, TechCategory.Misc, TechType);

            PrefabHandler.RegisterPrefab(this);
        }


        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                Log.Info($"Making {Information.AIDeepDrillerBatteryFriendly} GameObject");

                Log.Info("Instantiate GameObject");

                prefab = GameObject.Instantiate(LoadItems.FCSAIDeepDrillerBatteryPrefab);

                prefab.name = Information.AIDeepDrillerBatteryName;

                //========== Allows the building animation and material colors ==========// 

                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material.shader = shader;
                }

                SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                Log.Info("GetOrAdd TechTag");

                // Allows the object to be saved into the game 
                //by setting the TechTag and the PrefabIdentifier 
                prefab.GetOrAddComponent<TechTag>().type = this.TechType;

                Log.Info("GetOrAdd PrefabIdentifier");

                prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

                Log.Info($"Adding {nameof(AIDeepBatteryController)}");
                prefab.AddComponent<AIDeepBatteryController>();

                Log.Info($"Adding {nameof(InternalBatteryController)}");
                prefab.AddComponent<InternalBatteryController>();

                Log.Info("Made GameObject");
            }
            catch (Exception e)
            {
                Log.Error($"{Information.AIDeepDrillerBatteryFriendly} Message: {e.Message}");
            }

            return prefab;
        }


        private TechData GetBlueprintRecipe()
        {
            Log.Info($"Creating {Information.AIDeepDrillerBatteryFriendly} recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Copper, 1),
                    new Ingredient(TechType.AcidMushroom, 2),
                    new Ingredient(TechType.Silicone, 1),
                    new Ingredient(TechType.Titanium, 1)
                }
            };
            Log.Info($"Created Ingredients for FCS {Information.AIDeepDrillerBatteryFriendly}");
            return customFabRecipe;
        }


        //public override string IconFileName { get; } = "AIDeepDrillerBattery.png";
        //public override string AssetsFolder { get; } = Information.ASSETSFOLDER;
        //public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        //public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        //public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Fabricator;


    }
}
