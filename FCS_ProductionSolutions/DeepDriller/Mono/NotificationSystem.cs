using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class NotificationSystem : MonoBehaviour
    {
        private string _currentMessage;
        private FCSDeepDrillerController _mono;
        private Text _label;

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
        }


        internal void SetNotification(string message)
        {

            if (_label == null)
            {
                _label = _mono?.DisplayHandler?.GetStatusField();
            }

            if (_currentMessage.Equals(message) || _label == null) return;
            _currentMessage = message;
            _label.text = message;
        }
    }
}
