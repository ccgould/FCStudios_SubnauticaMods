using System;
using System.Collections;
using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Abstract;
using FCSAlterraIndustrialSolutions.Models.Buttons;
using FCSAlterraIndustrialSolutions.Models.Enums;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    public class DeepDrillerDisplay : AIDisplay
    {
        #region Private Members
        private GameObject _canvasGameObject;
        private GameObject _powerBTN;
        private GameObject _openStorageBTN;
        private GameObject _batteryMeter;
        private GameObject _openModuleDoor;

        #endregion

        #region Public Properties
        public DeepDrillerController Controller { get; set; }
        #endregion

        #region Public Methods
        public void Setup(DeepDrillerController deepDrillerController)
        {
            Controller = deepDrillerController;

            if (FindAllComponents() == false)
            {
                Log.Error("// ============== Error getting all Components ============== //");
                ShutDownDisplay();
                return;
            }

        }

        public override void ChangePageBy(int amountToChangePageBy)
        {
            throw new System.NotImplementedException();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "Open_BTN":
                    Controller.OpenStorage();
                    break;

                case "PowerBTN":
                    switch (Controller.PowerState)
                    {
                        case PowerState.On:
                            Controller.PowerOffDrill();
                            Controller.StopDrill();
                            break;
                        case PowerState.None:
                            Controller.PowerOnDrill();

                            break;
                        case PowerState.StandBy:
                            break;
                        case PowerState.Off:
                            Controller.PowerOnDrill();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case "Module_Door":
                    Controller.OpenModulesDoor();
                    break;
            }
        }

        public override void ItemModified<T>(T item)
        {
            throw new System.NotImplementedException();
        }

        public override bool FindAllComponents()
        {
            #region Canvas
            _canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;
            if (_canvasGameObject == null)
            {
                Log.Error("Canvas not found.");
                return false;
            }
            #endregion

            #region Power Button

            _powerBTN = _canvasGameObject.FindChild("PowerBTN")?.gameObject;

            if (_powerBTN == null)
            {
                Log.Error("Power Button not found.");
                return false;
            }

            var powerBTN = _powerBTN.AddComponent<InterfaceButton>();
            powerBTN.Display = this;
            powerBTN.BtnName = "PowerBTN";
            powerBTN.ButtonMode = InterfaceButtonMode.Background;
            powerBTN.TextLineOne = $"Toggle {Information.DeepDrillerFriendly} Power";
            powerBTN.Tag = this;
            #endregion

            #region Open Storage Button

            _openStorageBTN = _canvasGameObject.FindChild("Open_BTN")?.gameObject;

            if (_openStorageBTN == null)
            {
                Log.Error("Open Storage Button not found.");
                return false;
            }

            var openStorageBTN = _openStorageBTN.AddComponent<InterfaceButton>();
            openStorageBTN.Display = this;
            openStorageBTN.BtnName = "Open_BTN";
            openStorageBTN.ButtonMode = InterfaceButtonMode.None;
            openStorageBTN.TextLineOne = $"Open {Information.DeepDrillerFriendly} Storage";
            openStorageBTN.Tag = this;

            #endregion

            #region Open Modules Button

            _openModuleDoor = _canvasGameObject.FindChild("moduleDoor")?.gameObject;

            if (_openModuleDoor == null)
            {
                Log.Error("Open module door not found.");
                return false;
            }

            var openModuleDoor = _openModuleDoor.AddComponent<InterfaceButton>();
            openModuleDoor.Display = this;
            openModuleDoor.BtnName = "Module_Door";
            openModuleDoor.ButtonMode = InterfaceButtonMode.None;
            openModuleDoor.TextLineOne = $"Open {Information.DeepDrillerFriendly} Modulars";
            openModuleDoor.Tag = this;

            #endregion

            #region Battery

            _batteryMeter = _canvasGameObject.FindChild("Battery")?.gameObject;

            if (_batteryMeter == null)
            {
                Log.Error("Open Storage Button not found.");
                return false;
            }

            #endregion

            return true;
        }

        public void TurnDisplayOff()
        {

        } 
        #endregion

        #region IEnumerators
        public override IEnumerator PowerOff()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator ShutDown()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator CompleteSetup()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
