using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Model
{
    public class ProximityActivate : MonoBehaviour
    {

        public Transform distanceActivator, lookAtActivator;
        public float distance;
        public Transform activator;
        public bool activeState = false;
        public CanvasGroup target;
        public bool lookAtCamera = false; //Disabled because it made the object rotate
        float alpha;
        Quaternion originRotation, targetRotation;
        private bool checkIsPlayerIsBehind;

        public void Initialize(GameObject distanceActivator,GameObject lookAtActivator, float distance,bool checkIsPlayerIsBehind = false)
        {
            target = gameObject.GetComponentInChildren<CanvasGroup>();
            this.distanceActivator = distanceActivator.transform;
            this.lookAtActivator = lookAtActivator.transform;
            this.distance = distance;
            originRotation = transform.rotation;
            alpha = activeState ? 1 : -1;
            this.checkIsPlayerIsBehind = checkIsPlayerIsBehind;
            if (activator == null) activator = Player.main.transform;

        }

        bool IsTargetNear()
        {
            var distanceDelta = distanceActivator.position - activator.position;
            if (distanceDelta.sqrMagnitude < distance * distance)
            {
                #region Disabled because it checks if the player is more than 0.95 away if any lower player is not in range

                if (checkIsPlayerIsBehind)
                {
                    if (lookAtActivator != null)
                    {
                        var lookAtActivatorDelta = lookAtActivator.position - activator.position;
                        if (Vector3.Dot(activator.forward, lookAtActivatorDelta.normalized) > 0.95f)
                            return true;
                    }
                    var lookAtDelta = target.transform.position - activator.position;
                    if (Vector3.Dot(activator.forward, lookAtDelta.normalized) > 0.95f)
                        return true;
                }
                else
                {
                    return true;
                }

                #endregion
            }
            return false;
        }

        void Update()
        {
            if (!activeState)
            {
                if (IsTargetNear())
                {
                    alpha = 1;
                    activeState = true;
                }
            }
            else
            {
                if (!IsTargetNear())
                {
                    alpha = -1;
                    activeState = false;
                }
            }
            target.alpha = Mathf.Clamp01(target.alpha + alpha * DayNightCycle.main.deltaTime);
            if (lookAtCamera)
            {
                if (activeState)
                    targetRotation = Quaternion.LookRotation(activator.position - transform.position);
                else
                    targetRotation = originRotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, DayNightCycle.main.deltaTime);
            }
        }

    }
}
