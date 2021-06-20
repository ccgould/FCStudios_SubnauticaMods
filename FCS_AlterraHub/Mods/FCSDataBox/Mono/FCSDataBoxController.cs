using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using Story;
using UnityEngine;

namespace FCS_AlterraHub.Mods.FCSDataBox.Mono
{
    internal class FCSDataBoxController : MonoBehaviour
    {
        private BlueprintHandTarget _bluePrintDataBox;
        internal TechType UnlockTechType => Mod.OreConsumerFragmentTechType;

        private void Start()
        {
            gameObject.SetActive(false);
            _bluePrintDataBox = gameObject.EnsureComponent<BlueprintHandTarget>();
            _bluePrintDataBox.animator = gameObject.GetComponent<Animator>();
            _bluePrintDataBox.animParam = "databox_take";
            _bluePrintDataBox.viewAnimParam = "databox_lookat";
            _bluePrintDataBox.unlockTechType = UnlockTechType;
            _bluePrintDataBox.useSound = QPatch.BoxOpenSoundAsset;
            _bluePrintDataBox.disableGameObject = GameObjectHelpers.FindGameObject(gameObject, "BLUEPRINT_DATA_DISC");
            _bluePrintDataBox.inspectPrefab = AlterraHub.BluePrintDataDiscPrefab;
            _bluePrintDataBox.onUseGoal = new StoryGoal(String.Empty, Story.GoalType.PDA, 0);
            gameObject.SetActive(true);
            var genericHandTarget = gameObject.GetComponent<GenericHandTarget>();
            genericHandTarget.onHandHover.AddListener(_ => _bluePrintDataBox.HoverBlueprint());
            genericHandTarget.onHandClick.AddListener(_ => _bluePrintDataBox.UnlockBlueprint());
        }

        internal void Initialize()
        {

        }
    }
}
