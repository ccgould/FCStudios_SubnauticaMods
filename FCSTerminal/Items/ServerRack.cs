using FCSTerminal.Controllers;
using FCSTerminal.Helpers;
using FCSTerminal.Interfaces;
using FCSTerminal.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using UnityEngine;

namespace FCSTerminal.Items
{
    public class ServerRack : Buildable
    {
        private GameObject _storageContainer = null;
        public GameObject ServerRackPrefab;
        private readonly string ResourcePath;


        public ServerRack(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        public void Main()
        {
            var assetName = "FCS_Server_Rack";

            _storageContainer = Resources.Load<GameObject>("Submarine/Build/Locker");

            ServerRackPrefab = AssetHelper.Asset.LoadAsset<GameObject>(assetName);

            ServerRackPrefab.name = ClassID;

            // Add constructible
            var constructable = ServerRackPrefab.AddComponent<Constructable>();
            constructable.allowedOnWall = false;
            constructable.allowedOnGround = true;
            constructable.allowedInSub = true;
            constructable.allowedInBase = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOutside = false;
            constructable.model = ServerRackPrefab.FindChild("model");
            constructable.techType = TechType;

            // Add model controller
            var serverRackController = ServerRackPrefab.AddComponent<ServerRackController>();

            // Update large world entity
            LargeWorldEntity lwe = ServerRackPrefab.AddComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

            var techTag = ServerRackPrefab.AddComponent<TechTag>();
            techTag.type = TechType;

            // Add constructable bounds
            ConstructableBounds bounds = ServerRackPrefab.AddComponent<ConstructableBounds>();

            //var collider = ServerRackPrefab.AddComponent<BoxCollider>();
            //collider.size = new Vector3(0.02f, 0.06f, 0.02f);

            var rb = ServerRackPrefab.GetComponent<Rigidbody>();
            MonoBehaviour.DestroyImmediate(rb);

        }

        public override GameObject GetGameObject()
        {

            var prefab = GameObject.Instantiate(ServerRackPrefab);
            var container = GameObject.Instantiate(_storageContainer);
            prefab.name = this.ClassID;

            // Update container renderers
            GameObject cargoCrateModel = container.FindChild("model");
            Renderer[] cargoCrateRenderers = cargoCrateModel.GetComponentsInChildren<Renderer>();
            container.transform.parent = prefab.transform;
            foreach (Renderer rend in cargoCrateRenderers)
            {
                rend.enabled = false;
            }


            container.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            container.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
            container.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            container.SetActive(true);


            // Update colliders
            GameObject builderTrigger = container.FindChild("Builder Trigger");
            GameObject collider = container.FindChild("Collider");

            BoxCollider builderCollider = builderTrigger.GetComponent<BoxCollider>();
            builderCollider.isTrigger = false;
            builderCollider.enabled = false;

            BoxCollider objectCollider = collider.GetComponent<BoxCollider>();
            objectCollider.isTrigger = false;
            objectCollider.enabled = false;

            // Delete constructable bounds
            ConstructableBounds cb = container.GetComponent<ConstructableBounds>();
            GameObject.DestroyImmediate(cb);


            var canvas = TerminalPrefabShared.CreateCanvas(prefab.transform);


            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            var tech = new TechData()
            {
                Ingredients = new List<Ingredient>
               {

                   new Ingredient(TechType.Titanium,1),
                   new Ingredient(TechType.Titanium,1),
                   new Ingredient(TechType.Titanium,1),

               }
            };

            Log.Info("Created Ingredients for TeestBuild");
            return tech;
        }

        public override string AssetsFolder { get; } = $@"FCSTerminal/Assets";
        public override string IconFileName { get; } = "Default.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;
        public override string HandOverText { get; } = "Click to open";
    }
}
