using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class MultiplierController : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        private Text _label;
        private DSSTerminalController _mono;

        internal void Initialize(DSSTerminalController mono)
        {
            _mono = mono;
            TextLineOne = AuxPatchers.Multiplier();
            TextLineTwo = AuxPatchers.MultiplierDesc();
            _label = gameObject.GetComponentInChildren<Text>();
            UpdateLabel();
        }

        #region Overrides
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {

            base.OnPointerExit(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (this.IsHovered)
            {
                if (_label != null)
                {
                    switch (_mono.BulkMultiplier)
                    {
                        case BulkMultipliers.TimesTen:
                            _mono.BulkMultiplier = BulkMultipliers.TimesOne;
                            break;
                        case BulkMultipliers.TimesEight:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTen;
                            break;
                        case BulkMultipliers.TimesSix:
                            _mono.BulkMultiplier = BulkMultipliers.TimesEight;
                            break;
                        case BulkMultipliers.TimesFour:
                            _mono.BulkMultiplier = BulkMultipliers.TimesSix;
                            break;
                        case BulkMultipliers.TimesTwo:
                            _mono.BulkMultiplier = BulkMultipliers.TimesFour;
                            break;
                        case BulkMultipliers.TimesOne:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTwo;
                            break;
                    }

                    UpdateLabel();
                }
            }
            else
            {
                QuickLogger.Debug("Button Not Hovered", true);
            }
        }

        internal void UpdateLabel()
        {
            _label.text = $"x{(int) _mono.BulkMultiplier}";
        }

        #endregion

    }
}