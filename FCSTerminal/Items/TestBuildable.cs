using FCSTerminal.Helpers;
using FCSTerminal.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCSTerminal.Items
{
    public class TestBuildable : Buildable
    {
        public  GameObject prefab;
        private string ResourcePath;
        public TestBuildable(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        public void Main()
        {
            var assetName = "FCS_Server_Rack";


            prefab = AssetHelper.Asset.LoadAsset<GameObject>(assetName);

            prefab.name = ClassID;

            // Add contructable
            var constructable = prefab.AddComponent<Constructable>();
            constructable.allowedOnWall = false;
            constructable.allowedOnGround = true;
            constructable.allowedInSub = true;
            constructable.allowedInBase = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOutside = false;
            constructable.model = prefab.FindChild("model");
            constructable.techType = TechType;


            // Update large world entity
            LargeWorldEntity lwe = prefab.AddComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

            var techTag = prefab.AddComponent<TechTag>();
            techTag.type = TechType;

            // Add constructable bounds
            ConstructableBounds bounds = prefab.AddComponent<ConstructableBounds>();

            //var collider = prefab.AddComponent<BoxCollider>();
            //collider.size = new Vector3(0.02f, 0.06f, 0.02f);

            var rb = prefab.GetComponent<Rigidbody>();
            MonoBehaviour.DestroyImmediate(rb);

        }

        public override GameObject GetGameObject()
        {

            var prefabf = GameObject.Instantiate(prefab);
            prefab.name = this.ClassID;

            return prefabf;
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
