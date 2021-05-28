using UnityEngine;
using UWE;

public class DoorController : HandTarget, IHandTarget
{
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
        openSound = gameObject.AddComponent<FMOD_CustomEmitter>();
        var rejectedSoundAsset = ScriptableObject.CreateInstance<FMODAsset>();
        rejectedSoundAsset.id = "keypad_door_open";
        rejectedSoundAsset.path = "event:/env/keypad_door_open";
        openSound.asset = rejectedSoundAsset;
        openSound.restartOnPlay = true;
		if (!doorObject)
		{
			doorObject = gameObject;
		}

		sealedComponent = GetComponent<Sealed>();
		if (sealedComponent != null)
		{
			sealedComponent.openedEvent.AddHandler(gameObject, OnSealedDoorOpen);
		}

		closedPos = doorObject.transform.position;
		openPos = doorObject.transform.TransformPoint(new Vector3(-1.1f, 0f, 0f));
		
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
			Vector3 vector = doorObject.transform.position;
            vector = Vector3.Lerp(vector, doorOpen ? openPos : closedPos, Time.deltaTime * 2f);
            doorObject.transform.position = vector;
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
	}

	private void OnSealedDoorOpen(Sealed sealedComponent)
	{
		OnDoorToggle();
	}

	public StarshipDoor.OpenMethodEnum doorOpenMethod;
    public string openText = "OpenDoor";
    public string closeText = "CloseDoor";
    public bool doorOpen;
    public bool doorLocked = true;
    public bool startDoorOpen;
    public bool requirePlayerInFrontToOpen;
    public GameObject doorObject;
    public FMOD_CustomEmitter openSound;
    private Sealed sealedComponent;
    private Vector3 closedPos;
    private Vector3 openPos;
    public enum OpenMethodEnum
	{
        Manual,
        Sealed,
        Keycard,
        Powercell
	}
}