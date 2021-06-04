using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCS_AlterraHub.Mono.OreConsumer;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHubFabricatorBuilding.Mono
{
    internal class AntennaController : MonoBehaviour
    {
        internal List<ElectricalBox> _electricalBoxes = new List<ElectricalBox>();
        private Text _information;
        private Button _reactiveBTN;
        private MotorHandler _antenna;
        private Image _powerIcon;
        private const float SpeedIncrease = 16.666666666666666666666666666667f;
        public void Initialize(AlterraFabricatorStationController fabricatorStationController)
        {

            var electricalBoxes = gameObject.FindChild("ElectricalBox").transform;
            for (int i = 0; i < electricalBoxes.childCount; i++)
            {
                var eBox = electricalBoxes.GetChild(i).gameObject.AddComponent<ElectricalBox>();
                eBox.Initialize(this,i);
                _electricalBoxes.Add(eBox);
            }

            _antenna = GameObjectHelpers.FindGameObject(gameObject, "anim_mesh").EnsureComponent<MotorHandler>();
            _antenna.SetIncreaseRate(5);
            _antenna.Initialize(0);

            _powerIcon = GameObjectHelpers.FindGameObject(gameObject, "Image").GetComponent<Image>();

            _information = GameObjectHelpers.FindGameObject(gameObject, "Text").GetComponent<Text>();
            _reactiveBTN = gameObject.GetComponentInChildren<Button>();
            _reactiveBTN.onClick.AddListener(() =>
            {
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
        }

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