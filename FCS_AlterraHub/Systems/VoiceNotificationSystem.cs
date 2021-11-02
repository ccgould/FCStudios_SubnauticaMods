using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Helpers;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AlterraHub.Systems
{
    public class VoiceNotificationSystem : MonoBehaviour
    {

        public static VoiceNotificationSystem main;
        private static Dictionary<string,VoiceData> _voiceNotifications = new();
        public float minInterval;
        private float timeNextPlay = -1f;
        private Queue<PendingNotification> _queue = new();
        private Channel _currentChannel;
        private bool _wasPlaying;

        private void Awake()
        {
            main = this;
        }

        private void Start()
        {
            InvokeRepeating(nameof(CheckPendingMessages),1f,1f);
        }

        private void Update()
        {
           _currentChannel.isPlaying(out bool isPlaying);

            if (WorldHelpers.CheckIfPaused() && isPlaying)
            {
                _currentChannel.setPaused(true);
                _wasPlaying = true;
            }

            if (!WorldHelpers.CheckIfPaused() && _wasPlaying)
            {
                _currentChannel.setPaused(false);
                _wasPlaying = false;
            }
        }

        public static void RegisterVoice(string audioPath, string audioText)
        {
            var sound = AudioUtils.CreateSound(audioPath);
            var newKey = Path.GetFileNameWithoutExtension(audioPath) + "_key";
            if (!_voiceNotifications.ContainsKey(newKey))
            {
                _voiceNotifications.Add(newKey, new VoiceData
                {
                    Text = audioText, 
                    Sound = sound
                });
            }
        }

        private void CheckPendingMessages()
        {
            //QuickLogger.Debug($"Pending Message count: {_queue.Count} | Can Play: {GetCanPlay()}", true);
            if (!GetCanPlay() || !_queue.Any()) return;
            var notification = _queue.Dequeue();
            Play(notification.Key, notification.Seconds);
        }

        public bool GetCanPlay()
        {
            return DayNightCycle.main.timePassedAsFloat >= this.timeNextPlay;
        }
        
        public bool Play(string notificationKey, float seconds = 0f, params object[] args)
        {
            QuickLogger.Debug($"Attempting to play track: {notificationKey} | Is found {_voiceNotifications.ContainsKey(notificationKey)}", true);

            if (!GetCanPlay())
            {
                QuickLogger.Debug("aDDING VOICE TO QUEUE", true);
                AddToQueue(notificationKey, seconds);
                return false;
            }

            if (_voiceNotifications.ContainsKey(notificationKey))
            {
                var sound = _voiceNotifications[notificationKey].Sound;
                var text = _voiceNotifications[notificationKey].Text;

                if (!string.IsNullOrEmpty(text))
                {
                    DisplayMessage(text);
                }
                _currentChannel = AudioUtils.PlaySound(sound, SoundChannel.Master);
                timeNextPlay = DayNightCycle.main.timePassedAsFloat + seconds;
                return true;
            }

            return false;
        }

        private float GetCharDelay()
        {
            return 1f / speed;
        }
        public float speed = 15f;
        private void AddToQueue(string notificationKey, float args)
        {
            if (_queue.Any(x => x.Key.Equals(notificationKey))) return;

            _queue.Enqueue(new PendingNotification
            {
                Key = notificationKey,
                Seconds = args
            });
        }

        public bool Play(string notificationKey, string customString, params object[] args)
        {
            if (_voiceNotifications.ContainsKey(notificationKey))
            {
                var sound = _voiceNotifications[notificationKey].Sound;

                float timePassedAsFloat = DayNightCycle.main.timePassedAsFloat;
                if (timePassedAsFloat < this.timeNextPlay)
                {
                    return false;
                }
                this.timeNextPlay = timePassedAsFloat + this.minInterval;
                if (!string.IsNullOrEmpty(customString))
                {
                    DisplayMessage(customString);
                }
                AudioUtils.PlaySound(sound, SoundChannel.Voice);
                return true;
            }

            return false;
        }

        public void DisplayMessage(string text, object[] args = null)
        {
            DisplayMessage(text);
        }

        public void ShowSubtitle(string message)
        {
#if SUBNAUTICA
            Subtitles.main.Add(message, null);
#else
            Subtitles.main.AddRawLongInternal(0, new StringBuilder(message), 0, -1, -1);
#endif
        }
    }

    internal struct PendingNotification
    {
        public string Key { get; set; }
        public object[] Args { get; set; }
        public float Seconds { get; set; }
    }

    internal struct VoiceData
    {
        public string Text { get; set; }
        public Sound Sound { get; set; }
    }
}
