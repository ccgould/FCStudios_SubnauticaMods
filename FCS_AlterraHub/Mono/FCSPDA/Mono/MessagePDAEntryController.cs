using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    internal class MessagePDAEntryController : MonoBehaviour
    {
        private bool _isInitialized;
       
        internal void Initialize(AudioMessage message, MessagesController messagesController)
        {
            if(_isInitialized) return;
            GetComponentInChildren<Text>().text = message.Description;
            GetComponentInChildren<Button>().onClick.AddListener((() =>
            {
                messagesController.PlayAudioTrack(message.AudioClipName);
                message.HasBeenPlayed = true;
            }));
            _isInitialized = true;
        }
    }
}