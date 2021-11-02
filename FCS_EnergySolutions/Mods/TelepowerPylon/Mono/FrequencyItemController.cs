using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class FrequencyItemController : MonoBehaviour
    {
        public ITelepowerPylonConnection TargetController { get; private set; }
        public ITelepowerPylonConnection ParentController { get; private set; }
        private Text _text;
        private Toggle _toggleBtn;

        internal void Initialize(ITelepowerPylonConnection targetController, ITelepowerPylonConnection parent, bool isChecked = false)
        {
            TargetController = targetController;
            ParentController = parent;
            _text = gameObject.GetComponentInChildren<Text>();
            _toggleBtn = gameObject.GetComponentInChildren<Toggle>();
            _toggleBtn.SetIsOnWithoutNotify(isChecked);
            _toggleBtn.onValueChanged.AddListener((value =>
            {
                if (value)
                {
                    QuickLogger.Debug($"Trying to Enable pull mode: {ParentController.GetCurrentMode()}");
                    if (ParentController.GetCurrentMode() == TelepowerPylonMode.PULL)
                    {
                        TargetController.AddItemToPushGrid(ParentController, true);
                        ParentController.GetPowerManager().AddConnection(TargetController);
                    }
                }
                else
                {
                    ParentController.DeleteFrequencyItemAndDisconnectRelay(TargetController.UnitID);
                    TargetController.DeleteFrequencyItemAndDisconnectRelay(ParentController.UnitID);
                }
            }));

            _text.text = $"Unit ID : {targetController.UnitID}";
        }

        public void UnCheck(bool notify = false)
        {
            if(_toggleBtn == null) return;

            if (notify)
            {
                _toggleBtn.isOn = false;
            }
            else
            {
                _toggleBtn?.SetIsOnWithoutNotify(false);
            }
        }

        public void Check(bool notify = false)
        {
            if (_toggleBtn == null) return;

            if (notify)
            {
                _toggleBtn.isOn = true;
            }
            else
            {
                _toggleBtn?.SetIsOnWithoutNotify(true);
            }
        }

        public bool IsChecked()
        {
            return _toggleBtn != null && _toggleBtn.isOn;
        }
    }
}