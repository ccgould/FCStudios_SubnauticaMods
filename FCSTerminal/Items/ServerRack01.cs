using System.Collections.Generic;
using FCSTerminal.Configuration;
using FCSTerminal.Helpers;
using FCSTerminal.Logging;
using FCSTerminal.Models;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTerminal.Items
{
    public class ServerRack01 : FCSTerminalItem
    {
        public ServerRack01(TechType techType = TechType.None) : base("ServerRack01", "ServerRack01Prefab", techType)
        {
            //Asset Name
            var assetName = "server_rack_large_obj";

            // Set Reference
            ResourcePath = DefaultResourcePath + ClassID;

            //LoadAsset
            GameObject = AssetHelper.Asset.LoadAsset<GameObject>(assetName);

            //Create a new TechType
            TechType = TechTypeHandler.AddTechType(ClassID, "Server Rack Large", "Storage drives for the Terminal System", ImageUtils.LoadSpriteFromFile($"./QMods/{Information.ModName}/Assets/serverRackLarge.png"));

            Log.Info($"ClassID = {ClassID}");

            // Set the recipe
            Recipe = new TechData
            {
                Ingredients = new List<Ingredient>
                {
                    new Ingredient(TechType.TitaniumIngot,1),
                    new Ingredient(TechType.ComputerChip,1),
                    new Ingredient(TechType.Gold,1),
                    new Ingredient(TechType.JeweledDiskPiece,1)
                }
            };

            // Link the TechData to the TechType for the recipe
            CraftDataHandler.SetTechData(TechType, Recipe);
        }

        /// <summary>
        /// Registers an Item for the game
        /// </summary>
        public override void RegisterItem()
        {
            if (IsRegistered == false)
            {
                // Add prefab identifier
                var prefabId = GameObject.AddComponent<PrefabIdentifier>();
                prefabId.ClassId = PrefabFileName;

                // Add large world entity
                var lwe = GameObject.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

                // Add tech tag
                var techTag = GameObject.AddComponent<TechTag>();
                techTag.type = TechType;

                // Set proper shaders (for crafting animation)
                Shader marmosetUber = Shader.Find("MarmosetUBER");

                var renderers = GameObject.GetComponentsInChildren<Renderer>();

                if (renderers.Length > 0)
                {
                    foreach (Renderer rend in renderers)
                    {
                        if (rend.materials.Length > 0)
                        {
                            foreach (Material tmpMat in rend.materials)
                            {
                                tmpMat.shader = marmosetUber;
                            }
                        }
                    }
                }


                // Add contructable
                var constructable = GameObject.AddComponent<Constructable>();
                constructable.allowedInBase = true;
                constructable.allowedInSub = true;
                constructable.allowedOutside = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnConstructables = true;
                constructable.controlModelState = true;
                constructable.deconstructionAllowed = true;
                constructable.rotationEnabled = true;
                constructable.model = GameObject;
                constructable.techType = TechType;
                constructable.enabled = true;

                // Add constructable bounds
                var bounds = GameObject.AddComponent<ConstructableBounds>();
                bounds.bounds.position = new Vector3(bounds.bounds.position.x, bounds.bounds.position.y + 0.032f, bounds.bounds.position.z);

                // Add sky applier
                var applier = GameObject.GetComponent<SkyApplier>();
                if (applier == null)
                {
                    applier = GameObject.AddComponent<SkyApplier>();
                }
                applier.renderers = GameObject.GetComponentsInChildren<Renderer>();
                applier.anchorSky = Skies.Auto;

                // Add model controller
                GameObject.AddComponent<Container>();
                
                // Add new TechType to the buildables
                CraftDataHandler.AddBuildable(TechType);
                CraftDataHandler.AddToGroup(TechGroup.Miscellaneous, TechCategory.Misc, TechType);

                // Register Prefab
                PrefabHandler.RegisterPrefab(this);

                IsRegistered = true;
            }
        }

        public override GameObject GetGameObject()
        {
            
            var prefab = GameObject.Instantiate(GameObject);

            var container = GameObject;
            Log.Info($"Location {container.transform.localPosition}");
            container.SetActive(true);


            return prefab;
        }
    }
}
