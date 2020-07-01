using System;
using FCS_DeepDriller.Model;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_DeepDriller.Managers
{
    internal class LaserManager
    {
        private GameObject _gameObject;
        private GameObject _lasersStarters;
        private GameObject _xBubblesGroup;

        internal void Setup(FCSDeepDrillerController mono)
        {
            _gameObject = mono.gameObject;
            FindLaserPoints();
        }

        private void FindLaserPoints()
        {
            if (_gameObject != null)
            {
                _lasersStarters = GameObjectHelpers.FindGameObject(_gameObject, "Lasers_Main");
                _xBubblesGroup = GameObjectHelpers.FindGameObject(_gameObject, "xBubbles_Group");
                var lasersEnds = GameObjectHelpers.FindGameObject(_gameObject, "Laser_Ends");

                for (var i = 0; i < _lasersStarters.transform.childCount; i++)
                {
                    var laser = _lasersStarters.transform.GetChild(i);
                    var laserComp = laser.GetChild(0).gameObject.AddComponent<LaserScript>();
                    var end = lasersEnds.transform.GetChild(i);
                    laserComp.StartPoint = laser.transform;
                    laserComp.EndPoint = end;
                }
            }
        }

        internal void ChangeLasersState(bool state = true)
        {
            _lasersStarters.SetActive(state);
            _xBubblesGroup.SetActive(state);
        }
    }
}
