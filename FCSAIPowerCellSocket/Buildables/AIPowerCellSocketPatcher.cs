using System;
using FCSAIPowerCellSocket.Mono;
using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.IO;
using FCSAIPowerCellSocket.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSAIPowerCellSocket.Buildables
{
    internal partial class AIPowerCellSocketBuildable : Buildable
    {
        private static readonly AIPowerCellSocketBuildable Singleton = new AIPowerCellSocketBuildable();

        public override string IconFileName => "FCSAIPowerCellSocket.png";
        public override string AssetsFolder => Mod.GetAssetPath();
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public AIPowerCellSocketBuildable() : base(Mod.ClassID, Mod.ModFriendlyName, Mod.ModDescription)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }
            
            Singleton.Patch();
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_Prefab);
                GameObject consoleModel = prefab.FindChild("model");

                // Update sky applier
                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = consoleModel.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //Add the constructable component to the prefab
                Constructable constructable = prefab.AddComponent<Constructable>();

                constructable.allowedInBase = true; // Only allowed in Base
                constructable.allowedInSub = false; // Not allowed in Cyclops
                constructable.allowedOutside = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOnGround = false; // Only on ground
                constructable.allowedOnWall = true;
                constructable.allowedOnConstructables = false;
                constructable.controlModelState = true;
                constructable.rotationEnabled = false;
                constructable.techType = this.TechType;
                constructable.model = consoleModel;

                var center = new Vector3(-0.006649137f,0f, 0.1839597f);
                var size = new Vector3(2.706617f, 1.698831f, 0.3483825f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

                //Add the prefabIdentifier
                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = this.ClassID;
                prefab.EnsureComponent<AIPowerCellSocketAnimator>();
                prefab.EnsureComponent<AIPowerCellSocketPowerManager>();
                prefab.EnsureComponent<AIPowerCellSocketController>();

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error<AIPowerCellSocketBuildable>(e.Message);
                return null;
            }
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            
            return new TechData
            {
                Ingredients =
                {
                    new Ingredient(Mod.PowercellSocketId.ToTechType(), 1)
                }
            };
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {

            return new RecipeData
            {
                Ingredients =
                {
                    new Ingredient(Mod.PowercellSocketId.ToTechType(), 1)
                }
            };
        }
#endif
    }
}
