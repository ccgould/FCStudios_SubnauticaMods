using FCSCommon.Extensions;
using FCSDeepHarvester.Configuration;
using FCSDeepHarvester.Logging;
using FCSDeepHarvester.Models.Controllers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCSDeepHarvester.Models.Prefabs
{
    public class DeepHarvester : Buildable
    {


        private Constructable _constructable;
        private readonly GameObject _bulbo;

        public DeepHarvester(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
            Information.ASSETSFOLDER = Path.Combine("QMods", AssetsFolder);

            _bulbo = Resources.Load<GameObject>("WorldEntities/Seeds/BulboTreePiece");
        }

        /// <summary>
        /// Registers the prefab
        /// </summary>
        public void Register()
        {
            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.FcsDeepHarvesterPrefab.AddComponent<TechTag>();
            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructable bounds
            LoadItems.FcsDeepHarvesterPrefab.AddComponent<ConstructableBounds>().bounds = new OrientedBounds(new Vector3(-0.1f, -0.1f, 0f), new Quaternion(0, 0, 0, 0), new Vector3(0.9f, 0.5f, 0f));

            Log.Info("Destroy GameObject Rigid body");

            var rb = LoadItems.FcsDeepHarvesterPrefab.GetComponent<Rigidbody>();
            MonoBehaviour.DestroyImmediate(rb);
        }

        public override GameObject GetGameObject()
        {
            Log.Info($"// == Making {Information.ModFriendly} GameObject == //");

            Log.Info($"// == Instantiate {Information.ModFriendly} GameObject == //");

            GameObject prefab = GameObject.Instantiate(LoadItems.FcsDeepHarvesterPrefab);

            GameObject bulbo = GameObject.Instantiate(_bulbo);

            var pickable = bulbo.GetComponent<Pickupable>();

            //var planter = prefab.FindChild("model").FindChild("anim_parts").FindChild("TurnTable_Left").FindChild("Panter_1").GetOrAddComponent<Planter>();

            //planter.storageContainer = new StorageContainer();

            //planter.slots.Add(planter.transform);

            //planter.storageContainer.container.AddItem(pickable);

            CreateBuildableShaders(prefab);

            Log.Info("Adding Constructible");

            // Add constructible
            _constructable = GameObjectExtensions.GetOrAddComponent<Constructable>(prefab);
            _constructable.allowedOnWall = false;
            _constructable.allowedOnGround = true;
            _constructable.allowedInSub = false;
            _constructable.allowedInBase = true;
            _constructable.allowedOnCeiling = false;
            _constructable.allowedOutside = true;
            _constructable.model = prefab.FindChild("model");
            _constructable.techType = TechType;


            Log.Info("GetOrAdd TechTag");
            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            GameObjectExtensions.GetOrAddComponent<TechTag>(prefab).type = this.TechType;

            Log.Info("GetOrAdd PrefabIdentifier");

            GameObjectExtensions.GetOrAddComponent<PrefabIdentifier>(prefab).ClassId = this.ClassID;

            GameObjectExtensions.GetOrAddComponent<BoxCollider>(prefab);

            Log.Info("Add GameObject CustomBatteryController");

            GameObjectExtensions.GetOrAddComponent<DeepHarvesterController>(prefab);


            Log.Info("Made GameObject");

            //LoadItems.DEEP_HARVESTER_TECHTYPE = this.TechType;


            return prefab;
        }


        protected override TechData GetBlueprintRecipe()
        {
            Log.Info("Creating FCS Terminal Server Rack recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 1)
                }
            };
            Log.Info("Created Ingredients for FCS Terminal Server Rack");
            return customFabRecipe;
        }

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
                if (renderer.name != "Glass")
                {
                    renderer.material.shader = shader;
                }
            }

            SkyApplier skyApplier = GameObjectExtensions.GetOrAddComponent<SkyApplier>(prefab);
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;

        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;

        public override string AssetsFolder { get; } = Path.Combine(Information.ModName, "Assets");

        public override string IconFileName { get; } = "Default.png";

        public override string HandOverText { get; } = "FCS Deep Harvester";
    }
}
