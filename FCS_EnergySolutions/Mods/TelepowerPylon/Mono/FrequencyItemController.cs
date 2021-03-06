using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class FrequencyItemController : MonoBehaviour
    {
        public TelepowerPylonController TargetController { get; private set; }
        public TelepowerPylonController ParentController { get; private set; }
        private Text _text;

        internal void Initialize(TelepowerPylonController targetController, TelepowerPylonController parent)
        {
            TargetController = targetController;
            ParentController = parent;
            _text = gameObject.GetComponentInChildren<Text>();
            var delBTN = gameObject.GetComponentInChildren<Button>();
            delBTN.onClick.AddListener(() =>
            {
                if (ParentController.GetCurrentMode() == TelepowerPylonMode.PULL)
                {
                    ParentController.DeleteFrequencyItemAndDisconnectRelay(TargetController.UnitID);
                    TargetController.DeleteFrequencyItemAndDisconnectRelay(ParentController.UnitID);
                }
                else
                {
                    TargetController.DeleteFrequencyItemAndDisconnectRelay(ParentController.UnitID);
                    ParentController.DeleteFrequencyItemAndDisconnectRelay(TargetController.UnitID);
                }

                Destroy(gameObject);
            });
            _text.text = $"Unit ID : {targetController.UnitID}";
        }
    }
}