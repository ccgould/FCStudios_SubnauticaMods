using System;
using System.Collections.Generic;
using FCS_EnergySolutions.Mods.WindSurfer.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    public class HoloGraphControl : MonoBehaviour
    {
        public List<HolographSlot> Slots = new List<HolographSlot>(4);
        private GameObject _turbine;
        private GameObject _operator;
        private Button _deleteBTN;
        private GameObject _deleteBtnOBJ;
        private Text _unitID;
        private WindSurferOperatorController _windSurferOperatorController;
        private Text _powerInfo;
        private GameObject _platform;

        private WindSurferOperatorController WindSurferOperatorController
        {
            get
            {
                if (_windSurferOperatorController == null)
                {
                    _windSurferOperatorController = Slots[0].WindSurferOperatorController;
                }
                
                return _windSurferOperatorController;
            }
        }

        public PlatformController PlatFormController { get; set; }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (_deleteBTN != null && _deleteBtnOBJ != null && WindSurferOperatorController != null)
            {
                _deleteBTN.interactable = WindSurferOperatorController.ScreenTrigger.selected;
            }
        }

        private void Initialize()
        {
            if (_turbine == null)
            {
                _turbine = gameObject.FindChild("Turbine");
                _turbine.SetActive(false);
            }

            if (_operator == null)
            {
                _operator = gameObject.FindChild("Operator");
            }

            if (_platform == null)
            {
                _platform = gameObject.FindChild("Platform");
            }


            if (_unitID == null)
            {
                _unitID = gameObject.FindChild("UnitID").GetComponent<Text>();
            }

            if (_powerInfo == null)
            {
                _powerInfo = _turbine.FindChild("PowerInfo").GetComponent<Text>();
            }

            if (_deleteBTN == null)
            {
                _deleteBtnOBJ = gameObject.FindChild("DeleteBTN");
                _deleteBTN = _deleteBtnOBJ.GetComponent<Button>();
                _deleteBTN.onClick.AddListener((() =>
                {
                    var result = WindSurferOperatorController.TryRemovePlatform(PlatFormController);
                    if (result)
                    {
                        WindSurferOperatorController.RefreshHoloGrams();
                    }

                }));
            }

            FindSlots();

            InvokeRepeating(nameof(UpdatePowerInfo),1f,1f);
        }

        private void UpdatePowerInfo()
        {
            if (_powerInfo != null)
            {
                _powerInfo.text = PlatFormController?.GetPowerInfo() ?? "0/0";
            }

            if (_unitID != null)
            {
                UpdateUnitId();
            }
        }

        public void RefreshDeleteButton(bool value)
        {
            _deleteBtnOBJ.SetActive(value);
        }


        internal void SetIcon(HolographIconType type)
        {
            Initialize();

            _operator.SetActive(false);
            _turbine.SetActive(false);
            _platform.SetActive(false);

            switch (type)
            {
                case HolographIconType.Operator:
                    _operator.SetActive(true);
                    break;
                case HolographIconType.Turbine:
                    _turbine.SetActive(true);
                    break;
                case HolographIconType.Platform:
                    _platform.SetActive(true);
                    break;
            }
        }

        public void FindSlots()
        {
            if (Slots.Count == 0)
            {
                foreach (Transform slot in gameObject.FindChild("Slot").transform)
                {
                    var hSlot = slot.gameObject.AddComponent<HolographSlot>();
                    hSlot.Initialize();
                    Slots.Add(hSlot); 
                }
            }
        }

        public void UpdateUnitId()
        {
            if(_unitID == null) return;
            _unitID.text = PlatFormController?.GetUnitID();
        }
    }
}