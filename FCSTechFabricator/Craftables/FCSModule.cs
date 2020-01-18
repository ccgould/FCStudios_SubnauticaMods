using System.IO;
using FCSTechFabricator.Components;
using FCSTechFabricator.Configuration;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator.Craftables
{
    public class FCSModule : FcCraftable
    {
        private TechData _ingredients;
        private string _assetFolder = Mod.GetAssetPath();
        private string _iconFileName;
        public FCSModule(string classId, string friendlyName, string description, FcCraftingTab parentTab, TechData ingredients) : base(classId, friendlyName, description, parentTab)
        {
            _ingredients = ingredients;
            OnFinishedPatching += () =>
            {
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.CyclopsModule);
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule));

            prefab.name = this.PrefabFileName;

            prefab.AddComponent<FCSTechFabricatorTag>();

            return prefab;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(_assetFolder, $"{_iconFileName}.png")));
        }

        protected override TechData GetBlueprintRecipe()
        {
            return _ingredients;
        }

        public void ChangeIconLocation(string assetFolder, string iconFileName)
        {
            _assetFolder = assetFolder;
            _iconFileName = iconFileName;
        }

        public override string IconFileName => _iconFileName;
        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => Mod.AssetBundleName;
    }
}
