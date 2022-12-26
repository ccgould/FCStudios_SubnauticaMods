using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace FCS_HomeSolutions.Mods.TV.Mono
{
    internal class VideoSectionController : MonoBehaviour
    {
        private VideoPlayer _video;
        private List<string> _videoLocations;
        private string _currentChannel;
        private TVController _controller;
        private float _timer;
        private AudioSource _audio;
        private AudioLowPassFilter _lowPassFilter;

        private GameObject _videoGrid;
        private GameObject _videoPlayer;
        private GameObject _noVideosFound;
        private RenderTexture _renderTexture;
        private Button _playBTN;
        private Button _pauseBTN;

        private void Update()
        {
            if (_controller == null || _lowPassFilter == null) return;


            if (_video != null)
                _video.playbackSpeed = Mathf.Approximately(DayNightCycle.main.deltaTime, 0f) ? 0 : 1;

            if (_controller.GetIsOn())
            {
                if (_lowPassFilter != null)
                {
                    _lowPassFilter.cutoffFrequency = Player.main.IsUnderwater() ||
                                                     Player.main.inSeamoth ||
                                                     Player.main.inExosuit ? 1566f : 22000f;
                }
            }

        }

        internal void Initialize(TVController controller)
        {
            LoadVideoLocations();

            _controller = controller;

            if (_audio == null)
            {
                _audio = controller.gameObject.GetComponentInChildren<AudioSource>();
            }



            _lowPassFilter = controller.gameObject.GetComponentInChildren<AudioLowPassFilter>();

            var rawImage = controller.gameObject.GetComponentInChildren<RawImage>(true);

            _video = controller.gameObject.GetComponentInChildren<VideoPlayer>(true);

            _videoGrid = GameObjectHelpers.FindGameObject(gameObject, "Content");

            var addBTN = GameObjectHelpers.FindGameObject(gameObject, "AddBTN").GetComponent<Button>();
            addBTN.onClick.AddListener((() =>
            {
                Application.OpenURL(Path.Combine(Mod.GetAssetPath(),"Videos"));
            }));

            var videoSelection = GameObjectHelpers.FindGameObject(controller.gameObject, "VideoSelection");

            _videoPlayer = GameObjectHelpers.FindGameObject(controller.gameObject, "VideoPlayer");
            _noVideosFound = GameObjectHelpers.FindGameObject(videoSelection, "Details");
            var previousBTN = GameObjectHelpers.FindGameObject(_videoPlayer, "PreviousBTN").GetComponent<Button>();
            previousBTN.onClick.AddListener(ChannelDown);
            
            _playBTN = GameObjectHelpers.FindGameObject(_videoPlayer, "PlayBTN").GetComponent<Button>();
            _playBTN.onClick.AddListener((() =>
            {
                Play();
            }));

            _pauseBTN = GameObjectHelpers.FindGameObject(_videoPlayer, "PauseBTN").GetComponent<Button>();
            _pauseBTN.onClick.AddListener((() =>
            {
                Pause();
            }));

            var stopBTN = GameObjectHelpers.FindGameObject(_videoPlayer, "StopBTN").GetComponent<Button>();
            stopBTN.onClick.AddListener((() =>
            {
                _videoPlayer.SetActive(false);
                Stop();
            }));
            var nextBTN = GameObjectHelpers.FindGameObject(_videoPlayer, "NextBTN").GetComponent<Button>();
            nextBTN.onClick.AddListener(ChannelUp);

            //var closeBTN = GameObjectHelpers.FindGameObject(_videoPlayer, "CloseBTN").GetComponent<Button>();
            //closeBTN.onClick.AddListener((() =>
            //{
            //    _videoPlayer.SetActive(false);
            //    Stop();
            //}));

            _renderTexture = new RenderTexture(1920, 1080, 24);
            _renderTexture.Create();

            _video.targetTexture = _renderTexture;
            rawImage.texture = _renderTexture;
        }

        private void LoadVideoLocations()
        {
            if (_videoLocations == null)
            {
                _videoLocations = new List<string>();
            }

            _videoLocations.Clear();

            var path = Path.Combine(Mod.GetAssetPath(), "Videos");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var videos = Directory.GetFiles(path, "*.mp4");

            foreach (string filePath in videos)
            {
                if (File.Exists(filePath) && !_videoLocations.Contains(filePath))
                {
                    _videoLocations.Add(filePath);
                }
            }

            QuickLogger.Debug($"Loaded: {_videoLocations.Count}.", true);
        }

        internal void Show()
        {
            _noVideosFound.SetActive(_videoLocations.Count < 0);
            gameObject.SetActive(true);
            _videoPlayer.SetActive(false);
            LoadVideoLocations();
            OnLoadDisplay();
        }

        private void OnLoadDisplay()
        {
            foreach (Transform video in _videoGrid.transform)
            {
                Destroy(video.gameObject);
            }

            foreach (string location in _videoLocations)
            {
                var videoBTN = GameObject.Instantiate(ModelPrefab.TVVideoBTNPrefab);
                var tvVideoButton = videoBTN.AddComponent<TvVideoButton>();
                tvVideoButton.Set(location,this);
                videoBTN.transform.SetParent(_videoGrid.transform,false);
            }
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
            Stop();
            _videoPlayer.SetActive(false);
        }

        private void ForceChannel()
        {
            if (string.IsNullOrWhiteSpace(_currentChannel) || _videoLocations.Count <= 0 || !_videoLocations.Contains(_currentChannel)) return;
            _video.url = _currentChannel;
            Play();
            _videoPlayer.SetActive(true);
        }
        
        internal void ChannelUp()
        {
            _currentChannel = _videoLocations.SkipWhile(x => x != _currentChannel).Skip(1).DefaultIfEmpty(_videoLocations[0]).FirstOrDefault();
            ForceChannel();
        }

        internal void ChannelDown()
        {
            _currentChannel = _videoLocations.TakeWhile(x => x != _currentChannel).DefaultIfEmpty(_videoLocations[_videoLocations.Count - 1]).LastOrDefault();
            ForceChannel();
        }

        internal void Play()
        {
            _video.Play();
            _pauseBTN.gameObject.SetActive(true);
            _playBTN.gameObject.SetActive(false);
        }

        internal void Pause()
        {
            _video.Pause();
            _pauseBTN.gameObject.SetActive(false);
            _playBTN.gameObject.SetActive(true);
        }

        internal void Stop()
        {
            _video.clip = null;
            _video.Stop();
            ClearOutRenderTexture();
            _currentChannel = string.Empty;
        }


        public void ClearOutRenderTexture()
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }


        internal void VolumeUp()
        {
            if (!_controller.GetIsOn()) return;
            _timer = 0f;
            ChangeVolume(Mathf.Clamp(_audio.volume + 0.1f, 0, 1f));
        }

        internal void VolumeDown()
        {
            if (!_controller.GetIsOn()) return;
            _timer = 0f;
            ChangeVolume(Mathf.Clamp(_audio.volume - 0.1f, 0, 1f), false);
        }

        internal void ChangeVolume(float volume, bool isVolumeUp = true)
        {
            _audio.volume = volume;
            var amount = Mathf.RoundToInt(volume * 100).ToString();
            if (isVolumeUp)
            {
                _controller.SetVolumeText(amount);
            }
            else
            {
                _controller.SetVolumeText(volume <= 0f ? "MUTE" : amount);
            }
        }

        internal float GetVolume()
        {
            return _audio.volume;
        }

        internal void Mute(bool value)
        {
            _audio.mute = value;
        }

        public string GetCurrentChannel()
        {
            return _currentChannel;
        }

        public void LoadChannel(string channelPath)
        {
            if (_videoLocations.Contains(channelPath))
            {
                _currentChannel = channelPath;
                ForceChannel();
            }
        }

        public bool IsMuted()
        {
            return Mathf.Approximately(_audio.volume, 0) || _audio.mute;
        }
    }

    internal class TvVideoButton : MonoBehaviour, IPointerHoverHandler
    {
        private string _path;
        private string _fullName;

        internal void Set(string path,VideoSectionController controller)
        {
            _path = path;
            _fullName = Path.GetFileName(path);
            gameObject.GetComponentInChildren<Text>().text = _fullName.TruncateWEllipsis(9);
            var button = GetComponent<Button>();
            button.onClick.AddListener((() =>
            {
                controller.LoadChannel(path);
            }));
        }

        public void OnPointerHover(PointerEventData eventData)
        {
            HandReticle main = HandReticle.main;

            main.SetTextRaw(HandReticle.TextType.Use, _fullName);
            main.SetTextRaw(HandReticle.TextType.UseSubscript, "Click to play");
        }
    }
}