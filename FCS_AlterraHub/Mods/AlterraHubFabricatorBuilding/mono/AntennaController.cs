using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.OreConsumer.Model;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class AntennaController : MonoBehaviour
    {
        internal List<ElectricalBox> _electricalBoxes = new List<ElectricalBox>();
        private Text _information;
        private Button _reactiveBTN;
        private MotorHandler _antenna;
        private Image _powerIcon;
        private FCSMessageBox _messageBox;
        private const float SpeedIncrease = 16.666666666666666666666666666667f;
        public void Initialize(AlterraFabricatorStationController fabricatorStationController)
        {
            var electricalBoxes = GameObjectHelpers.FindGameObjects(gameObject, "AlterraHubFabStationElectricalBox");
            for (int i = 0; i < electricalBoxes.Count(); i++)
            {
                var eBox = electricalBoxes.ElementAt(i).gameObject.AddComponent<ElectricalBox>();
                eBox.Initialize(this,i);
                _electricalBoxes.Add(eBox);
            }

            _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();

            _antenna = GameObjectHelpers.FindGameObject(gameObject, "mesh_antenna").EnsureComponent<MotorHandler>();
            _antenna.SetIncreaseRate(5);
            _antenna.Initialize(0);

            _powerIcon = GameObjectHelpers.FindGameObject(gameObject, "Image").GetComponent<Image>();

            _information = GameObjectHelpers.FindGameObject(gameObject, "Text").GetComponent<Text>();
            _reactiveBTN = gameObject.GetComponentInChildren<Button>();
            _reactiveBTN.onClick.AddListener(() =>
            {
                if(!PlayerInteractionHelper.HasItem(Mod.StaffKeyCardTechType))
                {
                    _messageBox.Show("Staff key card required to activate antenna.",FCSMessageButton.OK,null);
                    return;
                }
                Mod.GamePlaySettings.IsPDAUnlocked = true;
                FCSPDAController.ForceOpen();
            });
            InvokeRepeating(nameof(UpdateScreen),1f,1f);

        }

        private void UpdateScreen()
        {
            var fixedBoxes = _electricalBoxes.Count(x => x.IsRepaired);
            _information.text = $"Error: Please repair all electrical boxes {fixedBoxes}/6";

            if (fixedBoxes >= 6)
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
            _antenna.RPMByPass(_electricalBoxes.Count(x=>x.IsRepaired) * SpeedIncrease);
            Mod.GamePlaySettings.FixedPowerBoxes.Add(id);
            OnBoxFixedAction?.Invoke(id);
            if (_electricalBoxes.Any(x => !x.IsRepaired) && LargeWorldStreamer.main.IsWorldSettled())
            {
                VoiceNotificationSystem.main.Play("ElectricalBoxesNeedFixing_key",
                    $"Further electrical boxes in need of repair {_electricalBoxes.Count(x => x.IsRepaired)}/{_electricalBoxes.Count}");
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