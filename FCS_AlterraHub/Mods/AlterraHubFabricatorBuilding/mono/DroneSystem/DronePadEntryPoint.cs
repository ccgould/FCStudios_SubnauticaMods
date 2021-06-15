using System;
using System.Collections;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    public class DronePadEntryPoint : MonoBehaviour
    {
        private IDroneDestination _parentPad;
        private int State;
        private Animator _animator;
        private bool _isDocking;

        private void Start()
        {
            _parentPad = gameObject.GetComponentInParent<IDroneDestination>();
            State = Animator.StringToHash("State");
            _animator = GetComponent<Animator>();
        }
        private void OnTriggerEnter(Collider collider)
        {
            Debug.Log($"Trigger entered");
            var drone = collider.gameObject.GetComponentInParent<DroneController>();
            if (drone != null)
            {
                Debug.Log($"Drone: {drone}");
                //There is a possible chance than a drone can pass and was not suppose to dock we
                //need to make sure this is the correct drone as well.

                if (drone.GetDestination() == _parentPad && drone.GetState() == DroneController.DroneStates.Transitioning)
                {
                    _parentPad.SetDockedDrone(drone);
                    _isDocking = true;
                    StartCoroutine(DockDrone(drone));
                }
                else
                {
                    QuickLogger.Debug("Drone not Docking.");
                }
            }
        }

        public IEnumerator PlayAndWaitForAnim(int valueToSet,string stateName,Action onCompleted)
        {
            _animator.SetInteger(State, valueToSet);

            //Wait until we enter the current state
            while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            {
                QuickLogger.Debug("Wait until we enter the current state.",true);
                yield return null;
            }

            //Now, Wait until the current state is done playing
            while ((_animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.99f)
            {
                QuickLogger.Debug("Now, Wait until the current state is done playing", true);
                yield return null;
            }

            //Done playing. Do something below!
            onCompleted?.Invoke();
        }

        internal IEnumerator DockDrone(DroneController drone)
        {
            while (drone.GetState() != DroneController.DroneStates.Docking)
            {
                yield return null;
            }
            QuickLogger.Debug("Docking Drone");
            CompleteDocking(drone);
            yield break;
        }

        internal void Depart(DroneController drone)
        {
            QuickLogger.Debug($"Departing Drone:{drone.GetId()} from Port: {_parentPad.GetBaseName()}", true);
            drone.transform.parent = transform;
            drone.transform.localPosition = Vector3.zero;
            drone.transform.localRotation = Quaternion.identity;

            StartCoroutine(PlayAndWaitForAnim(1,"Drone_Departing", () =>
                {
                    QuickLogger.Debug("UnDocking complete unlocking drone.", true);
                    drone.UnDock();
                    drone.IsDeparting(false);
                    _parentPad.CloseDoors();
                    _animator.SetInteger(State, 0);
                }
            ));
        }

        private void CompleteDocking(DroneController drone)
        {
            drone.transform.parent = transform;
            drone.transform.localPosition = Vector3.zero;
            drone.transform.localRotation = Quaternion.identity;
            _parentPad.OpenDoors();

            StartCoroutine(PlayAndWaitForAnim(2,"Drone_Approaching", () =>
                {
                    QuickLogger.Debug("Docking complete", true);
                    _parentPad.CloseDoors();
                    drone.IsDocking(false);
                    _parentPad.Offload(drone);
                }
            ));
        }
    }
}
