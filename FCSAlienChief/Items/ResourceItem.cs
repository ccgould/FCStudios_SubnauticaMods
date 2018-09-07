using FCSAlienChief.Model;
using System;
using System.Collections.Generic;
using FCSAlienChief.Data;
using FCSAlienChief.Helpers;
using SMLHelper.Patchers;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSAlienChief.Items
{
    /// <summary>
    /// A class that creates a Resource item and registers it on the register method
    /// </summary>
    public class ResourceItem : AlienChiefItem
    {
        private readonly string _assetName;


        #region Constructor

        /// <summary>
        /// Default Constructor 
        /// <summary>Feeds the abstract class</summary>
        /// </summary>
        public ResourceItem(string ID, string friendlyName, string description, string itemIcon, int craftAmount, List<Ingredient> ingredients, List<string> linkedItems) : base(ID, $"{ID}Prefab")
        {
            ClassID_I = ID;

            //Asset Name
            _assetName = $"{itemIcon}_obj";

            // Set the ID of the item
            this.ClassID = ID;

            //LoadAsset
            this.GameObject = AssetHelper.Asset.LoadAsset<GameObject>(_assetName);

            // Set the resource path for the item
            this.ResourcePath = AlienChiefItem.DefaultResourcePath + this.ClassID;

            //Create a new TechType
            this.TechType = TechTypeHandler.AddTechType(ClassID, friendlyName, description, ImageUtils.LoadSpriteFromFile($"./QMods/{Information.ModName}/Assets/{itemIcon}.png"));

            // Set the recipe
            if (linkedItems.Count != 0)
            {
                craftAmount = 0;
            }
            this.Recipe = ItemHelpers.CreateRecipe(craftAmount, ingredients, linkedItems);

        }
        #endregion

        /// <summary>
        /// Registers an Item for the game
        /// </summary>
        public override void RegisterItem()
        {
            if (this.IsRegistered == false)
            {


                // Set tech tag
                var techTag = this.GameObject.AddComponent<TechTag>();
                techTag.type = this.TechType;

                // Set prefab identifier
                var prefabId = this.GameObject.AddComponent<PrefabIdentifier>();
                prefabId.ClassId = this.ClassID;

                // Set proper shaders (for crafting animation)
                Shader marmosetUber = Shader.Find("MarmosetUBER");
                Texture normal = AssetHelper.Asset.LoadAsset<Texture>(_assetName);
                var renderer = this.GameObject.GetComponentInChildren<Renderer>();
                renderer.material.shader = marmosetUber;
                renderer.material.SetTexture("_BumpMap", normal);

                // Update sky applier
                var applier = this.GameObject.GetComponent<SkyApplier>();
                if (applier == null)
                    applier = this.GameObject.AddComponent<SkyApplier>();
                applier.renderers = new Renderer[] { renderer };
                applier.anchorSky = Skies.Auto;

                // We can pick this item
                var pickupable = this.GameObject.AddComponent<Pickupable>();
                pickupable.isPickupable = true;
                pickupable.randomizeRotationWhenDropped = true;


                // Make the object drop slowly in water
                var wf = GameObject.AddComponent<WorldForces>();
                wf.underwaterGravity = 0;
                wf.underwaterDrag = 20f;
                Log.Info($"Set {ClassID} WaterForces");

                // Link the TechData to the TechType for the recipe
                CraftDataHandler.SetTechData(TechType, Recipe);
                TechType_I = TechType;
                PrefabHandler.RegisterPrefab(this);
                this.IsRegistered = true;
            }
        }

        /// <summary>
        /// The GameObject of this PreFab
        /// </summary>
        /// <returns></returns>
        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(this.GameObject);

            prefab.name = this.PrefabFileName;

            // Add fabricating animation
            var fabricatingA = prefab.AddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            return prefab;
        }
    }
}
