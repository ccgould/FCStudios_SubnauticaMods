using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Abstract_Classes;
using FCSTechFabricator.Helpers;
using FCSTechFabricator.Mono;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCSTechFabricator
{
    internal class ModuleCraftable : TechFabCraftable
    {
        public ModuleCraftable(string classId, string friendlyName, string description, string icon, bool useEqupimentSlot = true, 
            EquipmentType equipmentType = EquipmentType.CyclopsModule, TechType requiredForUnlock = TechType.None) :
            base(classId, friendlyName, description, useEqupimentSlot, equipmentType, requiredForUnlock)
        {
            IconFileName = string.IsNullOrEmpty(icon) ? $"{classId}.png" : icon;

        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule));

            prefab.name = this.PrefabFileName;

            prefab.AddComponent<FCSTechFabricatorTag>();

            return prefab;
        }

        public override string[] StepsToFabricatorTab { get; }
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override TechType TechTypeID { get; set; }
        public override string IconFileName { get; }

        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }

        public override GameObject OriginalPrefab { get; set; }

    }
}
