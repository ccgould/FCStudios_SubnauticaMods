using System;
using QuantumTeleporter.Buildable;
using QuantumTeleporter.Mono;
using UnityEngine;

namespace QuantumTeleporter.Managers
{
    internal class QTDoorManager: HandTarget,IHandTarget
    {
        private QuantumTeleporterController _mono;
        private int _doorState;

        internal void Initalize(QuantumTeleporterController mono)
        {
            _mono = mono;

            _doorState = Animator.StringToHash("DoorState");
        }

        internal void OpenDoor()
        {
            if(_mono == null || _mono.AnimationManager == null) return;
            _mono.AnimationManager.SetBoolHash(_doorState,true);
        }

        internal void CloseDoor()
        {
            if (_mono == null || _mono.AnimationManager == null) return;
            _mono.AnimationManager.SetBoolHash(_doorState, false);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_mono == null) return;

            if(TeleportManager.IsTeleporting()) return;

            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Hand);
            
            if (!IsDoorOpen())
            {
                main.SetInteractText(QuantumTeleporterBuildable.OpenDoor());
            }
            else
            {
                main.SetInteractText(QuantumTeleporterBuildable.CloseDoor());
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsDoorOpen())
            {
                OpenDoor();
            }
            else
            {
                CloseDoor();
            }
        }

        private bool IsDoorOpen()
        {
            if (_mono == null || _mono.AnimationManager == null) return false;
            return _mono.AnimationManager.GetBoolHash(_doorState);
        }

        public void OnPlayerExit()
        {
            if (IsDoorOpen())
            {
                CloseDoor();
            }
        }
    }
}
