using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mono.PaintTool;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif


namespace FCS_HomeSolutions.Spawnables
{
    internal class PaintToolSpawnable : Equipable
    {
        public override EquipmentType EquipmentType => EquipmentType.Hand;
        public override string AssetsFolder => Mod.GetAssetPath();
        public override QuickSlotType QuickSlotType => QuickSlotType.Selectable;
        public override Vector2int SizeInInventory => new Vector2int(2, 2);
        public override TechGroup GroupForPDA => TechGroup.Personal;
        public override TechCategory CategoryForPDA => TechCategory.Tools;

        public PaintToolSpawnable() : base(Mod.PaintToolClassID, Mod.PaintToolFriendly, Mod.PaintToolDescription)
        {
            OnFinishedPatching += () =>
            {
                var paintToolKit = new FCSKit(Mod.PaintToolKitClassID, Mod.PaintToolFriendly, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                paintToolKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType,1,7500, StoreCategory.Home,true);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.PaintToolPrefab);
                
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
            return Mod.PaintToolIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
