using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.MiniMedBay.Mono;
internal class MiniMedBayTrigger : MonoBehaviour
{
    private bool inPlayerInTrigger;

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.gameObject.tag.Equals("Player")) return;
        inPlayerInTrigger = true;
    }

    private void OnTriggerStay(Collider collision)
    {
        if (!collision.gameObject.tag.Equals("Player")) return;
        inPlayerInTrigger = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        if (!collision.gameObject.tag.Equals("Player")) return;
        inPlayerInTrigger = false;
    }

    internal bool IsPlayerInTrigger()
    {
        return inPlayerInTrigger;
    }
}
