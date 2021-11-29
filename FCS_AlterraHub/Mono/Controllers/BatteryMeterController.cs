using FCS_AlterraHub.Model;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.Controllers
{
    public class BatteryMeterController : MonoBehaviour
    {
        private readonly Color _colorEmpty = new(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new(0f, 1f, 0f, 1f);
        private Image _batteryFill;
        private Text _batteryStatus;
        private Text _batteryPercentage;


        public void Initialize(float capacity = 0)
        {
            #region Battery Meter

            _batteryFill = gameObject?.FindChild("Fill")?.GetComponent<Image>();
            _batteryStatus = gameObject?.FindChild("BatteryStatus")?.GetComponent<Text>();
            
            if (_batteryStatus is not null)
            {
                _batteryStatus.text = $"0/{capacity}";
            }

            if (_batteryFill != null)
            {
                _batteryFill.color = _colorEmpty;
                _batteryFill.fillAmount = 0f;
            }

            _batteryPercentage = gameObject?.FindChild("Percentage")?.GetComponent<Text>();
            
            #endregion
        }

        public void UpdateBatteryStatus(PowercellData data)
        {
            var charge = data.GetCharge() < 1 ? 0f : data.GetCharge();

            var percent = charge / data.GetCapacity();

            if (_batteryFill != null)
            {
                if (data.GetCharge() >= 0f)
                {
                    var value = (percent >= 0.5f) ? Color.Lerp(_colorHalf, _colorFull, 2f * percent - 1f) : Color.Lerp(_colorEmpty, _colorHalf, 2f * percent);
                    _batteryFill.color = value;
                    _batteryFill.fillAmount = percent;
                }
                else
                {
                    _batteryFill.color = _colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }
            }

            _batteryPercentage.text = ((data.GetCharge() < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{percent:P0}");

            if (_batteryStatus != null)
            {
                _batteryStatus.text = $"{Mathf.RoundToInt(data.GetCharge())}/{data.GetCapacity()}";
            }
        }

        /// <summary>
        /// Updates the bar perecentage
        /// </summary>
        /// <param name="percent"></param>
        public void UpdateStateByPercentage(float percent)
        {
            if (_batteryFill != null)
            {
                var value = (percent >= 0.5f) ? Color.Lerp(_colorHalf, _colorFull, 2f * percent - 1f) : Color.Lerp(_colorEmpty, _colorHalf, 2f * percent);
                _batteryFill.color = value;
                _batteryFill.fillAmount = percent;
            }

            _batteryPercentage.text = $"{percent:P0}";
        }
    }
}
