using FCS_AlterraHub.Helpers;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Models;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Managers
{
    internal class LaserManager
    {
        private FCSDeepDrillerController _mono;
        private GameObject _lasersStarters;
        private GameObject _xBubblesGroup;
        private bool _initialized;

        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;

            FindLaserPoints();
        }

        private void FindLaserPoints()
        {
            if (_mono?.gameObject == null) return;

            _lasersStarters = GameObjectHelpers.FindGameObject(_mono.gameObject, "Lasers_Main");
            _xBubblesGroup = GameObjectHelpers.FindGameObject(_mono.gameObject, "xBubbles_Group");
            var lasersEnds = GameObjectHelpers.FindGameObject(_mono.gameObject, "Laser_Ends");
                
            if (!_lasersStarters || !_xBubblesGroup || !lasersEnds) return;

            for (var i = 0; i < _lasersStarters.transform.childCount; i++)
            {
                var laser = _lasersStarters.transform.GetChild(i);
                var laserComp = laser.GetChild(0).gameObject.AddComponent<LaserScript>();
                var end = lasersEnds.transform.GetChild(i);
                laserComp.StartPoint = laser.transform;
                laserComp.EndPoint = end;
            }

            if (!_mono.IsUnderWater())
            {
                ChangeBubblesState(false);
            }
            _initialized = true;
        }

        internal void ChangeLasersState(bool state = true)
        {
            _lasersStarters.SetActive(state);
        }

        internal void ChangeBubblesState(bool state = true)
        {
            _xBubblesGroup.SetActive(state);
        }
    }
}
