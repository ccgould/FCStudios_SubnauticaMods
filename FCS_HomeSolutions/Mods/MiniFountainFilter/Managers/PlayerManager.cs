using UnityEngine;

namespace FCS_HomeSolutions.Mods.MiniFountainFilter.Managers
{
    internal class PlayerManager
    {
        private Survival _survival;
        public float PlayerMaxWater => 100f;

        internal void Initialize()
        {
            _survival = Player.mainObject.GetComponent<Survival>();
        }

        internal float GetPlayerWaterLevel()
        {
            return _survival.water;
        }

        internal void SetPlayerWaterLevel(float amount)
        {
            _survival.water = Mathf.Clamp(amount, 0, PlayerMaxWater);
        }

        internal void AddWaterToPlayer(float amount)
        {
            _survival.water = Mathf.Clamp(_survival.water + amount, 0, PlayerMaxWater);
        }
    }
}
