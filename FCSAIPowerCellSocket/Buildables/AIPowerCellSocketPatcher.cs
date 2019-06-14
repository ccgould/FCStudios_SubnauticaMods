using FCSAIPowerCellSocket.Mono;
using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.IO;
using UnityEngine;

namespace FCSAIPowerCellSocket.Buildables
{
    internal partial class AIPowerCellSocketBuildable : Buildable
    {
        private static readonly AIPowerCellSocketBuildable Singleton = new AIPowerCellSocketBuildable();
        public override string AssetsFolder { get; } = $"FCSAIPowerCellSocket/Assets";
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public AIPowerCellSocketBuildable() : base("AIPowerCellSocket", "Alterra Industrial Powercell Socket", "Alterra Industrial wall mounted powercell socket for emergency power")
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

            //Add the prefabIdentifier
            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = this.ClassID;

            prefab.GetOrAddComponent<AIPowerCellSocketController>();

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                Ingredients =
                {
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Glass, 1)
                }
            };
        }
    }
}
