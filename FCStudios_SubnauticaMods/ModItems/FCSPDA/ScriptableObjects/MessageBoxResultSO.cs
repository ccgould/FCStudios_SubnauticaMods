using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;

[CreateAssetMenu(fileName = "MessageBox Result", menuName = "FCSStudios/MessageBox")]
public class MessageBoxResultSO : ScriptableObject
{
    public FCSMessageResult Result;
}
