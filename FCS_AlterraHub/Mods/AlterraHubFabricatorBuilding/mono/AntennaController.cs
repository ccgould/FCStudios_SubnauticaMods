using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
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
        private bool _pinValid;
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

            _antenna = gameObject.EnsureComponent<MotorHandler>();
            _antenna.SetIncreaseRate(5);
            _antenna.Initialize(0);

            _powerIcon = GameObjectHelpers.FindGameObject(antennaSecurityScreen, "Image").GetComponent<Image>();

            _information = GameObjectHelpers.FindGameObject(antennaSecurityScreen, "Text").GetComponent<Text>();

            _reactiveBTN = antennaSecurityScreen.GetComponentInChildren<Button>();
            _reactiveBTN.onClick.AddListener(() =>
            {
                if (!IsPinValid())
                {
                    _messageBox.Show(AlterraHub.AntennaPinNeededMessage(), FCSMessageButton.OK, null);
                    return;
                }

                _pinValid = true;
                _numberField.gameObject.SetActive(false);
                AlterraFabricatorStationController.Main.UpdateBeaconState(false);
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
            _information.text = $"Error: Please repair all electrical boxes {fixedBoxes}/{_electricalBoxes.Count} and enter 4 digit pin";

            if (fixedBoxes >= _electricalBoxes.Count)
            {
                _reactiveBTN.interactable = true;
            }

            if (AlterraFabricatorStationController.Main.DetermineIfUnlocked())
            {
                _numberField.gameObject.SetActive(false);
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

            OnBoxFixedAction?.Invoke(id);

            if (_electricalBoxes.Any(x => !x.IsRepaired) && LargeWorldStreamer.main.IsWorldSettled())
            {
                VoiceNotificationSystem.main.Play("ElectricalBoxesNeedFixing_key",
                    $"Further electrical boxes in need of repair {_electricalBoxes.Count(x => x.IsRepaired)}/{_electricalBoxes.Count}");
            }
            
            if (IsAllElectricalBoxesFixed())
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

            _pinValid = Mod.GamePlaySettings.IsPDAUnlocked;
        }

        public IEnumerable<int> Save()
        {
            foreach (ElectricalBox electricalBox in _electricalBoxes)
            {
                if(!electricalBox.IsRepaired) continue;
                yield return electricalBox.Id;
            }
        }

        public bool IsAllElectricalBoxesFixed()
        {
            return _electricalBoxes.All(x => x.IsRepaired);
        }

        public bool IsUnlocked()
        {
            return _pinValid;
        }

        public void CompleteObjective()
        {
            foreach (ElectricalBox electricalBox in _electricalBoxes)
            {
                electricalBox.Fix(true);
            }
        }

        public void MakeDirty()
        {
            foreach (ElectricalBox electricalBox in _electricalBoxes)
            {
                electricalBox.MakeDirty();
            }

            _antenna.RPMByPass(0f);
        }
    }
}