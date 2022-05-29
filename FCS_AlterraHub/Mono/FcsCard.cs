using FCS_AlterraHub.Systems;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    [RequireComponent(typeof(PrefabIdentifier))]
    internal class FcsCard : MonoBehaviour
    {
        private string _prefabId;
        private string _cardNumber;
        private bool _isInitialized;
        private CardSystem _cardSystem => CardSystem.main;

        #region Unity Methods

        private void Initialize()
        {
            if(_isInitialized) return;
            if (!CardSystem.main.CardExistFromPrefabID(GetPrefabId()))
            {
                CardSystem.main.GenerateNewCard(GetPrefabId());
            }

            SetCardNumber(CardSystem.main.GetCardNumber(GetPrefabId())); 
            _isInitialized =true;
        }

        #endregion


        internal string GetPrefabId()
        {
            return _prefabId ?? (_prefabId = gameObject.GetComponent<PrefabIdentifier>().Id ??
                                             gameObject.GetComponentInChildren<PrefabIdentifier>().Id);
        }

        internal void SetCardNumber(string cardNumber)
        {
            _cardNumber = cardNumber;
        }

        internal string GetCardNumber()
        {
            Initialize();
            return _cardNumber;
        }

        internal decimal GetCardBalance()
        {
            return CardSystem.main.GetAccountBalance();
        }
    }
}
