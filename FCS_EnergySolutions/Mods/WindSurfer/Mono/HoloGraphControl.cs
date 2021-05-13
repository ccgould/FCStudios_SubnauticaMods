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
        public PlatformController PlatFormController { get; set; }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (_deleteBTN != null)
            {
                //_deleteBtnOBJ.SetActive(Slots[0].WindSurferOperatorController.CanRemovePlatform(this));
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

            if (_deleteBTN == null)
            {
                _deleteBtnOBJ = gameObject.FindChild("DeleteBTN");
                _deleteBTN = _deleteBtnOBJ.GetComponent<Button>();
                _deleteBTN.onClick.AddListener((() =>
                {
                    Slots[0].WindSurferOperatorController.RemovePlatform(PlatFormController);
                }));
            }

            FindSlots();
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
                    Slots.Add(hSlot); 
                }
            }
        }
    }
}