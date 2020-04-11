using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCSTechFabricator.Components;
using FCSTechFabricator.Configuration;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator.Craftables
{
    public class FCSVehicleModule : FcEquipable
    {
        private EquipmentType _equipmentType;
        private TechData _ingredients;
        private string _iconFileName;
        private string _assetFolder;

        public FCSVehicleModule(string classId, string friendlyName, string description, EquipmentType equipmentType, TechData ingredients, FcCraftingTab parentTab) : base(classId, friendlyName, description, parentTab)
        {
            _equipmentType = equipmentType;
            _ingredients = ingredients;
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule));

            prefab.name = this.PrefabFileName;

            prefab.AddComponent<FCSTechFabricatorTag>();

            return prefab;
        }

#if SUBNAUTICA
        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{_iconFileName}.png"));
        }

        protected override TechData GetBlueprintRecipe()
        {
            return _ingredients;
        }

#elif BELOWZERO
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{_iconFileName}.png"));
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return _ingredients;
        }

#endif

        public void ChangeIconLocation(string assetFolder, string iconFileName)
        {
            _assetFolder = assetFolder;
            _iconFileName = iconFileName;
        }

        public override string IconFileName => _iconFileName;

        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
        public override EquipmentType EquipmentType => _equipmentType;
        protected override string AssetBundleName => Mod.AssetBundleName;
    }
}
