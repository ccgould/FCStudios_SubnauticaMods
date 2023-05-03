using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using UnityEngine;
using Ingredient = CraftData.Ingredient;

namespace FCS_AlterraHub.Items.Equipment
{
    public static class YeetKnifePrefab
    {
        public static PrefabInfo Info { get; } = PrefabInfo
            .WithTechType("YeetKnife", "Yeet Knife", "Powerful knife that makes me go yes.")
            .WithIcon(SpriteManager.Get(TechType.HeatBlade));

        public static void Register()
        {
            var customPrefab = new CustomPrefab(Info);

            var yeetKnifeObj = new CloneTemplate(Info, TechType.HeatBlade);
            yeetKnifeObj.ModifyPrefab += obj =>
            {
                var heatBlade = obj.GetComponent<HeatBlade>();
                var yeetKnife = obj.AddComponent<YeetKnife>().CopyComponent(heatBlade);
                Object.DestroyImmediate(heatBlade);
                yeetKnife.damage *= 2f;
            };
            customPrefab.SetGameObject(yeetKnifeObj);
            customPrefab.SetRecipe(new RecipeData(new Ingredient(TechType.Titanium, 2)));
            customPrefab.SetEquipment(EquipmentType.Hand);
            customPrefab.Register();
        }
    }

    public class YeetKnife : HeatBlade
    {
        public float hitForce = 1669;
        public ForceMode forceMode = ForceMode.Acceleration;

        public override string animToolName { get; } = TechType.HeatBlade.AsString(true);

        public override void OnToolUseAnim(GUIHand hand)
        {
            base.OnToolUseAnim(hand);

            GameObject hitObj = null;
            Vector3 hitPosition = default;
            UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, attackDist, ref hitObj, ref hitPosition);
            if (!hitObj)
            {
                // Nothing is in our attack range. Exit method.
                return;
            }

            var liveMixin = hitObj.GetComponentInParent<LiveMixin>();
            if (liveMixin && IsValidTarget(liveMixin))
            {
                var rigidbody = hitObj.GetComponentInParent<Rigidbody>();

                if (rigidbody)
                {
                    rigidbody.AddForce(MainCamera.camera.transform.forward * hitForce, forceMode);
                }
            }
        }
    }
}