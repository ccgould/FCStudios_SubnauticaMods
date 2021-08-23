using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_HomeSolutions.Mods.PeeperLoungeBar.Mono
{
    internal class PeeperLoungeBarController : FcsDevice,IHandTarget
    {
        private bool _runStartUpOnEnable;
        private Transform _rot;
        private Transform _playerTrans;
        private float _currentYaw;
        private Button _prevBTN;
        private Button _nextBTN;
        private int _index;
        private uGUI_Icon _icon;
        private Text _cost;
        private Text _name;
        private KeyValuePair<TechType, decimal> _food;
        private StorageContainer _sc;
        public float minInterval = 60;
        private float timeNextPlay = -1f;
        private bool _introHasBeenPlayed;
        private bool _hasPlayerLeft;
        private Random _random;
        private readonly List<string> _welcomeChatMessages = new()
        {
            "PLB_Hello",
            "PLB_AnnoyingFish"
        };

        private InterfaceInteraction _interactionHelper;


        public Channel AudioTrack { get; set; }
        private float _speed => QPatch.Configuration.PeeperLoungeBarTurnSpeed;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "PLB", Mod.ModPackID);
        }

        public override float GetPowerUsage()
        {
            return 0.01f;
        }

        private SoundEntry FindAudioClip(string trackName)
        {
            if(!QPatch.AudioClips.ContainsKey(trackName)) return  new SoundEntry();
            return QPatch.AudioClips[trackName];
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_sc != null && _sc.container.count > 0)
            {
                reason = AlterraHub.NotEmpty();
                return false;
            }
            reason= string.Empty;
            return true;
        }

        public override void Initialize()
        {
            if(IsInitialized) return;
            _playerTrans = Player.main.transform;
            _rot = GameObjectHelpers.FindGameObject(gameObject, "anim_controller").transform;
            _nextBTN = GameObjectHelpers.FindGameObject(gameObject, "nextBTN").GetComponent<Button>();
            _sc = gameObject.GetComponentInChildren<StorageContainer>();
            _sc.enabled = false;
            _sc.container.onRemoveItem += item =>
            {
                if (_sc.container.count <= 0 && GetCanPlay())
                {
                    PlayAudioTrack("PLB_FishRemoved");
                }
            };
            _random = new Random();

            CreateAquariumBTN();

            _nextBTN.onClick.AddListener(() =>
            {
                _index++;
                if (_index >= Mod.PeeperBarFoods.Count)
                {
                    _index = _index % Mod.PeeperBarFoods.Count;
                }

                UpdateSelection();
            });

            _prevBTN = GameObjectHelpers.FindGameObject(gameObject, "prevBTN").GetComponent<Button>();
            _prevBTN.onClick.AddListener(() =>
            {
                _index--;
                if (_index < 0)
                {
                    _index = Mod.PeeperBarFoods.Count - 1;
                }

                UpdateSelection();
            });            
            
            var _purchaseBTN = GameObjectHelpers.FindGameObject(gameObject, "PurchaseBTN").GetComponent<Button>();
            _purchaseBTN.onClick.AddListener(() =>
            {

                if (!PlayerInteractionHelper.CanPlayerHold(_food.Key))
                {
                    QuickLogger.ModMessage(AlterraHub.InventoryFull());
                    return;
                }

                if (!PlayerInteractionHelper.HasItem(FCSAlterraHubService.PublicAPI.AccountSystem.CardTechType) && GetCanPlay())
                {
                    //QuickLogger.ModMessage(AlterraHub.CardNotDetected());
                    PlayAudioTrack("PLB_NoCardDetected");
                    return;
                }

                if (!FCSAlterraHubService.PublicAPI.AccountSystem.HasEnough(_food.Value) && GetCanPlay())
                {
                    //QuickLogger.ModMessage("Not enough credit");
                    PlayAudioTrack("PLB_NotEnoughCredit");
                    return;
                }

                if(GetCanPlay()) PlayAudioTrack("PLB_ThankYou");
                FCSAlterraHubService.PublicAPI.AccountSystem.RemoveFinances(_food.Value);
                PlayerInteractionHelper.GivePlayerItem(_food.Key);
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

            });

            _icon = GameObjectHelpers.FindGameObject(gameObject, "icon").AddComponent<uGUI_Icon>();
            _cost = GameObjectHelpers.FindGameObject(gameObject, "Cost").GetComponent<Text>();
            _name = GameObjectHelpers.FindGameObject(gameObject, "Text").GetComponent<Text>();

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial, gameObject, 4f);

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            UpdateSelection();
            IsInitialized = true;
        }

        private void CreateAquariumBTN()
        {
            var aquariumBTN = GameObjectHelpers.FindGameObject(gameObject, "AquariumBTN").GetComponent<Button>();
            var toolTip = aquariumBTN.gameObject.AddComponent<FCSToolTip>();
            toolTip.RequestPermission += () => WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 4f);
            toolTip.Tooltip = Language.main.Get("UseAquarium");
            aquariumBTN.onClick.AddListener((() => { _sc.Open(transform); }));
        }

        private void UpdateSelection()
        {
            _food = Mod.PeeperBarFoods.ElementAt(_index);
            _icon.sprite = SpriteManager.Get(_food.Key);
            _cost.text = _food.Value.ToString("c");
            _name.text = Language.main.Get(_food.Key);
        }

        public bool GetCanPlay()
        {
            if (!QPatch.Configuration.PeeperLoungeBarPlayVoice) return false;
            return DayNightCycle.main.timePassedAsFloat >= this.timeNextPlay;
        }

        private void Update()
        {
            if (!IsConstructed || !IsInitialized) return;

            
            if (CheckIfPlayingTrack())
            {
                AudioTrack.setPaused(Time.timeScale <= 0f);
            }

            if (WorldHelpers.CheckIfInRange(Player.main.gameObject, gameObject, 5) && !uGUI.isLoading)
            {
                _sc.enabled = false;
                Vector3 eulerAngles = Quaternion.LookRotation(transform.InverseTransformDirection(Vector3.Normalize(_playerTrans.position - transform.position))).eulerAngles;
                var targetYaw = eulerAngles.y;
                _currentYaw = Mathf.LerpAngle(_currentYaw, targetYaw, Time.deltaTime * _speed);
                _rot.localEulerAngles = new Vector3(0f, _currentYaw, 0f);

                if (!_hasPlayerLeft) return;
                _hasPlayerLeft = false;
                Play();
            }
            else
            {
                _hasPlayerLeft = true;
            }
        }

        private bool CheckIfPlayingTrack()
        {
            AudioTrack.isPlaying(out bool isPlaying);
            return isPlaying;
        }

        private void Play()
        {
            float timePassedAsFloat = DayNightCycle.main.timePassedAsFloat;
            if (timePassedAsFloat < timeNextPlay)
            {
                return;
            }
            
            if (!CheckIfPlayingTrack() && GetCanPlay())
            {
                if (_sc.container.count > 0)
                {
                    var index = _random.Next(_welcomeChatMessages.Count);
                    PlayAudioTrack(_welcomeChatMessages[index]);
                }
                else
                {
                    PlayAudioTrack(_welcomeChatMessages[0]);
                }
            }

            this.timeNextPlay = timePassedAsFloat + this.minInterval;
        }
        
        public void PlayAudioTrack(string trackName)
        {
            if (string.IsNullOrEmpty(trackName))
            {
                QuickLogger.Debug("Track returned null", true);
                return;
            }

            var pos = new VECTOR
            {
                x = transform.position.x,
                y = transform.position.y,
                z = transform.position.z
            };

            var zero = new VECTOR();
            AudioTrack.set3DAttributes(ref pos, ref zero, ref zero);
            
            AudioTrack.isPlaying(out var isPlaying);

            if (!isPlaying)
            {
                var clip = FindAudioClip(trackName);
                Subtitles.main.Add(clip.Message, null);
                AudioTrack = AudioUtils.PlaySound(clip.Sound, SoundChannel.Voice);
                
            }
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;

                    if (!_introHasBeenPlayed && !uGUI.isLoading)
                    {
                        if(!FCSAlterraHubService.PublicAPI.GetGamePlaySettings().ConditionMet("PLBIntroPlayed"))
                        {
                            PlayAudioTrack("PLB_Intro");
                            _introHasBeenPlayed = true;
                            FCSAlterraHubService.PublicAPI.GetGamePlaySettings().SetCondition("PLBIntroPlayed");
                        }
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }

            //Doing this check because the storage container need to be disabled and its not during creation
            if (_sc != null && _sc.enabled)
            {
                _sc.enabled = false;
            }
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _interactionHelper.IsInRange) return;
            base.OnHandHover(hand);

            var data = new[]
            {
                $"{AlterraHub.PowerPerMinute(GetPowerUsage() * 60)}"
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}