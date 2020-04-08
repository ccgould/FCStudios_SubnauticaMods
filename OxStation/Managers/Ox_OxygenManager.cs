using FCSCommon.Enums;
using FCSCommon.Utilities;
using MAC.OxStation.Config;
using MAC.OxStation.Mono;
using MAC.OxStation.Patches;
using UnityEngine;

namespace MAC.OxStation.Managers
{
    internal class Ox_OxygenManager
    {
        private float _o2Level;
        private float _tankCapacity;
        private float _amountPerSecond;
        private OxStationController _mono;

        internal void Initialize(OxStationController mono)
        {
            _mono = mono;
            FillTank();
            _tankCapacity = QPatch.Configuration.TankCapacity;
        }

        private void FillTank()
        {
            _o2Level = QPatch.Configuration.TankCapacity;
        }

        /// <summary>
        /// Removes oxygen from the unit.
        /// </summary>
        /// <param name="getOxygenPerBreath"></param>
        internal bool RemoveOxygen(float getOxygenPerBreath)
        {
            if (!_mono.IsConstructed) return false;

            float num = Mathf.Min(getOxygenPerBreath, _o2Level);
            _o2Level = Mathf.Max(0f, this._o2Level - num);
            if (_o2Level < 1)
            {
                _o2Level = 0;
            }

            QuickLogger.Debug($"Unit Oxygen Level: {_o2Level}");
            return true;
        }

        /// <summary>
        /// The current oxygen level of the unit.
        /// </summary>
        /// <returns></returns>
        internal float GetO2Level()
        {
            return _o2Level;
        }

        /// <summary>
        /// Set the O2 level of the unit.
        /// </summary>
        /// <param name="amount"></param>
        internal void SetO2Level(float amount)
        {
            _o2Level = Mathf.Clamp(amount, 0, _tankCapacity);
        }

        /// <summary>
        /// Generates oxygen for the unit
        /// </summary>
        internal void GenerateOxygen()
        {
            if (_mono.PowerManager.GetPowerState() == FCSPowerStates.Powered)
            {
                SetO2Level(_o2Level + _amountPerSecond);
            }
        }

        /// <summary>
        /// Set the amount of oxygen to add tot he unit per second.
        /// </summary>
        /// <param name="amount"></param>
        internal void SetAmountPerSecond(float amount)
        {
            _amountPerSecond = amount;
        }

        /// <summary>
        /// Retrieves the amount of oxygen per second that is set;
        /// </summary>
        /// <returns></returns>
        internal float GetAmountPerSecond()
        {
            return _amountPerSecond;
        }

        public void GivePlayerO2()
        {
            if (_o2Level <= 0 || !_mono.IsConstructed) return;

            var o2Manager = Player.main.oxygenMgr;

            float playerO2Request;
            if (Mod.RTInstalled && Player.main.oxygenMgr.HasOxygenTank())
            {
                playerO2Request = Player_Patches.DefaultO2Level - o2Manager.GetOxygenAvailable();
            }
            else
            {
                playerO2Request = o2Manager.GetOxygenCapacity() - o2Manager.GetOxygenAvailable();
            }


            QuickLogger.Debug($"Taking: {playerO2Request} || RT Installed: {Mod.RTInstalled}", true);

            if (_o2Level >= playerO2Request)
            {
                _o2Level -= playerO2Request;
                o2Manager.AddOxygen(Mathf.Abs(playerO2Request));
                QuickLogger.Debug($"O2 Level: {_o2Level} || Tank Level {playerO2Request}", true);

                return;
            }

            var playerO2RequestRemainder = Mathf.Min(_o2Level, playerO2Request);
            _o2Level -= playerO2RequestRemainder;
            o2Manager.AddOxygen(Mathf.Abs(playerO2RequestRemainder));
            QuickLogger.Debug($"O2 Level: {_o2Level} || Tank Level {playerO2RequestRemainder}", true);

            //var modified = -Mathf.Min(-playerO2Request, _o2Level);
        }

        internal int GetO2LevelPercentageFull()
        {
            return Mathf.RoundToInt((100 * _o2Level) / _tankCapacity);
        }

        internal float GetO2LevelPercentage()
        {
            return _o2Level / _tankCapacity;
        }
    }
}
