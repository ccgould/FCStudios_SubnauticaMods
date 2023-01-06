using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.PaintTool.Mono;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif


namespace FCS_HomeSolutions.Mods.PaintTool.Spawnable
{
    internal class PaintToolSpawnable : Equipable
    {
        private readonly GameObject _prefab;
        public override EquipmentType EquipmentType => EquipmentType.Hand;
        public override string AssetsFolder => Mod.GetAssetPath();
        public override QuickSlotType QuickSlotType => QuickSlotType.Selectable;
        public override Vector2int SizeInInventory => new Vector2int(2, 2);
        public override TechGroup GroupForPDA => TechGroup.Personal;
        public override TechCategory CategoryForPDA => TechCategory.Tools;

        internal const string PaintToolClassID = "PaintTool";
        internal const string PaintToolFriendly = "Alterra Paint Tool";

        internal const string PaintToolDescription =
            "Change the color of Primary and Secondary surfaces, and LED lights (requires Paint Can). Only suitable for Alterra FCStudios products.";

        internal const string PaintToolPrefabName = "FCS_PaintTool";
        internal const string PaintToolKitClassID = "PaintTool_Kit";


        public PaintToolSpawnable() : base(PaintToolClassID, PaintToolFriendly, PaintToolDescription)
        {
            _prefab = ModelPrefab.GetPrefab(PaintToolPrefabName);

            OnStartedPatching += () =>
            {
                var paintToolKit = new FCSKit(PaintToolKitClassID, PaintToolFriendly, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                paintToolKit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType, 1, 7500, StoreCategory.Home, true);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);
                
                prefab.SetActive(false);

                prefab.AddComponent<PrefabIdentifier>();
                prefab.AddComponent<TechTag>().type = TechType;

                var pickUp = prefab.AddComponent<Pickupable>();
                pickUp.randomizeRotationWhenDropped = true;
                pickUp.isPickupable = true;

                var rigidBody = prefab.EnsureComponent<Rigidbody>();

                // Make the object drop slowly in water
                var wf = prefab.AddComponent<WorldForces>();
                wf.underwaterGravity = 0;
                wf.underwaterDrag = 10f;
                wf.enabled = true;
                wf.useRigidbody = rigidBody;

                // Set collider
                var collider = prefab.GetComponent<BoxCollider>();

                //Renderer
                var renderer = prefab.GetComponentInChildren<Renderer>();

                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;

                // Update sky applier
                var applier = prefab.GetComponent<SkyApplier>();
                if (applier == null)
                    applier = prefab.AddComponent<SkyApplier>();
                applier.renderers = new Renderer[] { renderer };
                applier.anchorSky = Skies.Auto;

                var cont = prefab.AddComponent<PaintToolController>();
                cont.ikAimRightArm = true;
                cont.hasBashAnimation = true;
                prefab.SetActive(true);


                Renderer[] componentsInChildren = prefab.transform.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer currenderer in componentsInChildren)
                {
                    currenderer.material.shader = Shader.Find("MarmosetUBER");
                }
                
                ModelPrefab.ApplyShaders(prefab, ModelPrefab.ModBundle);

                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = componentsInChildren;
                skyApplier.anchorSky = Skies.Auto;


                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }
        
        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(PaintToolKitClassID.ToTechType(), 1),
            }
        };
    }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
