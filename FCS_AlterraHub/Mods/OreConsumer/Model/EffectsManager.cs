using FCS_AlterraHub.Helpers;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.OreConsumer.Model
{
    internal class EffectsManager : MonoBehaviour
    {
        private GameObject _bubbles;
        private GameObject _smoke;
        private bool _isUnderwater;

        internal void Initialize(bool isUnderWater)
        {
            _bubbles = GameObjectHelpers.FindGameObject(gameObject, "xBubbles");
            _smoke = GameObjectHelpers.FindGameObject(gameObject, "WhiteSmoke");
            _isUnderwater = isUnderWater;
        }

        internal void ShowEffect()
        {
            if (_isUnderwater)
            {
                _bubbles.SetActive(true);
                _smoke.SetActive(false);
            }
            else
            {
                _bubbles.SetActive(false);
                _smoke.SetActive(true);
            }
        }

        internal void HideEffect()
        {
            _bubbles.SetActive(false);
            _smoke.SetActive(false);
        }
    }
}