using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FCSCommon.Utilities;
using NAudio.Wave;
using UnityEngine;
using UnityEngine.Networking;
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
            /*
            ".ogg",*/
            ".mp3",
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
                        string fileName;
                        fileName = fileInfo.Extension.Equals(".mp3") ? GetWaveFromMp3(fileInfo.FullName) : fileInfo.FullName;
                        //var clip = await LoadClip(fileInfo.FullName);
                        UnityWebRequest request = GetAudioFromFile(fileName);
                        yield return request.SendWebRequest();
                        Playlist.Add(new TrackData {Index=Playlist.Count,  Path = fileInfo.Name, AudioClip = DownloadHandlerAudioClip.GetContent(request) });
                    }
                }
            }

            callBack?.Invoke();
            _musicScanned = true;
        }


        private static string GetWaveFromMp3(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename + " not found");
            }
            string waveFilenameFromMp = GetWaveFilenameFromMp3(filename);
            if (!File.Exists(waveFilenameFromMp))
            {
                new FileInfo(waveFilenameFromMp).Directory.Create();
                try
                {
                    using (Mp3FileReader mp3FileReader = new Mp3FileReader(filename))
                    {
                        WaveFileWriter.CreateWaveFile(waveFilenameFromMp, mp3FileReader);
                    }
                }
                catch (Exception ex)
                {
                    QuickLogger.Error($"Failed to load {Path.GetFileName(filename)} : {ex.Message}");
                    return null;
                }
                return waveFilenameFromMp;
            }
            return waveFilenameFromMp;
        }

        private static string GetWaveFilenameFromMp3(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename + " not found");
            }
            string str = CreateMD5(File.ReadAllText(filename));
            return Path.Combine(TempPath, str + ".wav");
        }

        private static string CreateMD5(string input)
        {
            string result;
            using (MD5 md = MD5.Create())
            {
                byte[] bytes = Encoding.Default.GetBytes(input);
                byte[] array = md.ComputeHash(bytes);
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    stringBuilder.Append(array[i].ToString("X2"));
                }
                result = stringBuilder.ToString();
            }
            return result;
        }

        private static string TempPath
        {
            get
            {
                return tempPath = (tempPath ?? Path.Combine(Path.GetTempPath(), "FCStudios\\Subnautica\\Mods\\JukeBox\\"));
            }
        }

        private static string tempPath;

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

