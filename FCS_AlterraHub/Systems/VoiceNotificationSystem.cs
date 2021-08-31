using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            if (Time.timeScale <= 0f && isPlaying)
            {
                _currentChannel.setPaused(true);
                _wasPlaying = true;
            }

            if (Time.timeScale > 0 && _wasPlaying)
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
            if (!GetCanPlay() || !_queue.Any()) return;
            var notification = _queue.Dequeue();
            Play(notification.Key, notification.Args);
        }

        public bool GetCanPlay()
        {
            return DayNightCycle.main.timePassedAsFloat >= this.timeNextPlay;
        }

        public bool Play(string notificationKey, params object[] args)
        {
            if (!GetCanPlay())
            {
                AddToQueue(notificationKey, args);
            }
            if (_voiceNotifications.ContainsKey(notificationKey))
            {
                var sound = _voiceNotifications[notificationKey].Sound;
                var text = _voiceNotifications[notificationKey].Text;

                float timePassedAsFloat = DayNightCycle.main.timePassedAsFloat;
                if (timePassedAsFloat < this.timeNextPlay)
                {
                    return false;
                }
                this.timeNextPlay = timePassedAsFloat + this.minInterval;
                if (!string.IsNullOrEmpty(text))
                {
                    Subtitles.main.Add(text, args);
                }
                _currentChannel = AudioUtils.PlaySound(sound, SoundChannel.Master);
                return true;
            }
            
            return false;
        }

        private void AddToQueue(string notificationKey, object[] args)
        {
            if (_queue.Any(x => x.Key.Equals(notificationKey))) return;

            _queue.Enqueue(new PendingNotification
            {
                Key = notificationKey,
                Args = args
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
                    Subtitles.main.Add(customString, args);
                }
                AudioUtils.PlaySound(sound, SoundChannel.Voice);
                return true;
            }

            return false;
        }

        public void DisplayMessage(string text, object[] args = null)
        {
            Subtitles.main.Add(text, args);
        }
    }

    internal struct PendingNotification
    {
        public string Key { get; set; }
        public object[] Args { get; set; }
    }

    internal struct VoiceData
    {
        public string Text { get; set; }
        public Sound Sound { get; set; }
    }
}
