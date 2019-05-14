using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSCommon.Components;
using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using FCSAlterraIndustrialSolutions.Models.Components;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Prefabs
{
    public class DeepDriller : Buildable
    {

        public GameObject SmallLocker { get; set; }

        public DeepDriller(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        private readonly ItemsContainer _itemContainer;

        /// <summary>
        /// Registers prefab
        /// </summary>
        public void Register()
        {

            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.DeepDrillerPrefab.AddComponent<TechTag>();

            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructible bounds
            LoadItems.DeepDrillerPrefab.AddComponent<ConstructableBounds>();

            SmallLocker = Resources.Load<GameObject>("Submarine/Build/SmallLocker");

        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                Log.Info($"Making {Information.DeepDrillerFriendly} GameObject");

                Log.Info("Instantiate GameObject");

                prefab = GameObject.Instantiate(LoadItems.DeepDrillerPrefab);
                


                prefab.name = Information.DeepDrillerName;


                //var storage = GameObject.Instantiate(SmallLocker);

                //storage.name = $"{Information.DeepDrillerName}S2";

                //var container = storage.GetComponent<StorageContainer>();
                //container.width = 6;
                //container.height = 6;
                //container.container.Resize(6, 6);
                //container.storageLabel = $"{Information.DeepDrillerFriendly}";
                //container.hoverText = $"Open {Information.DeepDrillerFriendly}";

                //storage.transform.SetParent(prefab.transform, false);
                //storage.transform.localPosition = new Vector3(0.0226624f, 3.9232f, 1f);
                //storage.transform.Rotate(new Vector3(0, 0, 0));
                //storage.transform.localScale = new Vector3(0.8129452f, 0.65f, 0.7868078f);

                //var meshRenderers = storage.GetComponentsInChildren<MeshRenderer>();

                //foreach (var meshRenderer in meshRenderers)
                //{
                //    GameObject.DestroyImmediate(meshRenderer);
                //}


                //var label = storage.FindChild("Label");
                //GameObject.DestroyImmediate(label);



                //Locker1(ref prefab);
                //Locker2(ref prefab);
                //Locker3(ref prefab);
                //Locker4(ref prefab);
                
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

                Log.Info("Adding Constructible");

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

                Log.Info("GetOrAdd TechTag");

                var powerFX = prefab.AddComponent<PowerFX>();
                
                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                prefab.GetOrAddComponent<BoxCollider>();

                // Allows the object to be saved into the game 
                //by setting the TechTag and the PrefabIdentifier 
                prefab.GetOrAddComponent<TechTag>().type = this.TechType;

                Log.Info("GetOrAdd PrefabIdentifier");

                prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

                prefab.GetOrAddComponent<BoxCollider>();

                Log.Info($"Add Component {nameof(DeepDrillerController)}");
                prefab.GetOrAddComponent<DeepDrillerController>();

                Log.Info($"Add Component {nameof(LiveMixin)}");
                prefab.AddComponent<LiveMixin>();

                Log.Info("Made GameObject");
            }
            catch (Exception e)
            {
                Log.Error($"{Information.DeepDrillerFriendly} Message: {e.Message}");
            }

            return prefab;
        }

        private GameObject Locker1(ref GameObject prefab)
        {
            var storage = GameObject.Instantiate(SmallLocker);
            storage.name = $"{Information.DeepDrillerName}S1";

            var container = storage.GetComponent<StorageContainer>();
            container.width = 5;
            container.height = 5;
            container.container.Resize(5, 5);
            container.storageLabel = $"{Information.DeepDrillerFriendly}";
            container.hoverText = $"Open {Information.DeepDrillerFriendly}";

            storage.transform.SetParent(prefab.transform, false);
            storage.transform.localPosition = new Vector3(0.935f, 3.9232f, -0.0007f);
            storage.transform.Rotate(new Vector3(0, 90, 0));
            storage.transform.localScale = new Vector3(0.8129452f, 0.65f, 0.7868078f);

            var meshRenderers = storage.GetComponentsInChildren<MeshRenderer>();

            foreach (var meshRenderer in meshRenderers)
            {
                GameObject.DestroyImmediate(meshRenderer);
            }


            var label = storage.FindChild("Label");
            GameObject.DestroyImmediate(label);

            return storage;
        }

        private GameObject Locker2(ref GameObject prefab)
        {
            var storage = GameObject.Instantiate(SmallLocker);

            storage.name = $"{Information.DeepDrillerName}S2";

            var container = storage.GetComponent<StorageContainer>();
            container.width = 6;
            container.height = 6;
            container.container.Resize(6, 6);
            container.storageLabel = $"{Information.DeepDrillerFriendly}";
            container.hoverText = $"Open {Information.DeepDrillerFriendly}";

            storage.transform.SetParent(prefab.transform, false);
            storage.transform.localPosition = new Vector3(0.0226624f, 3.9232f, 1f);
            storage.transform.Rotate(new Vector3(0, 0, 0));
            storage.transform.localScale = new Vector3(0.8129452f, 0.65f, 0.7868078f);

            var meshRenderers = storage.GetComponentsInChildren<MeshRenderer>();

            foreach (var meshRenderer in meshRenderers)
            {
                GameObject.DestroyImmediate(meshRenderer);
            }


            var label = storage.FindChild("Label");
            GameObject.DestroyImmediate(label);

            return storage;
        }

        private GameObject Locker3(ref GameObject prefab)
        {
            var storage = GameObject.Instantiate(SmallLocker);

            storage.name = $"{Information.DeepDrillerName}S3";

            var container = storage.GetComponent<StorageContainer>();
            container.width = 5;
            container.height = 5;
            container.container.Resize(5, 5);
            container.storageLabel = $"{Information.DeepDrillerFriendly}";
            container.hoverText = $"Open {Information.DeepDrillerFriendly}";

            storage.transform.SetParent(prefab.transform, false);
            storage.transform.localPosition = new Vector3(-1.027f, 3.9232f, 0.002f);
            storage.transform.Rotate(new Vector3(0, -90, 0));
            storage.transform.localScale = new Vector3(0.8129452f, 0.65f, 0.7868078f);

            var meshRenderers = storage.GetComponentsInChildren<MeshRenderer>();

            foreach (var meshRenderer in meshRenderers)
            {
                GameObject.DestroyImmediate(meshRenderer);
            }


            var label = storage.FindChild("Label");
            GameObject.DestroyImmediate(label);

            return storage;
        }

        private GameObject Locker4(ref GameObject prefab)
        {
            var storage = GameObject.Instantiate(SmallLocker);

            storage.name = $"{Information.DeepDrillerName}S4";

            var container = storage.GetComponent<StorageContainer>();
            container.width = 5;
            container.height = 5;
            container.container.Resize(5, 5);
            container.storageLabel = $"{Information.DeepDrillerFriendly}";
            container.hoverText = $"Open {Information.DeepDrillerFriendly}";

            storage.transform.SetParent(prefab.transform, false);
            storage.transform.localPosition = new Vector3(0.004f, 3.9232f, -1.022f);
            storage.transform.Rotate(new Vector3(0, 180, 0));
            storage.transform.localScale = new Vector3(0.8129452f, 0.65f, 0.7868078f);

            var meshRenderers = storage.GetComponentsInChildren<MeshRenderer>();

            foreach (var meshRenderer in meshRenderers)
            {
                GameObject.DestroyImmediate(meshRenderer);
            }


            var label = storage.FindChild("Label");
            GameObject.DestroyImmediate(label);

            return storage;
        }

        //private void AddLocker(GameObject prefab, string lockerName, int width, int height, Vector3 position)
        //{
        //    var storage1 = GameObject.Instantiate(SmallLocker);
        //    storage1.name = lockerName;

        //    var container = storage1.GetComponent<StorageContainer>();
        //    container.width = width;
        //    container.height = height;
        //    container.container.Resize(width, height);
        //    container.storageLabel = $"{Information.DeepDrillerFriendly}";
        //    container.hoverText = $"Open {Information.DeepDrillerFriendly}";

        //    storage1.transform.SetParent(prefab.transform, false);
        //    storage1.transform.localPosition = position;
        //    storage1.transform.Rotate(new Vector3(0, 90, 0));
        //    storage1.transform.localScale = new Vector3(0.8129452f, 0.65f, 0.7868078f);
        //}


        protected override TechData GetBlueprintRecipe()
        {
            Log.Info($"Creating {Information.DeepDrillerFriendly} recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.Diamond, 2),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Glass, 1)
                }
            };
            Log.Info($"Created Ingredients for the {Information.DeepDrillerFriendly}");
            return customFabRecipe;
        }

        public override string IconFileName { get; } = "DeepDriller.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;
        public override string AssetsFolder { get; } = $"{Information.ModName}/Assets";
    }
}
