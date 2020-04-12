using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Helpers;
using MAC.OxStation.Buildables;
using MAC.OxStation.Mono;
using UnityEngine.UI;

namespace MAC.OxStation.Display
{
    internal class OxStationItemButton : InterfaceButton
    {
        internal Image OxstationHealth;
        internal Image OxstationOxygen;
        private OxStationController _station;
        private Text _text;

        public override void OnEnable()
        {
            base.OnEnable();
            _text = gameObject.GetComponentInChildren<Text>();
            InvokeRepeating(nameof(UpdateDisplay),0.5f,0.5f);
        }

        internal void UpdateDisplay()
        {
            if (_station == null)
            {
                if(Tag != null)
                    _station = (OxStationController)Tag;
            }

            if (_station == null || OxstationHealth == null || OxstationOxygen == null) return;
            OxstationHealth.fillAmount = _station.HealthManager.GetHealthPercentage();
            OxstationOxygen.fillAmount = _station.OxygenManager.GetO2LevelPercentageFloat();
            ToggleText();
        }

        public void ToggleText()
        {
            if(_station == null || _text == null) return;
            _text.text = _station.GetPingState() ? OxStationBuildable.Pinging() : OxStationBuildable.Ping();
        }
    }
}
