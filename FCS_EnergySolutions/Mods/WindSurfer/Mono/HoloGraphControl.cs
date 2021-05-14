using System.Collections.Generic;
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
        }

        public void RefreshDeleteButton(bool value)
        {
            _deleteBtnOBJ.SetActive(value);
            UpdateUnitId();
        }


        internal void SetAsTurbine()
        {
            Initialize();
            _turbine.SetActive(true);
            _operator.SetActive(false);
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