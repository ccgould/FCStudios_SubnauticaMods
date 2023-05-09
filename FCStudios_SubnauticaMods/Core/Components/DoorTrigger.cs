using UnityEngine;

namespace FCS_AlterraHub.Core.Components;

public class DoorTrigger : MonoBehaviour
{
    private DoorController door;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Player>(out Player player))
        {
            if(!door.IsOpen)
            {
                door.Open(player.transform.position);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            if (door.IsOpen)
            {
                door.Close();
            }
        }
    }
}
