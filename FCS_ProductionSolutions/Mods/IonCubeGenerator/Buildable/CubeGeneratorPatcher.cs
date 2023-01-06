using System.IO;
using System.Reflection;
using FCS_ProductionSolutions.Mods.IonCubeGenerator.Craftables;
using FCS_ProductionSolutions.Mods.IonCubeGenerator.Display;
using FCS_ProductionSolutions.Mods.IonCubeGenerator.Display.Patching;
using FCS_ProductionSolutions.Mods.IonCubeGenerator.Mono;
using SMLHelper.Crafting;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.IonCubeGenerator.Buildable
{
    // using Logger = QModManager.Utility.Logger;
#if SUBNAUTICA
    using RecipeData = SMLHelper.Crafting.TechData;
#endif

    internal partial class CubeGeneratorBuildable : SMLHelper.Assets.Buildable
    {
        private static readonly CubeGeneratorBuildable singleton = new CubeGeneratorBuildable();

        public static void PatchSMLHelper()
        {
            if (!singleton.GetPrefabs())
            {
                throw new FileNotFoundException("Failed to retrieve the IonCubeGenerator prefab from the asset bundle");
            }

            var alienIngot = new AlienIngot();

            if (!alienIngot.GetPrefabs(singleton._assetBundle))
            {
                throw new FileNotFoundException("Failed to retrieve the Alien Ingot Case prefab from the asset bundle");
            }

            alienIngot.Patch();

            var alienCase = new AlienEletronicsCase();

            if (!alienCase.GetPrefabs(singleton._assetBundle))
            {
                throw new FileNotFoundException("Failed to retrieve the Alien Eletronics Case prefab from the asset bundle");
            }

            alienCase.Patch();

            singleton.Patch();

        }

        public CubeGeneratorBuildable()
            : base("CubeGenerator", "Ion Cube Generator", "Condenses large quantities of power into Ion Cubes")
        {
            OnFinishedPatching += AdditionalPatching;
            OnFinishedPatching += DisplayLanguagePatching.AdditionPatching;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorPieces;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorPiece;
        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");
        public override TechType RequiredForUnlock { get; } = TechType.PrecursorPrisonIonGenerator;

        public override GameObject GetGameObject()
        {
            var prefab = GameObject.Instantiate(_ionCubeGenPrefab);
            GameObject consoleModel = prefab.FindChild("model");

            // Update sky applier
            SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
            skyApplier.renderers = consoleModel.GetComponentsInChildren<MeshRenderer>();
            skyApplier.anchorSky = Skies.Auto;

            //Add the constructible component to the prefab
            Constructable constructible = prefab.AddComponent<Constructable>();
            
            constructible.allowedInBase = true; // Only allowed in Base
            constructible.allowedInSub = false; // Not allowed in Cyclops
            constructible.allowedOutside = false;
            constructible.allowedOnCeiling = false;
            constructible.allowedOnGround = true; // Only on ground
            constructible.allowedOnWall = false;
            constructible.allowedOnConstructables = false;
            constructible.controlModelState = true;
            constructible.rotationEnabled = true;
            constructible.techType = this.TechType;
            constructible.model = consoleModel;

            //Add the prefabIdentifier
            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = this.ClassID;

            // Add the custom component
            CubeGeneratorMono cubeGenerator = prefab.AddComponent<CubeGeneratorMono>(); // Moved to the bottom to allow constructible to be added

            CubeGeneratorAnimator cubeGeneratorAnimator = prefab.AddComponent<CubeGeneratorAnimator>();

            IonGeneratorDisplay cubeGeneratorDisplay = prefab.AddComponent<IonGeneratorDisplay>();

            FMOD_CustomLoopingEmitter FMOD_CustomLoopingEmitter = prefab.AddComponent<FMOD_CustomLoopingEmitter>();

            return prefab;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
                Ingredients =
                {
                    new Ingredient(AlienIngot.TechTypeID, 2),
                    new Ingredient(AlienEletronicsCase.TechTypeID, 1),

                    new Ingredient(TechType.Glass, 1),
                    new Ingredient(TechType.Lubricant, 1),
                    new Ingredient(TechType.Kyanite, 2),
                }
            };
        }
    }
}
