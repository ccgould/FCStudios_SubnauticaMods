using FCS_AlterraHub.Buildables;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Systems
{
    internal class CreditNotificationSystem : MonoBehaviour
    {
        public float targetTime = 0;
        public static CreditNotificationSystem main;
        private decimal _amount;

        void Awake()
        {
            main = this;
        }

        private void Update()
        {
            if (_amount == 0)
            {
                return;
            }

            targetTime += Time.deltaTime;

            if (targetTime >= Main.Configuration.CreditMessageDelay)
            {
                timerEnded();
            }

        }

        void timerEnded()
        {
            if (_amount > 0)
            {
                QuickLogger.CreditMessage(AlterraHub.CreditMessage(_amount));
            }
            _amount = 0;
            targetTime = 0;
        }

        internal void AddCredit(decimal amount)
        {
            _amount += amount;
        }
    }
}
