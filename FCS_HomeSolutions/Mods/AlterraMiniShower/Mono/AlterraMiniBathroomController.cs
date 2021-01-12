using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using rail;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.AlterraMiniShower.Mono
{
    internal class AlterraMiniBathroomController : FcsDevice , IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private AlterraMiniBathroomDataEntry _saveData;
        private DoorController _showerDoor;
        private DoorController _toiletDoor;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "AMB", Mod.ModName);
        }
        
        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.ChangeColor(_saveData.Fcs.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetAlterraMiniBathroomSaveData(id);
        }
        
        public override void Initialize()
        {
            GameObjectHelpers.FindGameObject(gameObject, "ShowerControls").EnsureComponent<ShowerController>();
            _showerDoor = GameObjectHelpers.FindGameObject(gameObject, "ShowerDoor").EnsureComponent<DoorController>();
            _showerDoor.ClosePos = -0.01301698f;
            _showerDoor.OpenPos = 1.2f;
            
            _toiletDoor = GameObjectHelpers.FindGameObject(gameObject, "ToiletDoor").EnsureComponent<DoorController>();
            _toiletDoor.ClosePos = 0.003515291f;
            _toiletDoor.OpenPos = -1.2f;

            var toiletLightController = GameObjectHelpers.FindGameObject(gameObject, "ToiletTrigger").EnsureComponent<LightController>();
            toiletLightController.TargetLight = GameObjectHelpers.FindGameObject(gameObject, "ToiletLight").GetComponent<Light>();
            
            var showerLightController = GameObjectHelpers.FindGameObject(gameObject, "ShowerTrigger").EnsureComponent<LightController>();
            showerLightController.TargetLight = GameObjectHelpers.FindGameObject(gameObject, "ShowerLight").GetComponent<Light>();
            
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject,AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseEmissiveDecalsController, gameObject, new Color(0, 1, 1, 1));
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, gameObject, new Color(0.8f, 0.4933333f, 0f));
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseEmissiveDecalsController, gameObject,  2.5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.AlienChefFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.AlienChefFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new AlterraMiniBathroomDataEntry();
            }
            _saveData.Id = id;
            _saveData.Fcs = _colorManager.GetColor().ColorToVector4();
            _saveData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();

            newSaveData.AlterraMiniBathroomEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override bool CanDeconstruct(out string reason)
        {
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
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }
    }

    internal class ShowerController : HandTarget, IHandTarget
    {
        private bool _isOn;
        private ParticleSystem _shower;

        private void Start()
        {
            _shower = gameObject.transform.parent.GetComponentInChildren<ParticleSystem>();
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(_isOn ? "Turn Off Shower" : "Turn On Shower");
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if(_isOn)
            {
                _shower.Stop();
            }
            else
            {
                _shower.Play();
            }

            _isOn ^= true;
        }
    }

    internal class LightController : MonoBehaviour
    {
        public Light TargetLight { get; set; }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            if (TargetLight != null )
            {
                TargetLight.enabled = true;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            if (TargetLight != null)
            {
                TargetLight.enabled = false;
            }
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            if (TargetLight != null)
            {
                TargetLight.enabled = true;
            }
        }
    }

    internal class DoorController : HandTarget, IHandTarget
    {
        private Transform _transform;
        public float ClosePos { get; set; }
        public float OpenPos { get; set; }
        private float _targetPos;
        public bool IsOpen => Mathf.Approximately(_targetPos, OpenPos);
        private const float  Speed = 2.5f;


        private void Update()
        {
            MoveDoor();
        }

        public override void Awake()
        {
            base.Awake();
            _targetPos = ClosePos;
            _transform = gameObject.transform;
        }

        public void ForceOpen()
        {
            OpenDoor();
        }

        private void OpenDoor()
        {
            _targetPos = OpenPos;
        }

        private void CloseDoor()
        {
            _targetPos = ClosePos;
        }

        private void MoveDoor()
        {
            // remember, 10 - 5 is 5, so target - position is always your direction.
            Vector3 dir = new Vector3(_targetPos, _transform.localPosition.y, _transform.localPosition.z) - _transform.localPosition;

            // magnitude is the total length of a vector.
            // getting the magnitude of the direction gives us the amount left to move
            float dist = dir.magnitude;

            // this makes the length of dir 1 so that you can multiply by it.
            dir = dir.normalized;

            // the amount we can move this frame
            float move = Speed * DayNightCycle.main.deltaTime;

            // limit our move to what we can travel.
            if (move > dist) move = dist;

            // apply the movement to the object.
            _transform.Translate(dir * move);
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            if (IsOpen)
            {
                main.SetInteractText("Close Door");
            }
            else
            {
                main.SetInteractText("Open Door");
            }

            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (IsOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
    }
}
