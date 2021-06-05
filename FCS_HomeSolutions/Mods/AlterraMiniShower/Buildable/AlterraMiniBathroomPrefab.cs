using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.AlterraMiniShower.Mono;
using FCS_HomeSolutions.Mods.Cabinets.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.Cabinets.Buildable
{
    internal class AlterraMiniBathroomBuildable : SMLHelper.V2.Assets.Buildable
    {
        private GameObject _locker;
        private GameObject _stairShipChair;

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        public AlterraMiniBathroomBuildable() : base(Mod.AlterraMiniBathroomClassID, Mod.AlterraMiniBathroomFriendly, Mod.AlterraMiniBathroomDescription)
        {
            OnStartedPatching += () =>
            {
                var alterraMiniBathroomKit = new FCSKit(Mod.AlterraMiniBathroomKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                alterraMiniBathroomKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.AlterraMiniBathroomKitClassID.ToTechType(), 75000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };

            _stairShipChair = Resources.Load<GameObject>("Submarine/Build/StarshipChair");

        }

        public override GameObject GetGameObject()
        {
            try
            {
                GameObject starShipChair  = GameObject.Instantiate(_stairShipChair);
                var prefab = GameObject.Instantiate(ModelPrefab.AlterraMiniBathroomPrefab);


                // Scale
                starShipChair.transform.localScale *= 0.5f;
                foreach (Transform tr in starShipChair.transform)
                {
                    tr.localPosition = new Vector3(tr.localPosition.x, tr.localPosition.y + 0.3f, tr.localPosition.z);
                }

                Renderer[] renderers = starShipChair.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = false;
                }

                // Add large world entity
                var lwe = starShipChair.GetComponent<LargeWorldEntity>();
                if (lwe == null)
                    lwe = starShipChair.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Near;


                starShipChair.transform.parent = prefab.transform;
                UWE.Utils.ZeroTransform(starShipChair);
                starShipChair.transform.localPosition = new Vector3(0.80f,-0.30f,0.43f);

                var cb = starShipChair.GetComponentInChildren<ConstructableBounds>();
                cb.bounds.size = Vector3.zero;
                cb.bounds.position = Vector3.zero;
                //GameObject.Destroy(cb);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0.03977585f, 1.502479f, 0.4276283f);
                var size = new Vector3(2.949196f, 1.283838f, 1.457463f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                //GameObjectHelpers.FindGameObject(prefab, "SoundEmitter").AddComponent<FMOD_CustomEmitter>();
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<AlterraMiniBathroomController>();


                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.AlterraMiniBathroomKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
