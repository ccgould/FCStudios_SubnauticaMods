using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FCS_AlterraHub.Configuration;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubPod.Mono
{
    internal class DoorController : HandTarget, IHandTarget
    {
        public virtual bool IsSwingDoor => false;

        private void OnEnable()
        {
            if (NoCostConsoleCommand.main != null)
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

            SetDoorOpenClosePositions();

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

            QuickLogger.Debug("Door Controller Start");

            if (_saveData is not null)
            {
                doorLocked = _saveData.Item1;

                if (_saveData.Item2)
                {
                    Open();
                }
            }
        }

        private void SetDoorOpenClosePositions()
        {
            closedPos = doorObject.transform.position;

            switch (_doorDirection)
            {
                case DoorDirection.Up:
                    openPos = doorObject.transform.TransformPoint(new Vector3(0f, PositionOffset, 0f));
                    break;
                case DoorDirection.Down:
                    openPos = doorObject.transform.TransformPoint(new Vector3(0f, -PositionOffset, 0f));
                    break;
                case DoorDirection.Left:
                    openPos = doorObject.transform.TransformPoint(new Vector3(PositionOffset, 0f, 0f));
                    break;
                case DoorDirection.Right:
                    openPos = doorObject.transform.TransformPoint(new Vector3(-PositionOffset, 0f, 0f));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void Update()
        {
            if (doorOpenMethod == OpenMethodEnum.Manual || doorOpenMethod == OpenMethodEnum.None)
            {
                if (!IsSwingDoor)
                {
                    Vector3 vector = doorObject.transform.position;
                    vector = Vector3.Lerp(vector, doorOpen ? openPos : closedPos, Time.deltaTime * 2f);
                    doorObject.transform.position = vector;
                }
                else
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, doorOpen ? openRot : closedRot, Time.deltaTime * 5);
                }
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            if (doorOpenMethod == OpenMethodEnum.None) return;
            switch (doorOpenMethod)
            {
                case OpenMethodEnum.Manual:
                    if (!doorLocked)
                    {
                        HandReticle.main.SetText(HandReticle.TextType.Hand, doorOpen ? closeText : openText, false);
                        HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                        return;
                    }
                    else
                    {
                        HandReticle.main.SetText(HandReticle.TextType.Hand, $"{ManualHoverText1}\n{ManualHoverText2}", true);
                    }
                    break;
                case OpenMethodEnum.Sealed:
                    HandReticle.main.SetText(HandReticle.TextType.Hand, $"{Language.main.Get("Sealed_Door")}\n{Language.main.Get("SealedInstructions")}", true);

                    HandReticle.main.SetProgress(sealedComponent.GetSealedPercentNormalized());
                    HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
                    return;
                case OpenMethodEnum.Keycard:
                    HandReticle.main.SetText(HandReticle.TextType.Hand, $"{Language.main.Get("Locked_Door")}\n{Language.main.Get("DoorInstructions_Keycard")}", true);

                    HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                    return;
                case OpenMethodEnum.Powercell:
                    HandReticle.main.SetText(HandReticle.TextType.Hand, $"{Language.main.Get("Locked_Door")}\n{Language.main.Get("DoorInstructions_Powercell")}", true);

                    HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                    break;
                default:
                    return;
            }
        }

        public void OnHandClick(GUIHand guiHand)
        {
            if(doorOpenMethod ==  OpenMethodEnum.None) return;
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
            if (doorOpenMethod == OpenMethodEnum.Manual || doorOpenMethod == OpenMethodEnum.None)
            {
                doorOpen = !doorOpen;
            }
            if (openSound)
            {
                openSound.Play();
            }

            OnDoorStateChanged?.Invoke(this, doorOpen);
        }

        public void Open()
        {
            if(doorLocked || doorOpen) return;

            if (doorOpenMethod == OpenMethodEnum.Manual || doorOpenMethod == OpenMethodEnum.None)
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
            if (!doorOpen) return;

            if (doorOpenMethod == OpenMethodEnum.Manual || doorOpenMethod == OpenMethodEnum.None)
            {
                doorOpen = false;
            }
            if (openSound)
            {
                openSound.Play();
            }
        }

        public bool IsUnlocked => !doorLocked;


        public Action<DoorController, bool> OnDoorStateChanged;
        private void OnSealedDoorOpen(Sealed sealedComponent)
        {
            OnDoorToggle();
        }

        public OpenMethodEnum doorOpenMethod = OpenMethodEnum.None;
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
        private DoorDirection _doorDirection = DoorDirection.Left;
        public float PositionOffset;
        private Tuple<bool, bool> _saveData;

        public enum OpenMethodEnum
        {
            None = -1,
            Manual = 0,
            Sealed = 1,
            Keycard = 2,
            Powercell = 3
        }

        public enum DoorDirection
        {
            Up = 0,
            Down = 1,
            Left = 2,
            Right = 3,
        }

        public void SetDoorDirection(DoorDirection direction)
        {
            _doorDirection = direction;
        }

        public void LoadSave(Tuple<bool,bool> settings)
        {
            if (settings is null) return;
            QuickLogger.Debug($"DoorController: {settings.Item1} | {settings.Item2}");
            _saveData = settings;
        }
    }
}