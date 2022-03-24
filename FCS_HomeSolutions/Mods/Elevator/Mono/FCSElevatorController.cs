using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Elevator.Buildable;
using FCS_HomeSolutions.Patches;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Elevator.Mono
{
    internal class FCSElevatorController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private Dictionary<string, ElevatorFloorData> _floorData = new();
        private Transform _trans;
        private ElevatorFloorData _currentFloor;
        private Transform _platFormTrans;
        private bool _isRailingsVisible = true;
        private float _speed = 1f;
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private ElevatorDataEntry _savedData;
        private GameObject _startPos;
        private GameObject _floorsContainer;
        private readonly List<GameObject> _railing = new();
        private FCSMessageBox _messageBox;
        private bool _isMoving;
        private GameObject _screensGroup;
        private PlatformPoleController _poleController;
        private AudioLowPassFilter _lowPassFilter;
        private AudioSource _audio;
        private Transform _startPosTrans;
        internal const float POWERUSAGE = .5f;
        internal PlatformTrigger PlatformTrigger { get; private set; }
        private const float LerpTime = .1f;
        public override bool IsOperational => IsConstructed && IsInitialized && Manager != null && PlatformTrigger != null;

        public override float GetPowerUsage()
        {
            return IsMoving() ? POWERUSAGE : 0f;
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, ElevatorBuildable.ElevatorTabID, Mod.ModPackID);
            if (Manager == null) _screensGroup.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (_platFormTrans == null || _currentFloor == null || Manager == null)
            {
                _isMoving = false;
                return;
            }

            UpdateIsMoving();

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_currentFloor?.LevelObject != null && Manager.HasEnoughPower(POWERUSAGE))
            {
                var pos = _platFormTrans.position;
                var target = new Vector3(_currentFloor.LevelObject.transform.position.x,
                    _currentFloor.LevelObject.transform.position.y - 0.25f,
                    _currentFloor.LevelObject.transform.position.z);
                _platFormTrans.position = Vector3.MoveTowards(pos, Vector3.Lerp(pos, target, LerpTime), _speed * Time.deltaTime);

                if (IsMoving())
                {
                    Physics.SyncTransforms();
                }
            }
        }
        
        private bool MoveTowards(Transform t, Vector3 target, float speed)
        {
            if (Vector3.Distance(t.position, target) < speed)
            {
                t.position = target;
                return true;
            }
            else
            {
                t.position = Vector3.MoveTowards(t.position, target, speed);
                return false;
            }
        }

        private void UpdateIsMoving()
        {
            var target = new Vector3(_currentFloor.LevelObject.transform.position.x,
                _currentFloor.LevelObject.transform.position.y - 0.25f,
                _currentFloor.LevelObject.transform.position.z);

            if (Vector3.Distance(_platFormTrans.position, target) < _speed * Time.deltaTime)
            {
                _isMoving = false;
            }
            else
            {
                _isMoving = true;
            }
        }

        public override void Initialize()
        {
            _trans = gameObject.transform;
            var platForm = GameObjectHelpers.FindGameObject(gameObject, "Platform");
            PlatformTrigger = platForm.AddComponent<PlatformTrigger>();
            PlatformTrigger.Initialize(this);
            _platFormTrans = platForm.transform;
            
            _poleController = GameObjectHelpers.FindGameObject(gameObject, "pole")
                .AddComponent<PlatformPoleController>();
            _poleController.Initialize(this);

            _startPos = GameObjectHelpers.FindGameObject(gameObject, "StartPos");
            _startPosTrans = _startPos.transform;
            _floorsContainer = GameObjectHelpers.FindGameObject(gameObject, "Floors");
            
            var railings = GameObjectHelpers.FindGameObjects(gameObject, "railing_", SearchOption.StartsWith);

            foreach (GameObject railing in railings)
            {
                _railing.Add(railing);
            }

            _screensGroup = GameObjectHelpers.FindGameObject(gameObject, "Screens");
            _screensGroup.SetActive(true);


            foreach (Transform screen in _screensGroup.transform)
            {
                var screenController = screen.gameObject.AddComponent<ScreenController>();
                screenController.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            _colorManager.ChangeColor(new ColorTemplate{SecondaryColor = new Color(0.8666667f, 0.6588235f, 0.01960784f) });

            AddNewFloor("base", "Home", _startPos.transform.localPosition.y, 0);
            _currentFloor = GetFloorData("base");

            AlterraHub.ReplaceShadersV2(platForm);

            if (_audio == null)
            {
                _audio = gameObject.GetComponentInChildren<AudioSource>();
                _lowPassFilter = gameObject.GetComponentInChildren<AudioLowPassFilter>();
            }

            GameObjectHelpers.FindGameObject(gameObject, "TriggerBox").AddComponent<FloorTriggerBox>();
            GameObjectHelpers.FindGameObject(gameObject, "TriggerBox_1").AddComponent<FloorTriggerBox>();

            InvokeRepeating(nameof(UpdateAudioCheck), 1f, 1f);


            IsInitialized = true;
        }

        private void UpdateAudioCheck()
        {
            if (!IsOperational || _audio == null) return;

                _lowPassFilter.cutoffFrequency = Player.main.IsUnderwater() ||
                                                 Player.main.IsInBase() ||
                                                 Player.main.IsInSub() ||
                                                 Player.main.inSeamoth ||
                                                 Player.main.inExosuit
                    ? 1566f
                    : 22000f;


            if (_audio.isPlaying && WorldHelpers.CheckIfPaused() || !Manager.HasEnoughPower(POWERUSAGE))
            {
                _audio.Pause();
                return;
            }

            if (!_audio.isPlaying && IsMoving())
            {
                _audio.Play();
            }

            if (_audio.isPlaying && !IsMoving())
            {
                _audio.Stop();
            }
        }

    private ElevatorFloorData GetFloorData(string id)
        {
            return _floorData.ContainsKey(id) ? _floorData[id] : null;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);

                    //Set the platform position
                    _platFormTrans.localPosition = new Vector3(_platFormTrans.localPosition.x,_savedData.PlateformPosition, _platFormTrans.localPosition.z);

                    if (_savedData.FloorData != null)
                    {
                        foreach (KeyValuePair<string, ElevatorFloorData> floorData in _savedData.FloorData)
                        {
                            if (floorData.Key.Equals("base"))
                            {
                                var firstFloor = _floorData.ElementAt(0);
                                if (firstFloor.Value != null)
                                {
                                    firstFloor.Value.FloorName = floorData.Value.FloorName;
                                }

                            }
                            AddNewFloor(floorData.Value.FloorId,floorData.Value.FloorName,floorData.Value.Meters,floorData.Value.FloorIndex);
                        }

                        _poleController.UpdatePole();

                        foreach (var floorData in _floorData)
                        {
                            if (floorData.Value.FloorId.Equals(_savedData.CurrentFloorId))
                            {
                                _currentFloor = floorData.Value;
                                break;
                            }
                        }
                    }

                    ChangeRailingVisibility(_savedData.IsRailingsVisible);

                    _speed = _savedData.Speed;
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetElevatorDataEntrySaveData(GetPrefabID());
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (PlatformTrigger != null && PlatformTrigger.IsPlayerInside)
            {
                reason = "Cannot deconstruct while player in on elevator";
                return false;
            }
            reason = string.Empty;
            return true;
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
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new ElevatorDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.FloorData = _floorData;
            _savedData.CurrentFloorId = _currentFloor?.FloorId;
            _savedData.PlateformPosition = _platFormTrans.localPosition.y;
            _savedData.IsRailingsVisible = _isRailingsVisible;
            _savedData.Speed = _speed;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.ElevatorDataEntries.Add(_savedData);
        }

        public Dictionary<string,ElevatorFloorData> GetStoredFloorData()
        {
            return _floorData;
        }

        public void AddNewFloor(string floorId,string floorName, float meters,int floorIndex = -1)
        {
            if(_floorData.ContainsKey(floorId)) return;

            if (!floorId.Equals("base"))
            {
                var floorPrefab = GameObject.Instantiate(ModelPrefab.PlatformFloorFrame);

                _floorData.Add(floorId, new ElevatorFloorData
                {
                    Controller = this,
                    FloorName = floorName,
                    Meters = meters,
                    LevelObject = floorPrefab,
                    FloorId = floorId,
                    FloorIndex = floorIndex
                });


                //Set new floorPrefab

                var floorTrans = floorPrefab.transform;
                floorTrans.SetParent(_floorsContainer.transform);
                floorTrans.localPosition = new Vector3(0, meters, 0);
                floorTrans.rotation = _platFormTrans.rotation;
                GameObjectHelpers.FindGameObject(floorPrefab, "TriggerBox").AddComponent<FloorTriggerBox>();
                GameObjectHelpers.FindGameObject(floorPrefab, "TriggerBox_1").AddComponent<FloorTriggerBox>();

                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, floorPrefab, Color.cyan);
            }
            else
            {
                _floorData.Add(floorId, new ElevatorFloorData
                {
                    Controller = this,
                    FloorName = floorName,
                    Meters = meters,
                    LevelObject = _startPos,
                    FloorId = floorId
                });
            }
        }

        internal float GetSpeedValue()
        {
            return _speed;
        }
        
        /// <summary>
        /// Find the highest floor level in the collection and returns it
        /// </summary>
        /// <returns></returns>
        public float GetHighestFloorLevel()
        {
            if (_floorData == null || !_floorData.Any()) return 0f;

            return _floorData.Max(x => x.Value.Meters);
        }

        /// <summary>
        /// Find the highest floor level in the collection and adds 8 meters
        /// </summary>
        /// <returns></returns>
        public float GetNewFloorLevel()
        {
            if (_floorData == null || !_floorData.Any())
            {
                return 0f;
            }

            return GetHighestFloorLevel() + 10f;
        }

        public bool DeleteLevel(string floorId)
        {
            if (!_floorData.Any() || floorId.Equals("base")) return false;

            //if (!_floorData.Last().Key.Equals(floorId)) return false;
            
            if (!_floorData.ContainsKey(floorId)) return false;

            Destroy(_floorData[floorId].LevelObject);
            _floorData.Remove(floorId);
            return true;
        }

        public void GoToFloor(string floorId)
        {
            if(string.IsNullOrWhiteSpace(floorId)) return;
            if (_floorData.ContainsKey(floorId))
            {
                _currentFloor = _floorData[floorId];
            }
        }
        
        public void UpdateFloor(string floorId, string floorName, float floorMeters)
        {
            if (_floorData.ContainsKey(floorId))
            {
                var data =_floorData[floorId];
                data.FloorName = floorName;
                data.Meters = floorId.Equals("base") ? data.Meters : floorMeters;
                var lvlObject =  data.LevelObject.transform;
                lvlObject.localPosition = new Vector3(0, floorMeters, 0);
            }
        }

        public void ChangeRailingVisibility(bool value)
        {
            _isRailingsVisible = value;

            for (int i = _railing.Count - 1; i >= 0; i--)
            {
                if (_railing[i] == null)
                {
                    _railing.RemoveAt(i); continue;
                }

                _railing[i].SetActive(value);
            }
        }

        public void ChangeSpeed(float value)
        {
            _speed = value;
        }

        public bool IsRailingsVisible()
        {
            return _isRailingsVisible;
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public Transform GetHighestFloor()
        {
            //if (_floorData == null || !_floorData.Any()) return null;

            ////var max = _floorData.Max(x => x.Value.Meters);

            ////var level = _floorData.SingleOrDefault(x => Mathf.Approximately(x.Value.Meters,max));

            //var level = _floorData.OrderBy(x => x.Value.Meters).LastOrDefault();

            //return level.Value?.LevelObject.transform;

            if (_floorData == null || !_floorData.Any()) return null;

            var highestFloor = _floorData.Values.OrderByDescending(p => p.Meters).FirstOrDefault();
            QuickLogger.Debug($"Highest Floor: {highestFloor?.FloorName} : {highestFloor?.FloorIndex} : {highestFloor?.Meters}M",true);
            return highestFloor?.LevelObject.transform;
        }

        public override void OnHandHover(GUIHand hand)
        {
            base.OnHandHover(hand);

            if (!IsInitialized || !IsConstructed || PlatformTrigger == null || PlatformTrigger.IsPlayerInside)
            {
                //HandReticle main = HandReticle.main;
                //main.SetIcon(HandReticle.IconType.Default);
                return;
            }


            if (Manager == null)
            {
                var error = new[]
                {
                   "Must be built on a platform/base."
                };
                error.HandHoverPDAHelperEx(GetTechType(), HandReticle.IconType.HandDeny);
                return;
            }

            if (!Manager.HasEnoughPower(POWERUSAGE))
            {
                var error = new[]
                {
                    Language.main.Get("NoPower")
                };
                error.HandHoverPDAHelperEx(GetTechType(), HandReticle.IconType.HandDeny);
                return;
            }


            var key = GameInput.GetBindingName(GameInput.Button.Reload, GameInput.BindingSet.Primary);

            //var message = hand.IsTool()
            //    ? "Please clear hand to call elevator"
            //    : $"Press ({key}) to call elevator back to home.";

            var data = new[]
            {
                /*message,*/
                AlterraHub.PowerPerMinute(GetPowerUsage())
            };
            data.HandHoverPDAHelperEx(GetTechType(),HandReticle.IconType.Info);
            
            //if (GameInput.GetButtonDown(GameInput.Button.Reload))
            //{
            //    GoToFloor("base");
            //}

        }

        public void OnHandClick(GUIHand hand)
        {
            //Not in use
        }

        public bool IsMoving()
        {
            return _isMoving;
        }

        public ElevatorFloorData GetCurrentFloorData()
        {
            return _currentFloor;
        }

        public int GetCurrentFloorIndex()
        {
            return _currentFloor.FloorIndex;
        }

        public int GetPlatformCurrentHeightRounded()
        {
            return Mathf.FloorToInt(_platFormTrans.localPosition.y);
        }

        private float RoundToOneDec(float unrounded)
        {
            return Mathf.Round(unrounded * 1000) / 1000;
        }

        public int GetFloorCount()
        {
            return _floorData?.Count ?? ElevatorHUD.Main.MAX_FLOOR_LEVELS;
        }

        public void GoToFloor(GameObject floorObject)
        {
            GoToFloor(_floorData.FirstOrDefault(x=>x.Value.LevelObject == floorObject).Value?.FloorId ?? "base");
        }
    }

    internal class FloorTriggerBox : MonoBehaviour
    {
        private FCSElevatorController _controller;

        private void Start()
        {
            _controller = GetComponentInParent<FCSElevatorController>();


        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19 || _controller == null) return;
            _controller.GoToFloor(gameObject.transform.parent.gameObject);
        }
    }
}
