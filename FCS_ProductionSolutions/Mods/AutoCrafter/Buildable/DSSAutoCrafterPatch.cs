using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable
{
    internal class DSSAutoCrafterPatch : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        public override string AssetsFolder => Mod.GetAssetPath();

        public DSSAutoCrafterPatch() : base(Mod.DSSAutoCrafterClassName, Mod.DSSAutoCrafterFriendlyName, Mod.DSSAutoCrafterDescription)
        {
            OnFinishedPatching += () =>
            {
                var dssAutoCrafterKit = new FCSKit(Mod.DSSAutoCrafterKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                dssAutoCrafterKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.DSSAutoCrafterKitClassID.ToTechType(), 236250, StoreCategory.Production);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.DSSAutoCrafterPrefab);

                var center = new Vector3(0.112175f, 1.279794f, 0.03599018f);
                var size = new Vector3(2.570427f, 2.437856f, 1.906985f);


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

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DSSAutoCrafterController>();

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
                MaterialHelpers.ApplyShaderToMaterial(prefab, "DSS_ConveyorBelt");

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
            return Mod.DSSAutoCrafterIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.DSSAutoCrafterIngredients;
        }
#endif
    }
}
