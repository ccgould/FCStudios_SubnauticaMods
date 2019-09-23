using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.DeepDriller
{
    public partial class BatteryAttachmentBuildable : Craftable
    {


        private Text _label;

        internal static TechType TechTypeID { get; private set; }

        public override CraftTree.Type FabricatorType { get; } =
            FCSTechFabricatorBuildable.TechFabricatorCraftTreeType;

        public override string[] StepsToFabricatorTab { get; } = new[] { "AIS", "DD" };

        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";

        public BatteryAttachmentBuildable() : base("BatteryAttachment_DD", "Deep Driller Battery Attachment", "This specially made attachment allows you to run your deep driller off battery power")
        {
            OnFinishedPatching = () =>
            {
                TechTypeID = this.TechType;
                //Add the new TechType Hand Equipment type
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(QPatch.Kit);

            prefab.name = this.PrefabFileName;

            if (!FindAllComponents(prefab))
            {
                QuickLogger.Error("Failed to get all components");
                return null;
            }

            _label.text = FriendlyName;

            PrefabIdentifier prefabID = prefab.GetOrAddComponent<PrefabIdentifier>();

            prefabID.ClassId = this.ClassID;

            var techTag = prefab.GetOrAddComponent<TechTag>();
            techTag.type = TechType;

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;

        private bool FindAllComponents(GameObject prefab)
        {
            var canvasObject = prefab.GetComponentInChildren<Canvas>().gameObject;
            if (canvasObject == null)
            {
                QuickLogger.Error("Could not find the canvas");
                return false;
            }

            _label = canvasObject.FindChild("Screen").FindChild("Label").GetComponent<Text>();
            return true;
        }
    }
}
