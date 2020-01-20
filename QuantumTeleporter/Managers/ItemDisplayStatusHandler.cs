using System;
using FCSCommon.Abstract;
using FCSCommon.Controllers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using QuantumTeleporter.Buildable;
using QuantumTeleporter.Enumerators;
using QuantumTeleporter.Mono;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuantumTeleporter.Managers
{
    internal class ItemDisplayStatusHandler: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private QuantumTeleporterController _mono;
        private Text _textField;
        private QTDisplayManager _display;
        private Text _statusField;
        private float _maxInteractionRange;
        
        public virtual void Update()
        {
            bool inInteractionRange = InInteractionRange();

            if (this.IsHovered && inInteractionRange)
            {
#if SUBNAUTICA
                HandReticle.main.SetInteractTextRaw($"{QuantumTeleporterBuildable.Coordinate()}: {_mono.transform.position}", $"{QuantumTeleporterBuildable.PowerAvailable()}: {_mono.PowerManager.PowerAvailable()}");
#elif BELOWZERO

                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, $"{QuantumTeleporterBuildable.Coordinate()}: {_mono.transform.position}");
                HandReticle.main.SetTextRaw(HandReticle.TextType.HandSubscript, $"{QuantumTeleporterBuildable.PowerAvailable()}: {_mono.PowerManager.PowerAvailable()}");
#endif
            }
        }

        internal void Initialize(QuantumTeleporterController mono, GameObject textField, GameObject statusField, QTDisplayManager display, float maxInteractionRange)
        {
            _mono = mono;
            _display = display;

            if (statusField != null)
            {
                _statusField = statusField.GetComponentInChildren<Text>();
            }

            if (textField != null)
            {
                _textField = textField.GetComponentInChildren<Text>();
            }

            InvokeRepeating(nameof(UpdateStatus),1,1);
            mono.NameController.OnLabelChanged += OnLabelChanged;
            _maxInteractionRange = maxInteractionRange;
        }

        private void OnLabelChanged(string arg1, NameController arg2)
        {
            //TODO check if working
            QuickLogger.Debug("ItemDisplayStatusHandler: OnLabeled Changed", true);
            if (_textField != null && _textField.isActiveAndEnabled)
            {
                _textField.text = arg1;
            }
            else
            {
                QuickLogger.Info("OnLabelChanged _textfield returned null or isn't enabled");
            }
        }

        private void UpdateStatus()
        {
            if (_statusField == null || _mono == null) return;
   
            switch (_display.SelectedTab)
            {
                case QTTabs.Global:
                    _statusField.text = _mono.PowerManager.HasEnoughPower(QTTeleportTypes.Global) ? QuantumTeleporterBuildable.Online() : QuantumTeleporterBuildable.Offline();
                    break;
                case QTTabs.Intra:
                    _statusField.text = _mono.PowerManager.HasEnoughPower(QTTeleportTypes.Intra) ? QuantumTeleporterBuildable.Online() : QuantumTeleporterBuildable.Offline();
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (InInteractionRange())
            {
                this.IsHovered = true;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            this.IsHovered = false;
        }

        public bool IsHovered { get; set; }

        protected bool InInteractionRange()
        {
            return Mathf.Abs(Vector3.Distance(this.gameObject.transform.position, Player.main.transform.position)) <= _maxInteractionRange;
        }

        private void OnDestroy()
        {
            _mono.NameController.OnLabelChanged -= OnLabelChanged;
        }
    }
}
