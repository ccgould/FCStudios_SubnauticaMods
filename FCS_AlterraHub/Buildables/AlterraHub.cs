using System;
using System.Resources;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.AlterraHub;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_AlterraHub.Buildables
{
    public partial class AlterraHub : Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public AlterraHub() : base(Mod.AlterraHubClassID, Mod.AlterraHubFriendly, Mod.AlterraHubDescription)
        { 
            OnFinishedPatching += AdditionalPatching;
        }

        public override GameObject GetGameObject()
        {
            try
            {
                    var prefab = GameObject.Instantiate(AlterraHubPrefab);
                    var center = new Vector3(0.006554961f, 0.698558f, 0.00327754f);
                    var size = new Vector3(1.353966f, 1.11104f, 1.006555f);

                    GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                    var model = prefab.FindChild("model");

                    //========== Allows the building animation and material colors ==========// 
                    Shader shader = Shader.Find("MarmosetUBER");
                    Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                    SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                    skyApplier.renderers = renderers;
                    skyApplier.anchorSky = Skies.Auto;
                    //========== Allows the building animation and material colors ==========// 

                    // Add constructible
                    var constructable = prefab.AddComponent<Constructable>();

                    constructable.allowedOutside = false;
                    constructable.allowedInBase = true;
                    constructable.allowedOnGround = true;
                    constructable.allowedOnWall = false;
                    constructable.rotationEnabled = true;
                    constructable.allowedOnCeiling = false;
                    constructable.allowedInSub = true;
                    constructable.allowedOnConstructables = false;
                    constructable.model = model;
                    constructable.techType = TechType;
                    
                    PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                    prefabID.ClassId = ClassID;

                    prefab.AddComponent<TechTag>().type = TechType;
                    prefab.AddComponent<AlterraHubController>();
                    prefab.AddComponent<FCSGameLoadUtil>();

                    //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                    MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
                    return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }


#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            return Mod.AlterraHubIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.AlterraHubIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
#endif
    }
}