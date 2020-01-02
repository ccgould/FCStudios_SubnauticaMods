using System;
using FCSCommon.Utilities;
using FlexibleTechFabricator.Configuration;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace FlexibleTechFabricator.Craftables
{
    public class FCSKit : FcCraftable
    {
        private Text _label;

        public FCSKit(string classId, string friendlyName, FcCraftingTab parentTab) : 
            base(classId, friendlyName, $"A kit that allows you to build one {friendlyName} Unit", parentTab)
        {
        }

        public override GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

        protected override TechData GetBlueprintRecipe()
        {
            throw new NotImplementedException();
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

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => "fcstechfabricatormodbundle";
        public override string IconFileName => "Kit_FCS.png";
        public override string AssetsFolder => $"{Mod.ModFolderName}/Assets";
    }
}
