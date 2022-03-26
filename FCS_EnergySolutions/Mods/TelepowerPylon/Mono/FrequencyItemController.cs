using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class FrequencyItemController : MonoBehaviour
    {
        public BaseTelepowerPylonManager TargetController { get; private set; }
        public BaseTelepowerPylonManager ParentController { get; private set; }
        private Text _text;
        private Toggle _toggleBtn;

        //TODO The Push mode base toggle is checking because it has the connections but a pull wouldnt

        internal void Initialize(BaseTelepowerPylonManager targetController, BaseTelepowerPylonManager parent)
        {
            TargetController = targetController;
            ParentController = parent;
            _text = gameObject.GetComponentInChildren<Text>();
            _toggleBtn = gameObject.GetComponentInChildren<Toggle>();
            //_toggleBtn.SetIsOnWithoutNotify(isChecked); //TODO Find a way to handle this
            _toggleBtn.onValueChanged.AddListener((value =>
            {
                if (value)
                {
                    QuickLogger.Debug($"Trying to Enable pull mode: {ParentController.GetCurrentMode()}");

                    if (ParentController.GetCurrentMode() == TelepowerPylonMode.PULL)
                    {
                        if (TargetController.IsConnectionAllowed(ParentController))
                        {
                            ParentController.AddConnection(TargetController);
                            //We need to tell the base to check the item
                            //TargetController.ActivateItemOnPushGrid(ParentController);

                        }
                        else
                        {
                            QuickLogger.Debug("Loop Detection",true);
                        }
                    }
                    else
                    {
                        if (ParentController.IsConnectionAllowed(TargetController))
                        {
                            //TargetController.ActivateItemOnPullGrid(ParentController);
                            TargetController.AddConnection(ParentController,true);
                        }
                        else
                        {
                            QuickLogger.Debug("Loop Detection", true);
                        }
                    }
                }
                else
                {
                    if (ParentController.GetCurrentMode() == TelepowerPylonMode.PULL)
                    {
                        ParentController.RemoveConnection(TargetController);
                    }
                    else
                    {
                        TargetController.RemoveConnection(ParentController, true);
                    }
                }
            }));

            InvokeRepeating(nameof(RefreshBaseName),1f,1f);
            InvokeRepeating(nameof(RefreshToggleState),1f,1f);
        }

        private void RefreshBaseName()
        {
            _text.text = $"Base Name : {TargetController.GetBaseName()}";
        }

        private void RefreshToggleState()
        {
            _toggleBtn.SetIsOnWithoutNotify(TargetController.GetIsConnected(ParentController.GetBaseID()));
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