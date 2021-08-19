using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FCS_AlterraHub.Helpers;
using FCSCommon.Utilities;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using SearchOption = System.IO.SearchOption;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal class JukeBox : MonoBehaviour
    {
        private static bool _musicScanned;
        private static JukeBox _main;
        public static JukeBox Main
        {
            get
            {
                if (_main == null)
                {
                    GameObject gameObject = new GameObject("FCSJukeboxPlayer");
                    DontDestroyOnLoad(gameObject);
                    _main = gameObject.AddComponent<JukeBox>();
                    _main.StartCoroutine(ScanForMusic(null));
                }

                return _main;
            }
        }

        internal static List<TrackData> Playlist = new();

        private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            /*".mp3",
            ".ogg",*/
            ".wav",
            //".flac"
        };

        private static IEnumerator ScanForMusic(Action callBack)
        {
            var services = PlatformUtils.main.GetServices();
            var fullMusicPath = services.GetUserMusicPath();

            Playlist.Clear();

            if (!string.IsNullOrWhiteSpace(fullMusicPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(fullMusicPath);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                    directoryInfo.Refresh();
                }

                foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    if (_supportedExtensions.Contains(fileInfo.Extension))
                    {
                        //var clip = await LoadClip(fileInfo.FullName);
                        UnityWebRequest request = GetAudioFromFile(fileInfo.FullName);
                        yield return request.SendWebRequest();
                        Playlist.Add(new TrackData {Index=Playlist.Count,  Path = fileInfo.Name, AudioClip = DownloadHandlerAudioClip.GetContent(request) });
                    }
                }
            }

            callBack?.Invoke();
            _musicScanned = true;
        }

        private static UnityWebRequest GetAudioFromFile(string audioToLoad)
        {
            QuickLogger.Debug($"Trying to load audio : {audioToLoad}");
            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip($"file://{audioToLoad}", AudioType.WAV);
            return req;
        }

        public void Refresh(Action callBack)
        {
            _main.StartCoroutine(ScanForMusic(callBack));
        }

        public TrackData GetNextTrack(int currentTrack)
        {
            if (!Playlist.Any()) return new TrackData();
            currentTrack += 1;

            if (currentTrack > Playlist.Count - 1)
            {
                currentTrack = 0;
            }

            return Playlist[currentTrack];
        }

        public TrackData GetPreviousTrack(int currentTrack)
        {
            if (!Playlist.Any()) return new TrackData();
            currentTrack -= 1;

            if (currentTrack < 0)
            {
                currentTrack = Playlist.Count - 1;
            }

            return Playlist[currentTrack];
        }

        public TrackData GetFirstTrack()
        {
            return Playlist.FirstOrDefault();
        }
    }

    internal struct TrackData
    {
        public int Index { get; set; }
        public string Path { get; set; }
        public AudioClip AudioClip { get; set; }
    }
}

