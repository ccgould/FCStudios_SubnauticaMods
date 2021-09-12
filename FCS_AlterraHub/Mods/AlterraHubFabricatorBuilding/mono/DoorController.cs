using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    public class DoorController : HandTarget, IHandTarget
    {
        public virtual bool IsSwingDoor => false;

        private void OnEnable()
        {
            if(NoCostConsoleCommand.main != null)
                NoCostConsoleCommand.main.UnlockDoorsEvent += OnUnlockDoorsCheat;
        }

        private void OnDisable()
        {
            NoCostConsoleCommand.main.UnlockDoorsEvent -= OnUnlockDoorsCheat;
        }

        private void OnUnlockDoorsCheat()
        {
            if (NoCostConsoleCommand.main.unlockDoors)
            {
                UnlockDoor();
                return;
            }
            LockDoor();
        }

        private void Start()
        {
            openSound = FModHelpers.CreateCustomEmitter(gameObject, "keypad_door_open", "event:/env/keypad_door_open");

            if (!doorObject)
            {
                doorObject = gameObject;
            }

            sealedComponent = GetComponent<Sealed>();
            if (sealedComponent != null)
            {
                sealedComponent.openedEvent.AddHandler(gameObject, OnSealedDoorOpen);
            }

            openPos = doorObject.transform.TransformPoint(new Vector3(-1.1f, 0f, 0f));
            closedPos = doorObject.transform.position;
            
            //openRot = Quaternion.Euler(doorObject.transform.localEulerAngles.x, -90, doorObject.transform.localEulerAngles.y);
            closedRot = doorObject.transform.localRotation;

            if (startDoorOpen || doorOpen)
            {
                doorLocked = false;
                doorOpen = true;
                doorObject.transform.position = openPos;
            }
            if (!doorLocked)
            {
                UnlockDoor();
            }
            if (NoCostConsoleCommand.main?.unlockDoors ?? false)
            {
                UnlockDoor();
            }
        }

        private void Update()
        {
            if (doorOpenMethod == StarshipDoor.OpenMethodEnum.Manual)
            {
                if (!IsSwingDoor)
                {
                    Vector3 vector = doorObject.transform.position;
                    vector = Vector3.Lerp(vector, doorOpen ? openPos : closedPos, Time.deltaTime * 2f);
                    doorObject.transform.position = vector;
                }
                else
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation , doorOpen ? openRot : closedRot, Time.deltaTime * 5);
                }
            }
        }
        
        public void OnHandHover(GUIHand hand)
        {
            switch (doorOpenMethod)
            {
                case StarshipDoor.OpenMethodEnum.Manual:
                    if (!doorLocked)
                    {
                        HandReticle.main.SetInteractText(doorOpen ? closeText : openText);
                        HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                        return;
                    }
                    else
                    {
                        HandReticle.main.SetInteractText(ManualHoverText1, ManualHoverText2);
                    }
                    break;
                case StarshipDoor.OpenMethodEnum.Sealed:
                    HandReticle.main.SetInteractText("Sealed_Door", "SealedInstructions");
                    HandReticle.main.SetProgress(sealedComponent.GetSealedPercentNormalized());
                    HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
                    return;
                case StarshipDoor.OpenMethodEnum.Keycard:
                    HandReticle.main.SetInteractText("Locked_Door", "DoorInstructions_Keycard");
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                    return;
                case StarshipDoor.OpenMethodEnum.Powercell:
                    HandReticle.main.SetInteractText("Locked_Door", "DoorInstructions_Powercell");
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                    break;
                default:
                    return;
            }
        }

        public void OnHandClick(GUIHand guiHand)
        {
            if (requirePlayerInFrontToOpen && !Utils.CheckObjectInFront(transform, Player.main.transform, 90f))
            {
                return;
            }
            if (!doorLocked)
            {
                OnDoorToggle();
            }
        }

        public void UnlockDoor()
        {
            doorLocked = false;
        }

        public void LockDoor()
        {
            doorLocked = true;
        }

        private void OnDoorToggle()
        {
            if (doorOpenMethod == StarshipDoor.OpenMethodEnum.Manual)
            {
                doorOpen = !doorOpen;
            }
            if (openSound)
            {
                openSound.Play();
            }

            OnDoorStateChanged?.Invoke(this,doorOpen);
        }

        public void Open()
        {
            if (doorOpenMethod == StarshipDoor.OpenMethodEnum.Manual)
            {
                doorOpen = true;
            }
            if (openSound)
            {
                openSound.Play();
            }
        }

        public void Close()
        {
            if (doorOpenMethod == StarshipDoor.OpenMethodEnum.Manual)
            {
                doorOpen = false;
            }
            if (openSound)
            {
                openSound.Play();
            }
        }


        public Action<DoorController, bool> OnDoorStateChanged;
        private void OnSealedDoorOpen(Sealed sealedComponent)
        {
            OnDoorToggle();
        }

        public StarshipDoor.OpenMethodEnum doorOpenMethod;
        public string openText = "OpenDoor";
        public string closeText = "CloseDoor";
        public string ManualHoverText1 = "Locked_Door";
        public string ManualHoverText2 = AlterraHub.DoorInstructions();
        public bool doorOpen;
        public bool doorLocked = true;
        public bool startDoorOpen;
        public bool requirePlayerInFrontToOpen;
        public GameObject doorObject;
        public FMOD_CustomEmitter openSound;
        private Sealed sealedComponent;
        private Vector3 closedPos;
        private Vector3 openPos;
        public Quaternion closedRot;
        public Quaternion openRot;
        public enum OpenMethodEnum
        {
            Manual,
            Sealed,
            Keycard,
            Powercell
        }
    }
}