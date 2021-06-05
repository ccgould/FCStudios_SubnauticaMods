using System;
using FCS_AlterraHub.Abstract;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class MiniMedBayDisplay : AIDisplay
    {
        private MiniMedBayController _mono;
        private bool _initialized;
        private Text _dispenserCounter;
        private Text _healMeter;

        internal void Setup(MiniMedBayController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                _initialized = true;
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName.Equals("HealBTN"))
            {
                _mono.HealBedManager.HealPlayer();
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                _dispenserCounter = InterfaceHelpers.FindGameObject(gameObject, "MedKitDispenserCount").GetComponent<Text>();
                _healMeter = InterfaceHelpers.FindGameObject(gameObject, "HealMeter").GetComponent<Text>();
                
                var healBtnObj = InterfaceHelpers.FindGameObject(gameObject, "HealBTN");
                var healBtn = InterfaceHelpers.CreateButton(healBtnObj, "HealBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0f, 1f, 1f, 1f), 5);
                healBtn.TextLineOne = AuxPatchers.HealPlayer();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        public void UpdatePlayerHealthPercent(int amount)
        {
            _healMeter.text = $"{amount}%";
        }

        public void UpdateDispenserCount(int medKits)
        {
            if(_initialized)
                _dispenserCounter.text = medKits.ToString();
        }
    }
}
