using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class AntennaController : MonoBehaviour
    {
        internal List<ElectricalBox> _electricalBoxes = new();
        private Text _information;
        private Button _reactiveBTN;
        private MotorHandler _antenna;
        private Image _powerIcon;
        private FCSMessageBox _messageBox;
        private SearchField _numberField;
        private const string _accessCode = "1993";
        private const float SpeedIncrease = 16.666666666666666666666666666667f;
        public void Initialize(AlterraFabricatorStationController controller)
        {
            var electricalBoxes = GameObjectHelpers.FindGameObjects(controller.gameObject, "AlterraHubFabStationElectricalBox",SearchOption.StartsWith);
            var antennaSecurityScreen = GameObjectHelpers.FindGameObject(controller.gameObject, "AntennaSecurityScreen");
            
            for (int i = 0; i < electricalBoxes.Count(); i++)
            {
                var eBox = electricalBoxes.ElementAt(i).gameObject.AddComponent<ElectricalBox>();
                eBox.Initialize(this,i);
                _electricalBoxes.Add(eBox);
            }

            _numberField = GameObjectHelpers.FindGameObject(antennaSecurityScreen, "InputField").AddComponent<SearchField>();
            _numberField.HoverMessage = "Enter Passcode";

            _messageBox = GameObjectHelpers.FindGameObject(antennaSecurityScreen, "MessageBox").AddComponent<FCSMessageBox>();

            _antenna = GameObjectHelpers.FindGameObject(gameObject, "mesh_antenna").EnsureComponent<MotorHandler>();
            _antenna.SetIncreaseRate(5);
            _antenna.Initialize(0);

            _powerIcon = GameObjectHelpers.FindGameObject(antennaSecurityScreen, "Image").GetComponent<Image>();

            _information = GameObjectHelpers.FindGameObject(antennaSecurityScreen, "Text").GetComponent<Text>();
            _reactiveBTN = antennaSecurityScreen.GetComponentInChildren<Button>();
            _reactiveBTN.onClick.AddListener(() =>
            {
                if(!IsPinValid())
                {
                    _messageBox.Show("Valid passcode card required to activate antenna.",FCSMessageButton.OK,null);
                    return;
                }
                Mod.GamePlaySettings.IsPDAUnlocked = true;
                _numberField.gameObject.SetActive(false);
                FCSPDAController.ForceOpen();
            });
            InvokeRepeating(nameof(UpdateScreen),1f,1f);

        }

        private bool IsPinValid()
        {
            var text = _numberField.GetText();
            if (text.Length == 4)
            {
                return text.Equals(_accessCode);
            }

            return false;
        }

        private void UpdateScreen()
        {
            var fixedBoxes = _electricalBoxes.Count(x => x.IsRepaired);
            _information.text = $"Error: Please repair all electrical boxes {fixedBoxes}/{_electricalBoxes.Count} and enter passcode";

            if (fixedBoxes >= _electricalBoxes.Count)
            {
                _reactiveBTN.interactable = true;
            }

            if (Mod.GamePlaySettings.IsPDAUnlocked)
            {
                _reactiveBTN.gameObject.SetActive(false);
                _powerIcon.color = Color.green;
                _information.text = "Connected";
                _information.color = Color.green;
                CancelInvoke(nameof(UpdateScreen));
            }
        }

        public void OnBoxFixed(int id)
        {
            QuickLogger.Debug("Box Fixed",true);

            Mod.GamePlaySettings.FixedPowerBoxes.Add(id);
            OnBoxFixedAction?.Invoke(id);

            if (_electricalBoxes.Any(x => !x.IsRepaired) && LargeWorldStreamer.main.IsWorldSettled())
            {
                VoiceNotificationSystem.main.Play("ElectricalBoxesNeedFixing_key",
                    $"Further electrical boxes in need of repair {_electricalBoxes.Count(x => x.IsRepaired)}/{_electricalBoxes.Count}");
            }


            if (_electricalBoxes.All(x => x.IsRepaired))
            {
                _antenna.RPMByPass(66.666f);
            }

        }

        public Action<int> OnBoxFixedAction { get; set; }

        public void LoadSave()
        {
            foreach (int boxID in Mod.GamePlaySettings.FixedPowerBoxes)
            {
                var electricalBox = _electricalBoxes.FirstOrDefault(x => x.Id == boxID);
                electricalBox?.Fix(true);
            }
        }
    }
}