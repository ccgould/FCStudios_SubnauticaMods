using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Patches;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    public class MessagesController : MonoBehaviour
    {
        private bool _initialized;
        private GameObject _messageList;
        private readonly List<AudioMessage> _messages = new List<AudioMessage>();
        private Text _messageCounter;
        private FCSPDAController _pdaController;

#if SUBNAUTICA
        //private static FMOD.System FMOD_System => RuntimeManager.LowlevelSystem;
#else
        private static System FMOD_System => RuntimeManager.CoreSystem;
#endif

        internal void Initialize(FCSPDAController fcsPdaController)
        {
            if (_initialized) return;
            _messageList = GameObjectHelpers.FindGameObject(gameObject, "Messageslist");
            _pdaController = fcsPdaController;
            _messageCounter = GameObjectHelpers.FindGameObject(fcsPdaController.gameObject, "MessagesCounter").GetComponent<Text>();
            InvokeRepeating(nameof(UpdateMessageCounter), 1, 1);
            _initialized = true;
        }
        
        private void UpdateMessageCounter()
        {
            _messageCounter.text = _messages.Count(x => !x.HasBeenPlayed).ToString();
        }

        internal void AddNewMessage(string description,string from, string audioClipName, bool hasBeenPlayed = false)
        {
            var message = new AudioMessage(description, audioClipName) {HasBeenPlayed = hasBeenPlayed};
            _messages.Add(message);
            if (!hasBeenPlayed)
                uGUI_PowerIndicator_Initialize_Patch.MissionHUD.ShowNewMessagePopUp(from);
            RefreshUI();
        }

        private void RefreshUI()
        {
            for (int i = _messageList.transform.childCount - 1; i > 0; i--)
            {
                Destroy(_messageList.transform.GetChild(i).gameObject);
            }

            foreach (AudioMessage message in _messages)
            {
                var prefab = Instantiate(Buildables.AlterraHub.PDAEntryPrefab);
                var messageController = prefab.AddComponent<MessagePDAEntryController>();
                messageController.Initialize(message,this);
                prefab.transform.SetParent(_messageList.transform, false);
            }
        }

        public void PlayAudioTrack(string trackName)
        {
            if (string.IsNullOrEmpty(trackName))
            {
                QuickLogger.Debug("Track returned null",true);
                return;
            }

            CurrentTrackSound.isPlaying(out bool isPlaying);

            if (isPlaying)
            {
                CurrentTrackSound.stop();
            }

            //_pdaController.AudioTrack = AudioUtils.PlaySound(QuestManager.Instance.FindAudioClip(trackName), SoundChannel.Voice);
        }

        public Channel CurrentTrackSound { get; set; }
    }
}