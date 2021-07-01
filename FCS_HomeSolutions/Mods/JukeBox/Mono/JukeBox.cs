using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using FCS_AlterraHub.Helpers;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Object = UnityEngine.Object;
using SearchOption = System.IO.SearchOption;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal class JukeBox : MonoBehaviour
    {
        private static JukeBox _main;
        private static float _volume;
        private static bool _shuffle;
        private bool _pausedDirty;
        private bool _paused;
        private int _failed;
        private string _fullMusicPath;
        private string _file;
        private uint _length;
        private JukeBoxInstance _instance;
        private ChannelGroup _eventInstanceChannelGroup;
        private Repeat _repeat = Repeat.All;
        private Bus _bus;
        private EventInstance _eventInstance;
        private Channel _channel;
        private Sound _sound;
        private List<string> _playlist = new();
        private List<string> _shuffled = new();
        private List<string> _pendingInfo = new();
        private Dictionary<string, TrackInfo> _info = new();
        private readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".ogg",
            ".wav",
            ".flac"
        };

        private static readonly string[] authoredMusic = {
            "event:/jukebox/jukebox_one"
        };

        private CREATESOUNDEXINFO _exinfo;


        public static bool IsStartingOrPlaying => _main != null && !string.IsNullOrWhiteSpace(_main._file);
        public JukeBoxInstance Instance => _main == null ? null : _main._instance;
        public static string FullMusicPath => Main._fullMusicPath;
        public uint Length => _main == null ? 0U : _main._length;
        public static Repeat Repeat => Main._repeat;

        private static readonly Dictionary<string, string> musicLabels = new()
        {
            {
                "event:/jukebox/jukebox_one",
                "Subnautica - Jukebox One"
            }
        };

        private ChannelGroup _channelGroup;
        private bool _busChannelGroupLocked;
        private uint _position;
        private bool _release;

        public static JukeBox Main
        {
            get
            {
                if (_main == null)
                {
                    GameObject gameObject = new GameObject("JukeboxPlayer");
                    DontDestroyOnLoad(gameObject);
                    _main = gameObject.AddComponent<JukeBox>();
                    Scan();
                }
                return _main;
            }
        }

        public static bool Shuffle
        {
            get => _shuffle;
            set => _shuffle = value;
        }

        public static float Volume
        {
            get => _volume;
            set
            {
                if (!Mathf.Approximately(_volume, value))
                {
                    _volume = value;
                }
            }
        }

        public static bool Paused
        {
            get => Main._paused;
            set
            {
                if (Main._paused != value)
                {
                    Main._paused = value;
                    Main._pausedDirty = true;
                }
            }
        }

        private void Awake()
        {
            var services = PlatformUtils.main.GetServices();
            _fullMusicPath = services.GetUserMusicPath();
            _exinfo = default;
            _exinfo.cbsize = Marshal.SizeOf(_exinfo);
            var list = new List<string>(authoredMusic);

            for (int i = 0; i < list.Count; i++)
            {
                var text = list[i];
                int length;
                ERRCHECK(RuntimeManager.GetEventDescription(text).getLength(out length));
                var info = default(TrackInfo);
                info.Length = (uint)length;
                if (!musicLabels.TryGetValue(text, out info.Label))
                {
                    info.Label = $"Subnautica - {CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.Substring(text.LastIndexOf('/') + 1).Replace('_', ' '))}";
                }

                SetInfo(text, info);
            }
        }

        private void Update()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_channelGroup.hasHandle())
            {
                return;
            }
            RuntimeUtils.EnforceLibraryOrder();
            if (!_bus.hasHandle() &&
                RuntimeManager.StudioSystem.getBus("{f5cd1372-6a56-4fef-bd4b-b33143d09538}", out _bus) != RESULT.OK)
            {
                return;
            }

            if (!_busChannelGroupLocked)
            {
                if (_bus.lockChannelGroup() != RESULT.OK)
                {
                    return;
                }

                _busChannelGroupLocked = true;
            }

            if (_bus.getChannelGroup(out _channelGroup) != RESULT.OK)
            {
                return;
            }

            //RuntimeManager.

            //TODO Comeback here
        }

        private void SetInfo(string id, TrackInfo info)
        {
            _info[id] = info;
            JukeBoxInstance.NotifyInfo(id, info);
        }

        private static bool ERRCHECK(RESULT result)
        {
            return FMODUWE.CheckResult(result);
        }
        
        private static void Scan()
        {
            Main.ScanInternal();
        }

        private void ScanInternal()
        {
            _failed = 0;
            _playlist.Clear();
            _shuffled.Clear();
            _pendingInfo.Clear();
            if (!string.IsNullOrWhiteSpace(_fullMusicPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(_fullMusicPath);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                    directoryInfo.Refresh();
                }

                foreach (var fileInfo in directoryInfo.GetFiles("*",SearchOption.TopDirectoryOnly))
                {
                    if (_supportedExtensions.Contains(fileInfo.Extension))
                    {
                        _playlist.Add(fileInfo.Name);
                    }
                }
            }

            for (int i = _playlist.Count -1; i >= 0; i--)
            {
                var text = _playlist[i];
                if (!_info.ContainsKey(text))
                {
                    _pendingInfo.Add(text);
                }
            }

            var list = new List<string>();
            foreach (KeyValuePair<string, TrackInfo> info in _info)
            {
                var key = info.Key;
                if (!_playlist.Contains(key) && !IsEvent(key))
                {
                    list.Add(key);
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                _info.Remove(list[i]);
            }
            _playlist.AddRange(authoredMusic);
            SortPlayList();
            _shuffled.AddRange(_playlist);
            MathHelpers.ShuffleAll(_shuffled);
        }

        private void SortPlayList()
        {
            _playlist.Sort(PlaylistComparer);
        }

        private int PlaylistComparer(string x, string y)
        {
            bool flag = IsEvent(x);
            bool flag2 = IsEvent(y);
            if (flag == flag2)
            {
                if (flag)
                {
                    string strA = x;
                    string strB = y;
                    if (_info.TryGetValue(x, out var trackInfo))
                    {
                        strA = trackInfo.Label;
                    }

                    if (_info.TryGetValue(y, out var trackInfo2))
                    {
                        strB = trackInfo2.Label;
                    }
                    return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
                }
                return string.Compare(x, y, StringComparison.Ordinal);
            }

            if (!flag)
            {
                return -1;
            }
            return 1;
        }
        public static TrackInfo GetInfo(string id)
        {
            TrackInfo result;
            if (!string.IsNullOrWhiteSpace(id))
            {
                if (Main._info.TryGetValue(id, out result))
                {
                    return result;
                }

                if (!IsEvent(id))
                {
                    Main._pendingInfo.Remove(id);
                    Main._pendingInfo.Add(id);
                }
            }

            result = DummyTrackInfo(id);
            return result;
        }

        private static bool IsEvent(string id)
        {
            return id.StartsWith("event:", StringComparison.OrdinalIgnoreCase);
        }

        private static TrackInfo DummyTrackInfo(string id)
        {
            return new()
            {
                Label = id,
                Length = 0U
            };
        }

        private void PlayInternal(JukeBoxInstance instance)
        {
            if (_instance != instance)
            {
                ReleaseInstance();
                _instance = instance;
                _volume = _instance.Volume;
                _repeat = _instance.Repeat;
                _shuffle = _instance.Shuffle;
                _instance.OnControl();
            }

            var file = _instance.File;
            _failed = 0;
            if (string.Equals(file, _file, StringComparison.Ordinal))
            {
                _position = 0U;
                _paused = false;
                return;
            }

            _release = true;
            _file = file;
            _paused = false;
        }

        private void StopInternal()
        {
            _failed = 0;
            ReleaseInstance();
            _release = true;
            _file = null;
            _paused = false;
        }

        public bool HasFileInternal(string file)
        {
            return this._playlist.Contains(file);
        }

        private void ReleaseInstance()
        {
            if (_instance != null)
            {
                _instance.OnRelease();
                _instance = null;
            }
        }

        private void Release()
        {
            //if (this._eventInstance.isValid())
            //{
            //    if (this._eventInstanceChannelGroup.hasHandle())
            //    {
            //        ERRCHECK(this._eventInstanceChannelGroup.removeDSP(this._fft));
            //    }
            //    ERRCHECK(this._eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
            //    this._eventInstance.clearHandle();
            //    this._position = 0U;
            //}
            //this._eventInstanceChannelGroup.clearHandle();
            //if (_channel.hasHandle())
            //{
            //    RESULT result = _channel.removeDSP(this._fft);
            //    if (result != RESULT.ERR_INVALID_HANDLE && result != RESULT.ERR_CHANNEL_STOLEN)
            //    {
            //        ERRCHECK(result);
            //    }
            //    result = this._channel.stop();
            //    if (result != RESULT.ERR_INVALID_HANDLE && result != RESULT.ERR_CHANNEL_STOLEN)
            //    {
            //        ERRCHECK(result);
            //    }
            //    this._channel.clearHandle();
            //}
            //if (this._sound.hasHandle())
            //{
            //    ERRCHECK(this._sound.release());
            //    this._sound.clearHandle();
            //    this._position = 0U;
            //}
        }
    }

    internal enum Repeat
    {
        Off,
        Track,
        All
    }

    public struct TrackInfo
    {
        public string Label;
        public uint Length;
    }
}
