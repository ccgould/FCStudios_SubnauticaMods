using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_AlterraHub.ModItems.TestObject.Mono;
using FCSCommon.Helpers;
using SMLHelper.Crafting;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Buildable
{
    internal class DroneDepotPortBuildable : FCSBuildableModBase
    {
        private TechType _kitTechType;

        public DroneDepotPortBuildable() : base(Main.MODNAME, "fcsDepotDronePort", FileSystemHelper.ModDirLocation, "DepotDronePort", "Drone Port")
        {
            OnStartedPatching += () =>
            {
                var kit = new FCSKit(ClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                kit.PatchSMLHelper();
                _kitTechType = kit.TechType;
            };

            OnFinishedPatching += () =>
            {
                PatchedTechType = TechType;
                //FCSPDAController.AddAdditionalPage<uGUI_IonCube>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_Ioncube", bundleName, FileSystemHelper.ModDirLocation, false));
                FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Misc);
            };
        }

        public static TechType PatchedTechType { get; private set; }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            yield return base.GetGameObjectAsync(gameObject);

            Prefab.AddComponent<TestController>();

            yield break;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients =
                {      
                new Ingredient(TechType.Glass, 2),
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.ComputerChip, 1)
                }
            };
        }
    }
}
