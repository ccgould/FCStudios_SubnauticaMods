using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono
{
    internal class KitCraftable : TechFabCraftable
    {
        private Text _label;

        public override GameObject OriginalPrefab { get; set; } = QPatch.Kit;
        public override string IconFileName { get;}
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; }
        public override TechType TechTypeID { get; set; }

        public KitCraftable(string classId, string friendlyName, string description, string icon, string[] stepsToFabricatorTab) : 
            base(classId, friendlyName, description)
        {
            StepsToFabricatorTab = stepsToFabricatorTab;
            IconFileName = string.IsNullOrEmpty(icon) ? "Kit_FCS.png" : icon;
        }

        public override void GetGameObjectExt(GameObject instantiatedPrefab)
        {
            FindAllComponents(instantiatedPrefab);
        }

        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }

        private bool FindAllComponents(GameObject prefab)
        {
            var canvasObject = prefab.GetComponentInChildren<Canvas>().gameObject;
            if (canvasObject == null)
            {
                QuickLogger.Error("Could not find the canvas");
                return false;
            }

            _label = canvasObject.FindChild("Screen").FindChild("Label").GetComponent<Text>();
            _label.text = FriendlyName;
            return true;
        }
    }
}
