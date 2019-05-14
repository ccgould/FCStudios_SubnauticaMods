using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSCyclopsDock.Configuration;
using FCSCyclopsDock.Models.Controllers;
using FCSCyclopsDock.Models.Operations;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using FCSCommon.Helpers;
using JetBrains.Annotations;
using UnityEngine;

namespace FCSCyclopsDock.Models.Prefabs
{
    public class CyclopsDock : Buildable
    {
        public override string AssetsFolder { get; } = Information.GetAssetPath();
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        public CyclopsDock(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                QuickLogger.Info($"Making {Information.ModFriendly} GameObject");

                QuickLogger.Debug("Instantiate GameObject");

                prefab = GameObject.Instantiate(LoadItems.CyclopsDockPrefab);



                prefab.name = Information.ModName;

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

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.GetOrAddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = prefab.FindChild("model");
                constructable.rotationEnabled = true;
                constructable.techType = TechType;

                QuickLogger.Debug("GetOrAdd TechTag");

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                prefab.GetOrAddComponent<BoxCollider>();

                // Allows the object to be saved into the game 
                //by setting the TechTag and the PrefabIdentifier 
                prefab.GetOrAddComponent<TechTag>().type = this.TechType;

                QuickLogger.Debug("GetOrAdd PrefabIdentifier");

                prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

                prefab.GetOrAddComponent<BoxCollider>();

                QuickLogger.Debug($"Add Component {nameof(CyclopsDockController)}");
                prefab.GetOrAddComponent<CyclopsDockController>();

                QuickLogger.Debug($"Add Component {nameof(DockSubmarine)}");
                prefab.GetOrAddComponent<DockSubmarine>();


                QuickLogger.Debug("Made GameObject");
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{Information.ModFriendly} Message: {e.Message}");
            }

            return prefab;
        }



        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating {Information.ModFriendly} recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.Diamond, 2),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Glass, 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients for the {Information.ModFriendly}");
            return customFabRecipe;
        }

        public void Register()
        {
            QuickLogger.Debug("Set GameObject Tag");

            var techTag = LoadItems.CyclopsDockPrefab.AddComponent<TechTag>();

            techTag.type = TechType;

            QuickLogger.Debug("Set GameObject Bounds");

            // Add constructible bounds
            LoadItems.CyclopsDockPrefab.AddComponent<ConstructableBounds>();
        }
    }
}
