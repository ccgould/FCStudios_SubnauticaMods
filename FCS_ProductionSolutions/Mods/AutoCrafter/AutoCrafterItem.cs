using System;
using FCS_AlterraHub.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class AutoCrafterItem : MonoBehaviour
    {
        private Text _unitID;
        private DSSAutoCrafterController _autoCrafterController;
        private StandByPageController _mono;
        private Toggle _button;
        private const float _maxInteraction = 0.9f;

        public Action<DSSAutoCrafterController, bool> OnButtonClick { get; set; }

        internal void Initialize(StandByPageController mono)
        {
            _mono = mono;

            _unitID = gameObject.GetComponentInChildren<Text>();
            _button = gameObject.GetComponentInChildren<Toggle>();
            _button.onValueChanged.AddListener((value => { OnButtonClick?.Invoke(_autoCrafterController, value); }));
            var fcsbutton = _button.gameObject.AddComponent<FCSButton>();
            fcsbutton.MaxInteractionRange = _maxInteraction;
        }

        internal void Set(DSSAutoCrafterController crafter, bool state)
        {
            _autoCrafterController = crafter;
            _unitID.text = crafter.UnitID;
            _button.SetIsOnWithoutNotify(state);
            gameObject.SetActive(true);
        }

        internal bool GetState()
        {
            return _button.isOn;
        }

        internal void SetState(bool state)
        {
            _button.isOn = state;
        }

        public void Reset()
        {
            _unitID.text = string.Empty;
            _autoCrafterController = null;
            _button.SetIsOnWithoutNotify(false);
            gameObject.SetActive(false);
        }
    }
}