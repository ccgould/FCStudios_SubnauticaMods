using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Configuration;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_AlterraHomeSolutions.Spawnables
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
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.PaintToolKitClassID.ToTechType(),
                    300000000f, StoreCategory.Home);
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
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>
                {
                    new Ingredient(TechType.Copper,1)
                }
            };
        }
    }
}
