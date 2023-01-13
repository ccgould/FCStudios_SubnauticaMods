using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Gendarme;
using SMLHelper.Utility;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    public class JukeboxV2 : MonoBehaviour, ICompileTimeCheckable
    {
        // Token: 0x170001C2 RID: 450
        // (get) Token: 0x06002536 RID: 9526 RVA: 0x000B289A File Offset: 0x000B0A9A
        internal static JukeboxV2 main
        {
            get
            {
                if (_main == null)
                {
                    GameObject gameObject = new GameObject("JukeboxPlayer");
                    DontDestroyOnLoad(gameObject);
                    _main = gameObject.AddComponent<JukeboxV2>();
                    Scan();
                }
                return _main;
            }
        }

        // Token: 0x170001C3 RID: 451
        // (get) Token: 0x06002537 RID: 9527 RVA: 0x000B28CD File Offset: 0x000B0ACD
        public static bool isStartingOrPlaying
        {
            get
            {
                return _main != null && !string.IsNullOrEmpty(_main._file);
            }
        }

        // Token: 0x170001C4 RID: 452
        // (get) Token: 0x06002538 RID: 9528 RVA: 0x000B28F0 File Offset: 0x000B0AF0
        // (set) Token: 0x06002539 RID: 9529 RVA: 0x000B28FC File Offset: 0x000B0AFC
        public static JukeboxV2.Repeat repeat
        {
            get
            {
                return main._repeat;
            }
            set
            {
                if (main._repeat != value)
                {
                    main._repeat = value;
                }
            }
        }

        // Token: 0x170001C5 RID: 453
        // (get) Token: 0x0600253A RID: 9530 RVA: 0x000B2916 File Offset: 0x000B0B16
        // (set) Token: 0x0600253B RID: 9531 RVA: 0x000B2922 File Offset: 0x000B0B22
        public static bool shuffle
        {
            get
            {
                return main._shuffle;
            }
            set
            {
                if (main._shuffle != value)
                {
                    main._shuffle = value;
                }
            }
        }

        // Token: 0x170001C6 RID: 454
        // (get) Token: 0x0600253C RID: 9532 RVA: 0x000B293C File Offset: 0x000B0B3C
        // (set) Token: 0x0600253D RID: 9533 RVA: 0x000B2948 File Offset: 0x000B0B48
        public static float volume
        {
            get
            {
                return main._volume;
            }
            set
            {
                if (!Mathf.Approximately(main._volume, value))
                {
                    main._volume = value;
                }
            }
        }

        // Token: 0x170001C7 RID: 455
        // (get) Token: 0x0600253E RID: 9534 RVA: 0x000B2967 File Offset: 0x000B0B67
        // (set) Token: 0x0600253F RID: 9535 RVA: 0x000B2973 File Offset: 0x000B0B73
        public static uint position
        {
            get
            {
                return main._position;
            }
            set
            {
                if (main._position != value)
                {
                    main._position = value;
                    main._positionDirty = true;
                }
            }
        }

        // Token: 0x170001C8 RID: 456
        // (get) Token: 0x06002540 RID: 9536 RVA: 0x000B2998 File Offset: 0x000B0B98
        // (set) Token: 0x06002541 RID: 9537 RVA: 0x000B29A4 File Offset: 0x000B0BA4
        public static bool paused
        {
            get
            {
                return main._paused;
            }
            set
            {
                if (main._paused != value)
                {
                    main._paused = value;
                    main._pausedDirty = true;
                }
            }
        }

        // Token: 0x170001C9 RID: 457
        // (get) Token: 0x06002542 RID: 9538 RVA: 0x000B29C9 File Offset: 0x000B0BC9
        public static uint length
        {
            get
            {
                if (!(_main != null))
                {
                    return 0U;
                }
                return _main._length;
            }
        }

        // Token: 0x170001CA RID: 458
        // (get) Token: 0x06002543 RID: 9539 RVA: 0x000B29E4 File Offset: 0x000B0BE4
        public static List<float> spectrum
        {
            get
            {
                if (!(_main != null))
                {
                    return null;
                }
                return _main._spectrum;
            }
        }

        // Token: 0x170001CB RID: 459
        // (get) Token: 0x06002544 RID: 9540 RVA: 0x000B29FF File Offset: 0x000B0BFF
        public static JukeboxInstance instance
        {
            get
            {
                if (!(_main != null))
                {
                    return null;
                }
                return _main._instance;
            }
        }

        // Token: 0x170001CC RID: 460
        // (get) Token: 0x06002545 RID: 9541 RVA: 0x000B2A1A File Offset: 0x000B0C1A
        public static string fullMusicPath
        {
            get
            {
                return main._fullMusicPath;
            }
        }

        // Token: 0x06002546 RID: 9542 RVA: 0x000B2A28 File Offset: 0x000B0C28
        [SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
        private void Awake()
        {
            PlatformServices services = PlatformUtils.main.GetServices();
            this._fullMusicPath = services.GetUserMusicPath();
            this._exinfo = default(CREATESOUNDEXINFO);
            this._exinfo.cbsize = Marshal.SizeOf<CREATESOUNDEXINFO>(this._exinfo);
            List<string> list = new List<string>(authoredMusic);
            using (Dictionary<JukeboxV2.UnlockableTrack, string>.Enumerator enumerator = unlockableMusic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    List<string> list2 = list;
                    KeyValuePair<JukeboxV2.UnlockableTrack, string> keyValuePair = enumerator.Current;
                    list2.Add(keyValuePair.Value);
                }
            }
            //for (int i = 0; i < list.Count; i++)
            //{
                //string text = list[i];
                //int length;
                //ERRCHECK(RuntimeManager.GetEventDescription(text).getLength(out length));
                //JukeboxV2.TrackInfo info = default(JukeboxV2.TrackInfo);
                //info.length = (uint)length;
                //if (!musicLabels.TryGetValue(text, out info.label))
                //{
                //    info.label = string.Format("Subnautica - {0}", CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.Substring(text.LastIndexOf('/') + 1).Replace('_', ' ')));
                //}
                //this.SetInfo(text, info);
            //}
        }

        // Token: 0x06002547 RID: 9543 RVA: 0x000B2B6C File Offset: 0x000B0D6C
        private void Update()
        {
            this.Initialize();
            if (!this._channelGroup.hasHandle())
            {
                return;
            }
            if (this._instance == null)
            {
                this._release = true;
            }
            if (this._release)
            {
                this._release = false;
                this.Release();
            }
            this._db = 0f;
            this._peak = 0f;
            this._spectrum.Clear();
            this._dbLevels.Clear();
            this._peakLevels.Clear();
            this.UpdateStudio();
            this.UpdateLowLevel();
            this.UpdateGameMusicMute();
            this.UpdateInfo();
        }

        // Token: 0x06002548 RID: 9544 RVA: 0x000B2C08 File Offset: 0x000B0E08
        private void UpdateStudio()
        {
            if (!this._eventInstance.isValid())
            {
                if (!string.IsNullOrEmpty(this._file) && IsEvent(this._file))
                {
                    EventDescription eventDescription = RuntimeManager.GetEventDescription(this._file);
                    int length;
                    ERRCHECK(eventDescription.getLength(out length));
                    this._length = (uint)length;
                    ERRCHECK(eventDescription.createInstance(out this._eventInstance));
                    this._eventInstance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, minDistance);
                    this._eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
                    return;
                }
            }
            else if (!this._eventInstanceChannelGroup.hasHandle())
            {
                this._eventInstance.getChannelGroup(out this._eventInstanceChannelGroup);
                if (this._eventInstanceChannelGroup.hasHandle())
                {
                    this.SetParameters();
                    ERRCHECK(this._eventInstance.setVolume(this._volume));
                    ERRCHECK(this._eventInstance.setTimelinePosition((int)this._position));
                    this._eventInstance.start();
                    this._positionDirty = false;
                    ERRCHECK(this._eventInstanceChannelGroup.addDSP(-3, this._fft));
                    ERRCHECK(this._eventInstance.setPaused(this._paused));
                    this._pausedDirty = false;
                    this._instance.file = this._file;
                    return;
                }
            }
            else
            {
                bool flag = false;
                PLAYBACK_STATE playback_STATE;
                ERRCHECK(this._eventInstance.getPlaybackState(out playback_STATE));
                if (playback_STATE == PLAYBACK_STATE.STOPPED)
                {
                    flag = true;
                }
                if (flag)
                {
                    this.HandleLooping();
                    return;
                }
                if (this._positionDirty)
                {
                    this._positionDirty = false;
                    if (this._eventInstance.setTimelinePosition((int)this._position) != RESULT.OK)
                    {
                        this.HandleLooping();
                    }
                }
                ERRCHECK(this._eventInstance.setVolume(this._volume));
                if (this._pausedDirty)
                {
                    this._pausedDirty = false;
                    ERRCHECK(this._eventInstance.setPaused(this._paused));
                }
                int position;
                if (playback_STATE == PLAYBACK_STATE.PLAYING && this._eventInstance.getTimelinePosition(out position) == RESULT.OK)
                {
                    this._position = (uint)position;
                }
                this.SetParameters();
                this.UpdateSpectrum();
            }
        }

        // Token: 0x06002549 RID: 9545 RVA: 0x000B2E10 File Offset: 0x000B1010
        private void UpdateLowLevel()
        {
            if (!this._sound.hasHandle())
            {
                if (!string.IsNullOrEmpty(this._file) && !IsEvent(this._file))
                {
                    if (this._playlist.IndexOf(this._file, StringComparer.Ordinal) < 0)
                    {
                        this._file = null;
                        return;
                    }
                    string text = Path.Combine(this._fullMusicPath, this._file);
                    if (!File.Exists(text))
                    {
                        this.HandleOpenError();
                        return;
                    }
                    ERRCHECK(RuntimeManager.CoreSystem.createSound(text, MODE._3D | MODE.CREATESTREAM | MODE.ACCURATETIME | MODE.NONBLOCKING | MODE._3D_LINEARSQUAREROLLOFF, ref this._exinfo, out this._sound));
                    JukeboxV2.TrackInfo trackInfo;
                    if (this._info.TryGetValue(this._file, out trackInfo))
                    {
                        this._length = trackInfo.length;
                        return;
                    }
                }
            }
            else
            {
                OPENSTATE openstate = OPENSTATE.READY;
                uint num;
                bool flag;
                bool flag2;
                ERRCHECK(this._sound.getOpenState(out openstate, out num, out flag, out flag2));
                if (!this._channel.hasHandle())
                {
                    FMOD.System coreSystem = RuntimeManager.CoreSystem;
                    if (openstate == OPENSTATE.READY)
                    {
                        this._failed = 0;
                        ERRCHECK(this._sound.setLoopCount(1));
                        ERRCHECK(coreSystem.playSound(this._sound, this._channelGroup, true, out this._channel));
                        ERRCHECK(this._sound.getLength(out this._length, TIMEUNIT.MS));
                        ERRCHECK(this._channel.setPriority(64));
                        ERRCHECK(this._channel.set3DMinMaxDistance(minDistance, maxDistance));
                        this.SetParameters();
                        ERRCHECK(this._channel.setVolume(this._volume));
                        ERRCHECK(this._channel.setPosition(this._position, TIMEUNIT.MS));
                        this._positionDirty = false;
                        ERRCHECK(this._channel.addDSP(-3, this._fft));
                        ERRCHECK(this._channel.setPaused(this._paused));
                        this._pausedDirty = false;
                        this._instance.file = this._file;
                        return;
                    }
                    if (openstate == OPENSTATE.ERROR)
                    {
                        this._release = true;
                        this.HandleOpenError();
                        return;
                    }
                }
                else
                {
                    bool flag3 = false;
                    if (openstate == OPENSTATE.READY)
                    {
                        flag3 = true;
                    }
                    else
                    {
                        bool flag4;
                        RESULT result = this._channel.isPlaying(out flag4);
                        if (result == RESULT.ERR_INVALID_HANDLE || result == RESULT.ERR_CHANNEL_STOLEN)
                        {
                            flag3 = true;
                        }
                    }
                    if (flag3)
                    {
                        this.HandleLooping();
                        return;
                    }
                    if (this._positionDirty && (openstate == OPENSTATE.READY || openstate == OPENSTATE.PLAYING))
                    {
                        this._positionDirty = false;
                        if (this._channel.setPosition(this._position, TIMEUNIT.MS) != RESULT.OK)
                        {
                            this.HandleLooping();
                        }
                    }
                    ERRCHECK(this._channel.setVolume(this._volume));
                    if (this._pausedDirty)
                    {
                        this._pausedDirty = false;
                        ERRCHECK(this._channel.setPaused(this._paused));
                    }
                    if (openstate == OPENSTATE.PLAYING)
                    {
                        ERRCHECK(this._channel.getPosition(out this._position, TIMEUNIT.MS));
                    }
                    this.SetParameters();
                    this.UpdateSpectrum();
                }
            }
        }

        // Token: 0x0600254A RID: 9546 RVA: 0x000B30F4 File Offset: 0x000B12F4
        private void UpdateInfo()
        {
            if (!this._infoSound.hasHandle())
            {
                if (this._pendingInfo.Count == 0)
                {
                    return;
                }
                int index = this._pendingInfo.Count - 1;
                string text = this._pendingInfo[index];
                string text2 = Path.Combine(this._fullMusicPath, text);
                if (File.Exists(text2))
                {
                    this._infoFile = text;
                    ERRCHECK(RuntimeManager.CoreSystem.createSound(text2, MODE._2D | MODE.CREATESTREAM | MODE.OPENONLY | MODE.ACCURATETIME | MODE.NONBLOCKING, ref this._exinfo, out this._infoSound));
                    return;
                }
                this._pendingInfo.RemoveAt(index);
                JukeboxV2.TrackInfo info = new JukeboxV2.TrackInfo
                {
                    label = text,
                    length = 0U
                };
                this.SetInfo(text, info);
                return;
            }
            else
            {
                OPENSTATE openstate = OPENSTATE.READY;
                uint num;
                bool flag;
                bool flag2;
                ERRCHECK(this._infoSound.getOpenState(out openstate, out num, out flag, out flag2));
                if (openstate == OPENSTATE.READY)
                {
                    int num2 = this._pendingInfo.IndexOf(this._infoFile);
                    if (num2 >= 0)
                    {
                        this._pendingInfo.RemoveAt(num2);
                        JukeboxV2.TrackInfo info2 = default(JukeboxV2.TrackInfo);
                        string text3;
                        string text4;
                        this.TryGetArtistAndTitle(this._infoSound, out text3, out text4);
                        if (!string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(text4))
                        {
                            info2.label = string.Format("{0} - {1}", text3, text4);
                        }
                        else
                        {
                            info2.label = Path.GetFileNameWithoutExtension(this._infoFile).Replace('_', ' ');
                        }
                        ERRCHECK(this._infoSound.getLength(out info2.length, TIMEUNIT.MS));
                        this.SetInfo(this._infoFile, info2);
                    }
                    this._infoFile = null;
                    this._infoSound.release();
                    this._infoSound.clearHandle();
                    return;
                }
                if (openstate == OPENSTATE.ERROR)
                {
                    int num3 = this._pendingInfo.IndexOf(this._infoFile);
                    if (num3 >= 0)
                    {
                        this._pendingInfo.RemoveAt(num3);
                        JukeboxV2.TrackInfo info3 = default(JukeboxV2.TrackInfo);
                        info3.label = Path.GetFileNameWithoutExtension(this._infoFile);
                        info3.length = 0U;
                        this.SetInfo(this._infoFile, info3);
                    }
                    this._infoFile = null;
                    this._infoSound.release();
                    this._infoSound.clearHandle();
                }
                return;
            }
        }

        // Token: 0x0600254B RID: 9547 RVA: 0x000B3314 File Offset: 0x000B1514
        private void TryGetArtistAndTitle(Sound sound, out string artist, out string title)
        {
            artist = null;
            title = null;
            int num;
            int num2;
            ERRCHECK(sound.getNumTags(out num, out num2));
            StringBuilder stringBuilder = new StringBuilder();
            int i = 0;
            while (i < num)
            {
                TAG tag;
                sound.getTag(null, i, out tag);
                TAGDATATYPE datatype = tag.datatype;
                string text;
                if (datatype == TAGDATATYPE.STRING)
                {
                    text = Marshal.PtrToStringAnsi(tag.data, (int)tag.datalen);
                    goto IL_76;
                }
                if (datatype == TAGDATATYPE.STRING_UTF16)
                {
                    text = Marshal.PtrToStringUni(tag.data, (int)(tag.datalen / 2U));
                    goto IL_76;
                }
            IL_13B:
                i++;
                continue;
            IL_76:
                if (string.IsNullOrEmpty(text))
                {
                    goto IL_13B;
                }
                stringBuilder.Length = 0;
                foreach (char c in text)
                {
                    if (!char.IsControl(c))
                    {
                        stringBuilder.Append(c);
                    }
                }
                if (stringBuilder.Length == 0)
                {
                    goto IL_13B;
                }
                text = stringBuilder.ToString();
                string a = tag.name;
                if (!(a == "ARTIST"))
                {
                    if (a == "TPE1")
                    {
                        artist = text;
                        goto IL_13B;
                    }
                    if (!(a == "TITLE"))
                    {
                        if (!(a == "TIT2"))
                        {
                            goto IL_13B;
                        }
                        title = text;
                        goto IL_13B;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(title))
                        {
                            title = text;
                            goto IL_13B;
                        }
                        goto IL_13B;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(artist))
                    {
                        artist = text;
                        goto IL_13B;
                    }
                    goto IL_13B;
                }
            }
        }

        // Token: 0x0600254C RID: 9548 RVA: 0x000B346C File Offset: 0x000B166C
        private void HandleOpenError()
        {
            this._failed++;
            if (this._repeat != Repeat.All)
            {
                this._file = null;
                return;
            }
            int num = this._shuffle ? this._shuffled.Count : this._playlist.Count;
            if (this._failed < num)
            {
                this._file = this.GetNextInternal(this._file, true, this._shuffle, true);
                return;
            }
            this._file = null;
        }

        // Token: 0x0600254D RID: 9549 RVA: 0x000B34E4 File Offset: 0x000B16E4
        private void HandleLooping()
        {
            this._release = true;
            if (this._repeat == Repeat.All)
            {
                this._file = this.GetNextInternal(this._file, true, this._shuffle, true);
                return;
            }
            if (this._repeat == Repeat.Track)
            {
                position = 0U;
                return;
            }
            this._file = null;
            this._paused = false;
        }

        // Token: 0x0600254E RID: 9550 RVA: 0x000B353A File Offset: 0x000B173A
        private void OnApplicationQuit()
        {
            this.DeinitializeInternal();
        }

        // Token: 0x0600254F RID: 9551 RVA: 0x000B3542 File Offset: 0x000B1742
        public static void Deinitialize()
        {
            if (_main != null)
            {
                _main.DeinitializeInternal();
            }
        }

        // Token: 0x06002550 RID: 9552 RVA: 0x000B355B File Offset: 0x000B175B
        public static void Scan()
        {
            main.ScanInternal();
        }

        // Token: 0x06002551 RID: 9553 RVA: 0x000B3567 File Offset: 0x000B1767
        public static void Play(JukeboxInstance instance)
        {
            main.PlayInternal(instance);
        }

        // Token: 0x06002552 RID: 9554 RVA: 0x000B3574 File Offset: 0x000B1774
        public static void Stop()
        {
            main.StopInternal();
        }

        // Token: 0x06002553 RID: 9555 RVA: 0x000B3580 File Offset: 0x000B1780
        public static string GetNext(JukeboxInstance jukebox, bool forward)
        {
            if (jukebox == null)
            {
                return null;
            }
            bool allowReshuffle = main._instance != null && main._instance == jukebox;
            return main.GetNextInternal(jukebox.file, forward, jukebox.shuffle, allowReshuffle);
        }

        // Token: 0x06002554 RID: 9556 RVA: 0x000B35D6 File Offset: 0x000B17D6
        public static bool HasFile(string file)
        {
            return main.HasFileInternal(file);
        }

        // Token: 0x06002555 RID: 9557 RVA: 0x000B35E3 File Offset: 0x000B17E3
        public static JukeboxV2.Repeat GetNextRepeat(JukeboxV2.Repeat current)
        {
            switch (current)
            {
                case Repeat.Off:
                    return Repeat.Track;
                case Repeat.Track:
                    return Repeat.All;
                case Repeat.All:
                    return Repeat.Off;
                default:
                    return Repeat.All;
            }
        }

        // Token: 0x06002556 RID: 9558 RVA: 0x000B3600 File Offset: 0x000B1800
        private void OnUnlock(JukeboxV2.UnlockableTrack track, bool notify)
        {
            string text;
            if (unlockableMusic.TryGetValue(track, out text) && !this._playlist.Contains(text))
            {
                this._playlist.Add(text);
                this._shuffled.Insert(UnityEngine.Random.Range(0, this._shuffled.Count), text);

            }
        }

        // Token: 0x06002557 RID: 9559 RVA: 0x000B3678 File Offset: 0x000B1878
        public static void Unlock(JukeboxV2.UnlockableTrack track, bool notify = true)
        {
            Player main = Player.main;
            if (main == null)
            {
                return;
            }
        }

        // Token: 0x06002558 RID: 9560 RVA: 0x000B36C4 File Offset: 0x000B18C4
        public static void UnlockAll()
        {
            foreach (KeyValuePair<JukeboxV2.UnlockableTrack, string> keyValuePair in unlockableMusic)
            {
                Unlock(keyValuePair.Key, false);
            }
        }

        // Token: 0x06002559 RID: 9561 RVA: 0x000B371C File Offset: 0x000B191C
        public static void Deserialize(List<JukeboxV2.UnlockableTrack> tracks)
        {
            if (tracks == null)
            {
                return;
            }
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                JukeboxV2.UnlockableTrack unlockableTrack = tracks[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    if (tracks[j] == unlockableTrack)
                    {
                        tracks.RemoveAt(i);
                        break;
                    }
                }
            }
            for (int k = 0; k < tracks.Count; k++)
            {
                main.OnUnlock(tracks[k], false);
            }
            main.SortPlaylist();
        }

        // Token: 0x0600255A RID: 9562 RVA: 0x000B3798 File Offset: 0x000B1998
        private static EventInstance CreateSnapshot(string path)
        {
            EventInstance result = RuntimeManager.CreateInstance(path);
            if (!result.hasHandle())
            {
                UnityEngine.Debug.LogErrorFormat("Snapshot is not found at path '{0}'", new object[]
                {
                path
                });
            }
            return result;
        }

        // Token: 0x0600255B RID: 9563 RVA: 0x000B37CC File Offset: 0x000B19CC
        [SuppressMessage("Gendarme.Rules.Portability", "DoNotHardcodePathsRule")]
        private void Initialize()
        {
            if (this._channelGroup.hasHandle())
            {
                return;
            }
            RuntimeUtils.EnforceLibraryOrder();

            try
            {
                //AudioUtils.BusPaths.Music
                _bus = RuntimeManager.GetBus("bus:/master/SFX_for_pause/nofilter/music");
            }
            catch (Exception)
            {

                return;
            }

            if (!this._bus.hasHandle())
            {
                return;
            }
            if (!this._busChannelGroupLocked)
            {
                if (this._bus.lockChannelGroup() != RESULT.OK)
                {
                    return;
                }
                this._busChannelGroupLocked = true;
            }
            if (this._bus.getChannelGroup(out this._channelGroup) != RESULT.OK)
            {
                return;
            }
            RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out this._fft);
            this._fft.setMeteringEnabled(true, false);
            this._fft.setParameterInt(1, 0);
            this._fft.setParameterInt(0, 128);

            //this.snapshotOn = CreateSnapshot("snapshot:/jukebox_on");
            //this.snapshotMute = CreateSnapshot("snapshot:/jukebox_mute");
            //this.snapshotMuffle = CreateSnapshot("snapshot:/jukebox_muffle");
            //this.snapshotReverb = CreateSnapshot("snapshot:/jukebox_verb_small");
        }

        // Token: 0x0600255C RID: 9564 RVA: 0x000B38CC File Offset: 0x000B1ACC
        private void DeinitializeInternal()
        {
            this._file = null;
            this.ReleaseInstance();
            this.Release();
            if (this._fft.hasHandle())
            {
                ERRCHECK(this._fft.release());
                this._fft.clearHandle();
            }
            //this.SetSnapshotState(this.snapshotOn, ref this.stateOn, false);
            //this.SetSnapshotState(this.snapshotMute, ref this.stateMute, false);
            //this.SetSnapshotState(this.snapshotMuffle, ref this.stateMuffle, false);
            //this.SetSnapshotState(this.snapshotReverb, ref this.stateReverb, false);
            //ERRCHECK(this.snapshotOn.release());
            //ERRCHECK(this.snapshotMute.release());
            //ERRCHECK(this.snapshotMuffle.release());
            //ERRCHECK(this.snapshotReverb.release());
            //this.snapshotOn.clearHandle();
            //this.snapshotMute.clearHandle();
            //this.snapshotMuffle.clearHandle();
            //this.snapshotReverb.clearHandle();
            if (this._bus.isValid() && this._busChannelGroupLocked)
            {
                ERRCHECK(this._bus.unlockChannelGroup());
            }
            this._busChannelGroupLocked = false;
            this._bus.clearHandle();
            this._channelGroup.clearHandle();
        }

        // Token: 0x0600255D RID: 9565 RVA: 0x000B3A14 File Offset: 0x000B1C14
        private void ScanInternal()
        {
            this._failed = 0;
            this._playlist.Clear();
            this._shuffled.Clear();
            this._pendingInfo.Clear();
            if (!string.IsNullOrEmpty(this._fullMusicPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(this._fullMusicPath);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                    directoryInfo.Refresh();
                }
                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    if (this.supportedExtensions.Contains(fileInfo.Extension))
                    {
                        string name = fileInfo.Name;
                        this._playlist.Add(name);
                    }
                }
            }
            for (int j = this._playlist.Count - 1; j >= 0; j--)
            {
                string text = this._playlist[j];
                if (!this._info.ContainsKey(text))
                {
                    this._pendingInfo.Add(text);
                }
            }
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, JukeboxV2.TrackInfo> keyValuePair in this._info)
            {
                string key = keyValuePair.Key;
                if (!this._playlist.Contains(key) && !IsEvent(key))
                {
                    list.Add(key);
                }
            }
            for (int k = 0; k < list.Count; k++)
            {
                this._info.Remove(list[k]);
            }
            this._playlist.AddRange(authoredMusic);
            this.SortPlaylist();
            this._shuffled.AddRange(this._playlist);
            MathExtensions.ShuffleAll<string>(this._shuffled, -1);
        }

        // Token: 0x0600255E RID: 9566 RVA: 0x000B3C40 File Offset: 0x000B1E40
        private void SortPlaylist()
        {
            this._playlist.Sort(new Comparison<string>(this.PlaylistComparer));
        }

        // Token: 0x0600255F RID: 9567 RVA: 0x000B3C5C File Offset: 0x000B1E5C
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
                    JukeboxV2.TrackInfo trackInfo;
                    if (this._info.TryGetValue(x, out trackInfo))
                    {
                        strA = trackInfo.label;
                    }
                    JukeboxV2.TrackInfo trackInfo2;
                    if (this._info.TryGetValue(y, out trackInfo2))
                    {
                        strB = trackInfo2.label;
                    }
                    return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
                }
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            else
            {
                if (!flag)
                {
                    return -1;
                }
                return 1;
            }
        }

        // Token: 0x06002560 RID: 9568 RVA: 0x000B3CCC File Offset: 0x000B1ECC
        private void PlayInternal(JukeboxInstance instance)
        {
            if (this._instance != instance)
            {
                this.ReleaseInstance();
                this._instance = instance;
                volume = this._instance.volume;
                repeat = this._instance.repeat;
                shuffle = this._instance.shuffle;
                this._instance.OnControl();
            }
            string file = this._instance.file;
            this._failed = 0;
            if (string.Equals(file, this._file, StringComparison.Ordinal))
            {
                position = 0U;
                paused = false;
                return;
            }
            this._release = true;
            this._file = file;
            this._paused = false;
        }

        // Token: 0x06002561 RID: 9569 RVA: 0x000B3D73 File Offset: 0x000B1F73
        private void StopInternal()
        {
            this._failed = 0;
            this.ReleaseInstance();
            this._release = true;
            this._file = null;
            this._paused = false;
        }

        // Token: 0x06002562 RID: 9570 RVA: 0x000B3D98 File Offset: 0x000B1F98
        public string GetNextInternal(string currentFile, bool forward, bool shuffle, bool allowReshuffle)
        {
            if (shuffle)
            {
                int count = this._shuffled.Count;
                if (count == 0)
                {
                    return null;
                }
                int num2;
                if (count >= 2)
                {
                    int num = this._shuffled.IndexOf(currentFile, StringComparer.OrdinalIgnoreCase);
                    if (num >= 0)
                    {
                        if (forward)
                        {
                            num2 = num + 1;
                            if (num2 >= count)
                            {
                                if (allowReshuffle)
                                {
                                    MathExtensions.ShuffleAll<string>(this._shuffled, currentFile);
                                    num2 = 1;
                                }
                                else
                                {
                                    num2 = 0;
                                }
                            }
                        }
                        else
                        {
                            num2 = num - 1;
                            if (num2 < 0)
                            {
                                num2 = 0;
                            }
                        }
                    }
                    else
                    {
                        num2 = 0;
                    }
                }
                else
                {
                    num2 = 0;
                }
                return this._shuffled[num2];
            }
            else
            {
                int count2 = this._playlist.Count;
                if (count2 == 0)
                {
                    return null;
                }
                int num4;
                if (count2 >= 2)
                {
                    int num3 = this._playlist.IndexOf(currentFile, StringComparer.OrdinalIgnoreCase);
                    if (num3 >= 0)
                    {
                        if (forward)
                        {
                            num4 = num3 + 1;
                            if (num4 >= count2)
                            {
                                num4 = 0;
                            }
                        }
                        else
                        {
                            num4 = num3 - 1;
                            if (num4 < 0)
                            {
                                num4 = count2 - 1;
                            }
                        }
                    }
                    else
                    {
                        num4 = 0;
                    }
                }
                else
                {
                    num4 = 0;
                }
                return this._playlist[num4];
            }
        }

        // Token: 0x06002563 RID: 9571 RVA: 0x000B3E7C File Offset: 0x000B207C
        public bool HasFileInternal(string file)
        {
            return this._playlist.Contains(file);
        }

        // Token: 0x06002564 RID: 9572 RVA: 0x000B3E8A File Offset: 0x000B208A
        private void SetInfo(string id, JukeboxV2.TrackInfo info)
        {
            this._info[id] = info;
            JukeboxInstance.NotifyInfo(id, info);
        }

        // Token: 0x06002565 RID: 9573 RVA: 0x000B3EA0 File Offset: 0x000B20A0
        public static JukeboxV2.TrackInfo GetInfo(string id)
        {
            JukeboxV2.TrackInfo result;
            if (!string.IsNullOrEmpty(id))
            {
                if (main._info.TryGetValue(id, out result))
                {
                    return result;
                }
                if (!IsEvent(id))
                {
                    main._pendingInfo.Remove(id);
                    main._pendingInfo.Add(id);
                }
            }
            result = DummyTrackInfo(id);
            return result;
        }

        // Token: 0x06002566 RID: 9574 RVA: 0x000B3EFC File Offset: 0x000B20FC
        private static JukeboxV2.TrackInfo DummyTrackInfo(string id)
        {
            return new JukeboxV2.TrackInfo
            {
                label = id,
                length = 0U
            };
        }

        // Token: 0x06002567 RID: 9575 RVA: 0x000B3F24 File Offset: 0x000B2124
        public static JukeboxV2.TrackInfo GetInfo(JukeboxV2.UnlockableTrack track)
        {
            string id;
            JukeboxV2.TrackInfo result;
            if (unlockableMusic.TryGetValue(track, out id))
            {
                result = GetInfo(id);
            }
            else
            {
                result = DummyTrackInfo(id);
            }
            return result;
        }

        // Token: 0x06002568 RID: 9576 RVA: 0x000B3F51 File Offset: 0x000B2151
        private void UpdateGameMusicMute()
        {
            //this.SetSnapshotState(this.snapshotOn, ref this.stateOn, isStartingOrPlaying && !paused && this._audible);
        }

        // Token: 0x06002569 RID: 9577 RVA: 0x000B3F7C File Offset: 0x000B217C
        private void SetSnapshotState(EventInstance snapshot, ref bool state, bool value)
        {
            if (state == value)
            {
                return;
            }
            state = value;
            if (snapshot.hasHandle())
            {
                if (state)
                {
                    snapshot.start();
                    return;
                }
                snapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        // Token: 0x0600256A RID: 9578 RVA: 0x000B3FA8 File Offset: 0x000B21A8
        private void SetParameters()
        {
            Player main = Player.main;
            JukeboxInstance instance = JukeboxV2.instance;
            if (instance == null)
            {
                return;
            }
            ISpeakerHost speakerHost = (main != null) ? Speaker.GetHost(main.currentSub as MonoBehaviour) : null;
            ISpeakerHost host = Speaker.GetHost(instance);
            bool flag = main != null && Ocean.main != null && Ocean.GetDepthOf(main.gameObject) > 0f;
            bool flag2 = Ocean.main != null && Ocean.GetDepthOf(instance.gameObject) > 0f;
            bool flag3 = Speaker.IsSameHost(speakerHost, host);
            bool flag4 = flag == flag2;
            Vector3 vector;
            float num;
            float num2;
            if (instance.GetSoundPosition(out vector, out num, out num2))
            {
                this.soundPosition = vector;
            }
            VECTOR position = this.soundPosition.ToFMODVector();
            if (this._channel.hasHandle())
            {
                ERRCHECK(this._channel.set3DAttributes(ref position, ref this.vectorVelocity));
            }
            else if (this._eventInstance.isValid())
            {
                this.attributes.position = position;
                ERRCHECK(this._eventInstance.set3DAttributes(this.attributes));
            }
            Vector3 position2 = ((Player.main != null) ? Player.main.transform : MainCamera.camera.transform).position;
            float sqrMagnitude = (this.soundPosition - position2).sqrMagnitude;
            this._audible = (sqrMagnitude <= maxDistance * maxDistance);
            bool value = !this._audible || (!flag3 && (speakerHost != null || !flag4));
            //this.SetSnapshotState(this.snapshotMute, ref this.stateMute, value);
            //bool value2 = flag3;
            //this.SetSnapshotState(this.snapshotReverb, ref this.stateReverb, value2);
            //bool value3 = !flag3;
            //this.SetSnapshotState(this.snapshotMuffle, ref this.stateMuffle, value3);
        }

        // Token: 0x0600256B RID: 9579 RVA: 0x000B4188 File Offset: 0x000B2388
        private void UpdateMetering()
        {
            this._fft.getMeteringInfo(this._metering, null);
            int numchannels = (int)this._metering.numchannels;
            this._db = 0f;
            this._dbLevels.Clear();
            if (numchannels > 0)
            {
                float num = 0f;
                for (int i = 0; i < numchannels; i++)
                {
                    float num2 = this._metering.rmslevel[i];
                    float item = FMODExtensions.LinearToDb(num2);
                    this._dbLevels.Add(item);
                    num += num2 * num2;
                }
                num = Mathf.Sqrt(num / (float)numchannels);
                this._db = FMODExtensions.LinearToDb(num);
            }
            this._peak = 0f;
            this._peakLevels.Clear();
            if (numchannels > 0)
            {
                for (int j = 0; j < numchannels; j++)
                {
                    float num3 = this._metering.peaklevel[j];
                    this._peakLevels.Add(num3);
                    this._peak += num3 * num3;
                }
                this._peak = Mathf.Sqrt(this._peak / (float)numchannels);
            }
        }

        // Token: 0x0600256C RID: 9580 RVA: 0x000B428C File Offset: 0x000B248C
        private void UpdateSpectrum()
        {
            IntPtr ptr;
            uint num;
            ERRCHECK(this._fft.getParameterData(2, out ptr, out num));
            Marshal.PtrToStructure<JukeboxV2.UWE_DSP_PARAMETER_FFT>(ptr, this._fftData);
            this._fftData.Update(ref this._spectrumData);
            int numchannels = this._fftData.numchannels;
            this._spectrum.Clear();
            if (numchannels > 0)
            {
                float num2 = (float)numchannels;
                int num3 = 63;
                if (num3 > this._fftData.length)
                {
                    num3 = this._fftData.length;
                }
                if (this._spectrum.Capacity < num3)
                {
                    this._spectrum.Capacity = num3;
                }
                for (int i = 0; i < num3; i++)
                {
                    float num4 = 0f;
                    for (int j = 0; j < numchannels; j++)
                    {
                        float num5 = this._spectrumData[j][i];
                        num4 += num5 * num5;
                    }
                    num4 = Mathf.Sqrt(num4 / num2);
                    this._spectrum.Add(num4);
                }
            }
        }

        // Token: 0x0600256D RID: 9581 RVA: 0x000B4381 File Offset: 0x000B2581
        private void ReleaseInstance()
        {
            if (this._instance != null)
            {
                this._instance.OnRelease();
                this._instance = null;
            }
        }

        // Token: 0x0600256E RID: 9582 RVA: 0x000B43A4 File Offset: 0x000B25A4
        private void Release()
        {
            if (this._eventInstance.isValid())
            {
                if (this._eventInstanceChannelGroup.hasHandle())
                {
                    ERRCHECK(this._eventInstanceChannelGroup.removeDSP(this._fft));
                }
                ERRCHECK(this._eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
                this._eventInstance.clearHandle();
                this._position = 0U;
            }
            this._eventInstanceChannelGroup.clearHandle();
            if (this._channel.hasHandle())
            {
                RESULT result = this._channel.removeDSP(this._fft);
                if (result != RESULT.ERR_INVALID_HANDLE && result != RESULT.ERR_CHANNEL_STOLEN)
                {
                    ERRCHECK(result);
                }
                result = this._channel.stop();
                if (result != RESULT.ERR_INVALID_HANDLE && result != RESULT.ERR_CHANNEL_STOLEN)
                {
                    ERRCHECK(result);
                }
                this._channel.clearHandle();
            }
            if (this._sound.hasHandle())
            {
                ERRCHECK(this._sound.release());
                this._sound.clearHandle();
                this._position = 0U;
            }
        }

        // Token: 0x0600256F RID: 9583 RVA: 0x000B4497 File Offset: 0x000B2697
        private static bool IsEvent(string id)
        {
            return id.StartsWith("event:", StringComparison.OrdinalIgnoreCase);
        }

        // Token: 0x06002570 RID: 9584 RVA: 0x000B44A5 File Offset: 0x000B26A5
        private static bool ERRCHECK(RESULT result)
        {
            return result.CheckResult();
        }

        // Token: 0x06002571 RID: 9585 RVA: 0x000B44B0 File Offset: 0x000B26B0
        public static bool ToSeconds(uint milliseconds, ref uint seconds)
        {
            uint num = milliseconds / 1000U;
            bool result = seconds != num;
            seconds = num;
            return result;
        }

        // Token: 0x06002572 RID: 9586 RVA: 0x000B44D0 File Offset: 0x000B26D0
        public static void ToTime(uint time, out uint hrs, out uint min, out uint sec)
        {
            hrs = time / 3600U;
            min = time / 60U % 60U;
            sec = time % 60U;
        }

        // Token: 0x06002573 RID: 9587 RVA: 0x000B44EC File Offset: 0x000B26EC
        public static string FormatTime(uint seconds)
        {
            uint hours;
            uint minutes;
            uint seconds2;
            ToTime(seconds, out hours, out minutes, out seconds2);
            return FormatTime(hours, minutes, seconds2);
        }

        // Token: 0x06002574 RID: 9588 RVA: 0x000B4510 File Offset: 0x000B2710
        public static string FormatTime(uint hours, uint minutes, uint seconds)
        {
            if (hours > 0U)
            {
                return string.Format("{0}:{1}:{2}", hours.ToString(), minutes.ToString("00"), seconds.ToString("00"));
            }
            if (minutes > 0U)
            {
                return string.Format("{0}:{1}", minutes.ToString("0"), seconds.ToString("00"));
            }
            return string.Format("0:{0}", seconds.ToString("00"));
        }

        // Token: 0x06002575 RID: 9589 RVA: 0x000B4588 File Offset: 0x000B2788
        public string CompileTimeCheck()
        {
            StringBuilder sb = new StringBuilder();
            Array values = Enum.GetValues(typeof(JukeboxV2.UnlockableTrack));
            for (int i = 0; i < values.Length; i++)
            {
                JukeboxV2.UnlockableTrack unlockableTrack = (JukeboxV2.UnlockableTrack)values.GetValue(i);
                string text;
                if (unlockableMusic.TryGetValue(unlockableTrack, out text))
                {
                    for (int j = 0; j < authoredMusic.Length; j++)
                    {
                        string b = authoredMusic[j];
                        if (string.Equals(text, b, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendFormat("UnlockableTrack.{0} unlocks '{1}' track which also exists in authoredMusic. Either assign different track or remove it from authoredMusic.\n", unlockableTrack, text);
                        }
                    }
                }
                else
                {
                    sb.AppendFormat("UnlockableTrack.{0} should be defined in unlockableMusic.\n", unlockableTrack);
                }
            }
            Action<string> action = delegate (string eventName)
            {
                if (!musicLabels.ContainsKey(eventName))
                {
                    sb.AppendFormat("'{0}' is missing from musicLabels dictionary. Add it here so Jukebox will show specified label instead of FMOD event path. \n", eventName);
                }
            };
            for (int k = 0; k < authoredMusic.Length; k++)
            {
                action(authoredMusic[k]);
            }
            using (Dictionary<JukeboxV2.UnlockableTrack, string>.Enumerator enumerator = unlockableMusic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Action<string> action2 = action;
                    KeyValuePair<JukeboxV2.UnlockableTrack, string> keyValuePair = enumerator.Current;
                    action2(keyValuePair.Value);
                }
            }
            if (sb.Length == 0)
            {
                return null;
            }
            return sb.ToString();
        }

        // Token: 0x06002576 RID: 9590 RVA: 0x000B46E4 File Offset: 0x000B28E4
        private void DrawDebug()
        {
            Dbg.Write("_bus: {0}", new object[]
            {
            this._bus.handle
            });
            Dbg.Write("_channelGroup: {0}", new object[]
            {
            this._channelGroup.handle
            });
            //Dbg.Write("\nsnapshotOn: {0} {1}", new object[]
            //{
            //this.snapshotOn.handle,
            //this.stateOn
            //});
            //Dbg.Write("snapshotMute: {0} {1}", new object[]
            //{
            //this.snapshotMute.handle,
            //this.stateMute
            //});
            //Dbg.Write("snapshotMuffle: {0} {1}", new object[]
            //{
            //this.snapshotMuffle.handle,
            //this.stateMuffle
            //});
            //Dbg.Write("snapshotReverb: {0} {1}", new object[]
            //{
            //this.snapshotReverb.handle,
            //this.stateReverb
            //});
            Dbg.Write("\n_fft: {0}", new object[]
            {
            this._fft.handle
            });
            Dbg.Write("\n_file: {0}", new object[]
            {
            (this._file != null) ? this._file : "null"
            });
            Dbg.Write("\n_sound: {0}", new object[]
            {
            this._sound.handle
            });
            Dbg.Write("_channel: {0}", new object[]
            {
            this._channel.handle
            });
            Dbg.Write("\n_eventInstance: {0}", new object[]
            {
            this._eventInstance.handle
            });
            Dbg.Write("_eventInstanceChannelGroup: {0}", new object[]
            {
            this._eventInstanceChannelGroup.handle
            });
            Dbg.Write("\n_infoSound: {0}", new object[]
            {
            this._infoSound.handle
            });
        }

        // Token: 0x04002721 RID: 10017
        private static readonly string[] authoredMusic = new string[]
        {

        };

        // Token: 0x04002722 RID: 10018
        private static readonly Dictionary<string, string> musicLabels = new Dictionary<string, string>
        {  
        };

        // Token: 0x04002723 RID: 10019
        private static readonly Dictionary<JukeboxV2.UnlockableTrack, string> unlockableMusic = new Dictionary<JukeboxV2.UnlockableTrack, string>
        {
        
        };

        // Token: 0x04002724 RID: 10020
        private static float minDistance = 1f;

        // Token: 0x04002725 RID: 10021
        private static float maxDistance = 20f;

        // Token: 0x04002726 RID: 10022
        private readonly HashSet<string> supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".ogg",
        ".wav",
        ".flac"
    };

        // Token: 0x04002727 RID: 10023
        private const MODE soundMode = MODE._3D | MODE.CREATESTREAM | MODE.ACCURATETIME | MODE.NONBLOCKING | MODE._3D_LINEARSQUAREROLLOFF;

        // Token: 0x04002728 RID: 10024
        private const int windowSize = 128;

        // Token: 0x04002729 RID: 10025
        private static JukeboxV2 _main;

        // Token: 0x0400272A RID: 10026
        private JukeboxV2.Repeat _repeat = Repeat.All;

        // Token: 0x0400272B RID: 10027
        private bool _shuffle;

        // Token: 0x0400272C RID: 10028
        private float _volume = 1f;

        // Token: 0x0400272D RID: 10029
        private float _attenuation = 1f;

        // Token: 0x0400272E RID: 10030
        private uint _position;

        // Token: 0x0400272F RID: 10031
        private bool _paused;

        // Token: 0x04002730 RID: 10032
        private bool _positionDirty;

        // Token: 0x04002731 RID: 10033
        private bool _pausedDirty;

        // Token: 0x04002732 RID: 10034
        private JukeboxInstance _instance;

        // Token: 0x04002733 RID: 10035
        private string _file;

        // Token: 0x04002734 RID: 10036
        private DSP _fft;

        // Token: 0x04002735 RID: 10037
        private uint _length;

        // Token: 0x04002736 RID: 10038
        private float _db;

        // Token: 0x04002737 RID: 10039
        private float _peak;

        // Token: 0x04002738 RID: 10040
        private List<float> _spectrum = new List<float>();

        // Token: 0x04002739 RID: 10041
        private List<float> _dbLevels = new List<float>();

        // Token: 0x0400273A RID: 10042
        private List<float> _peakLevels = new List<float>();

        // Token: 0x0400273B RID: 10043
        private List<string> _playlist = new List<string>();

        // Token: 0x0400273C RID: 10044
        private List<string> _shuffled = new List<string>();

        // Token: 0x0400273D RID: 10045
        private List<string> _pendingInfo = new List<string>();

        // Token: 0x0400273E RID: 10046
        private Dictionary<string, JukeboxV2.TrackInfo> _info = new Dictionary<string, JukeboxV2.TrackInfo>();

        // Token: 0x0400273F RID: 10047
        private string _infoFile;

        // Token: 0x04002740 RID: 10048
        private Sound _infoSound;

        // Token: 0x04002741 RID: 10049
        private bool _audible;

        // Token: 0x04002742 RID: 10050
        private UWE_DSP_METERING_INFO _metering = new UWE_DSP_METERING_INFO();

        // Token: 0x04002743 RID: 10051
        private JukeboxV2.UWE_DSP_PARAMETER_FFT _fftData = new JukeboxV2.UWE_DSP_PARAMETER_FFT();

        // Token: 0x04002744 RID: 10052
        private float[][] _spectrumData;

        // Token: 0x04002745 RID: 10053
        private bool _release;

        // Token: 0x04002746 RID: 10054
        private int _failed;

        // Token: 0x04002747 RID: 10055
        private Vector3 soundPosition;

        // Token: 0x04002748 RID: 10056
        private VECTOR vectorVelocity;

        // Token: 0x04002749 RID: 10057
        private ATTRIBUTES_3D attributes = new ATTRIBUTES_3D
        {
            position = default(VECTOR),
            velocity = default(VECTOR),
            forward = Vector3.forward.ToFMODVector(),
            up = Vector3.up.ToFMODVector()
        };

        // Token: 0x0400274A RID: 10058
        private float panScalar;

        // Token: 0x0400274B RID: 10059
        private float panVelocity;

        // Token: 0x0400274C RID: 10060
        private const float panTransitionDuration = 0.5f;

        // Token: 0x0400274D RID: 10061
        private bool stateOn;

        // Token: 0x0400274E RID: 10062
        private bool stateMute;

        // Token: 0x0400274F RID: 10063
        private bool stateMuffle;

        // Token: 0x04002750 RID: 10064
        private bool stateReverb;

        // Token: 0x04002751 RID: 10065
        //private EventInstance snapshotOn;

        // Token: 0x04002752 RID: 10066
       // private EventInstance snapshotMute;

        // Token: 0x04002753 RID: 10067
       // private EventInstance snapshotMuffle;

        // Token: 0x04002754 RID: 10068
        //private EventInstance snapshotReverb;

        // Token: 0x04002755 RID: 10069
        private string _fullMusicPath;

        // Token: 0x04002756 RID: 10070
        private Bus _bus;

        // Token: 0x04002757 RID: 10071
        private bool _busChannelGroupLocked;

        // Token: 0x04002758 RID: 10072
        private ChannelGroup _channelGroup;

        // Token: 0x04002759 RID: 10073
        private Sound _sound;

        // Token: 0x0400275A RID: 10074
        private Channel _channel;

        // Token: 0x0400275B RID: 10075
        private CREATESOUNDEXINFO _exinfo;

        // Token: 0x0400275C RID: 10076
        private EventInstance _eventInstance;

        // Token: 0x0400275D RID: 10077
        private ChannelGroup _eventInstanceChannelGroup;

        // Token: 0x02000BBC RID: 3004
        [StructLayout(LayoutKind.Sequential)]
        public class UWE_DSP_PARAMETER_FFT
        {
            // Token: 0x06006347 RID: 25415 RVA: 0x0020465C File Offset: 0x0020285C
            public void Update(ref float[][] spectrum)
            {
                if (spectrum == null || spectrum.Length < this.numchannels)
                {
                    spectrum = new float[this.numchannels][];
                }
                for (int i = 0; i < this.numchannels; i++)
                {
                    float[] array = spectrum[i];
                    if (array == null || array.Length < this.length)
                    {
                        array = new float[this.length];
                        spectrum[i] = array;
                    }
                    Marshal.Copy(this.spectrum_internal[i], array, 0, this.length);
                }
            }

            // Token: 0x040060D0 RID: 24784
            public int length;

            // Token: 0x040060D1 RID: 24785
            public int numchannels;

            // Token: 0x040060D2 RID: 24786
            [SuppressMessage("Gendarme.Rules.Maintainability", "AvoidAlwaysNullFieldRule")]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private IntPtr[] spectrum_internal;
        }

        // Token: 0x02000BBD RID: 3005
        public struct TrackInfo
        {
            // Token: 0x040060D3 RID: 24787
            public string label;

            // Token: 0x040060D4 RID: 24788
            public uint length;
        }

        // Token: 0x02000BBE RID: 3006
        public enum Repeat
        {
            // Token: 0x040060D6 RID: 24790
            Off,
            // Token: 0x040060D7 RID: 24791
            Track,
            // Token: 0x040060D8 RID: 24792
            All
        }

        // Token: 0x02000BBF RID: 3007
        public enum UnlockableTrack
        {
            // Token: 0x040060DA RID: 24794
            None,
            // Token: 0x040060DB RID: 24795
            Track1,
            // Token: 0x040060DC RID: 24796
            Track2,
            // Token: 0x040060DD RID: 24797
            Track3,
            // Token: 0x040060DE RID: 24798
            Track4,
            // Token: 0x040060DF RID: 24799
            Track5,
            // Token: 0x040060E0 RID: 24800
            Track6,
            // Token: 0x040060E1 RID: 24801
            Track7,
            // Token: 0x040060E2 RID: 24802
            Track8,
            // Token: 0x040060E3 RID: 24803
            Track9
        }
    }
}