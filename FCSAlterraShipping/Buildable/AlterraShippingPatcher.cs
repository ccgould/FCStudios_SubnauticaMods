using FCSAlterraShipping.Display;
using FCSAlterraShipping.Display.Patching;
using FCSAlterraShipping.Models;
using FCSAlterraShipping.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System.IO;

namespace FCSAlterraShipping.Buildable
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using UnityEngine;

    internal partial class AlterraShippingBuildable : Buildable
    {
        private static readonly AlterraShippingBuildable Singleton = new AlterraShippingBuildable();

        #region Public Properties
        public override string AssetsFolder { get; } = $"FCSAlterraShipping/Assets";
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        #endregion

        public static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();
        }

        public AlterraShippingBuildable() :
            base("FCSAlterraShipping", "Alterra Shipping", "Shipping all your parcels.")
        {
            OnFinishedPatching += AdditionalPatching;
            OnFinishedPatching += DisplayLanguagePatching.AdditionPatching;
        }

        public override GameObject GetGameObject()
        {
            SubRoot currentSub = Player.main.currentSub;


            if (currentSub != null)
            {
                QuickLogger.Debug($"Current Sub = {currentSub.GetInstanceID()}", true);

                var manager = ShippingTargetManager.FindManager(currentSub);

                QuickLogger.Debug($"Manager {manager}", true);

                if (manager != null && manager.ShippingTargets.Count >= ShippingTargetManager.MaxShippingContainers)
                {
                    ErrorMessage.AddMessage(Language.main.Get(OverLimitKey));
                    return null;
                }
            }
            else
            {
                QuickLogger.Debug($"Current Sub is null");

            }
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
            constructable.allowedOnGround = true; // Only on ground
            constructable.allowedOnWall = false;
            constructable.allowedOnConstructables = false;
            constructable.controlModelState = true;
            constructable.rotationEnabled = true;
            constructable.techType = this.TechType;
            constructable.model = consoleModel;

            CreateDisplayedContainer(prefab);

            prefab.GetOrAddComponent<AlterraShippingTransferHandler>();

            prefab.GetOrAddComponent<AlterraShippingAnimator>();

            prefab.GetOrAddComponent<AlterraShippingTarget>();

            //Add the prefabIdentifier
            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = this.ClassID;

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
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.VehicleStorageModule, 1),
                    new Ingredient(TechType.Beacon, 1),
                    new Ingredient(TechType.Glass, 1)
                }
            };
        }

        private void CreateDisplayedContainer(GameObject prefab)
        {
            GameObject container = prefab.FindChild("model")
                .FindChild("mesh_body")
                .FindChild("cargo_container")?.gameObject;

            if (container != null)
            {
                QuickLogger.Debug("Container Display Object Created", true);
                var cargoBox = Resources.Load<GameObject>("WorldEntities/Doodads/Debris/Wrecks/Decoration/Starship_cargo");

                if (cargoBox == null)
                {
                    QuickLogger.Debug("Cargo Box could not be found.");
                    return;
                }

                var cargoBoxModel = GameObject.Instantiate<GameObject>(cargoBox);

                GameObjectHelpers.DestroyComponent(cargoBoxModel);

                cargoBoxModel.transform.SetParent(container.transform);
                cargoBoxModel.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                cargoBoxModel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                cargoBoxModel.transform.Rotate(new Vector3(0, 90, 0));
            }
            else
            {
                QuickLogger.Error("Cannot Find cargo_container in the prefab");
            }
        }
    }
}
