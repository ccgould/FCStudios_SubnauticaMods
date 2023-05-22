using UnityEngine;

namespace FCS_AlterraHub.Core.Components;

internal class uGUIFCS : MonoBehaviour
{
    private void Awake()
    {
        ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdatePDA, new ManagedUpdate.OnUpdate(FCSPDAController.PerformUpdate));
    }

    private void OnDestroy()
    {
        ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdatePDA, new ManagedUpdate.OnUpdate(PDA.PerformUpdate));
    }
}