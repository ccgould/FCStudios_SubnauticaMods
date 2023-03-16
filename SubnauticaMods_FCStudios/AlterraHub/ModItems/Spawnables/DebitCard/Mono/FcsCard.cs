using FCS_AlterraHub.Core.Services;
using UnityEngine;


namespace FCS_AlterraHub.ModItems.Spawnables.DebitCard.Mono
{
    [RequireComponent(typeof(PrefabIdentifier))]
    internal class FcsCard : MonoBehaviour
    {
        private string _prefabId;
        private string _cardNumber;
        private bool _isInitialized;

        #region Unity Methods

        private void Initialize()
        {
            if (_isInitialized) return;
            if (!AccountService.main.CardExistFromPrefabID(GetPrefabId()))
            {
                AccountService.main.GenerateNewCard(GetPrefabId());
            }

            SetCardNumber(AccountService.main.GetCardNumber(GetPrefabId()));
            _isInitialized = true;
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
            return AccountService.main.GetAccountBalance();
        }
    }
}
