using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCS_AIMarineTurbine.Mono
{
    internal class AIWindSuferController : MonoBehaviour, IProtoEventListener, IConstructable
    {
        private Rigidbody _rigidBody;
        private WorldForces _wordForces;
        private bool _hasBeenUnderWater;
        private bool _frozen;
        private bool _hasReachedDepth;
        private bool _begingLerp;
        private Constructable _constructable;
        private bool _init;
        private float _timer = 5.0f;

        private void Awake()
        {
            _rigidBody = GetComponentInParent<Rigidbody>();

            if (_rigidBody == null)
            {
                QuickLogger.Error("The component rigidBody was not found on the object");
            }

            _wordForces = GetComponentInParent<WorldForces>();

            if (_wordForces == null)
            {
                QuickLogger.Error("The component WorldForces was not found on the object");
            }

            InvokeRepeating("PrintLog", 1, 1);
            _init = true;
        }

        private void Start()
        {
            if (!FindAllComponents())
            {
                QuickLogger.Error("Wasnt able to find all components!");
                gameObject.transform.r
            }
        }

        private bool FindAllComponents()
        {
            var model = gameObject.FindChild("model")?.gameObject;

            if (model == null)
            {
                QuickLogger.Error("Cannot find the model gameobject");
                return false;
            }

            var target = model.FindChild("Target")?.gameObject;

            if (target == null)
            {
                QuickLogger.Error("Cannot find the target gameobject");
                return false;
            }

            var dest = target.FindChild("dest")?.gameObject;

            if (dest == null)
            {
                QuickLogger.Error("Cannot find the dest gameobject");
                return false;
            }

            var boxcollider = dest.GetComponent<BoxCollider>();
            Destroy(boxcollider);

            var trigger = target.FindChild("trigger")?.gameObject;

            if (trigger == null)
            {
                QuickLogger.Error("Cannot find the trigger gameobject");
                return false;
            }

            var ladder = trigger.AddComponent<LadderController>();
            ladder.Target = dest;
            return true;
        }

        void PrintLog()
        {
            //QuickLogger.Debug($"Position {transform.position.y} || HasReachedDepth {_hasReachedDepth} || Frozen {_frozen} || BelowWater {IsBelowWater()}", true);

            //QuickLogger.Debug($" Above Water {IsAboveWater()} || Init {_init}", true);
        }

        private void Update()
        {
            //DropObject();

        }

        private void DropObject()
        {
            if (_init)
            {
                _timer -= 1 * DayNightCycle.main.deltaTime;

                if (_timer <= 0) return;
                _rigidBody.isKinematic = false;
            }
        }

        private void FixedUpdate()
        {
            if (IsBelowWater() && !_frozen)
            {
                if (Math.Round(transform.position.y) <= -4 && !_hasReachedDepth)
                {
                    _hasReachedDepth = true;
                    _rigidBody.isKinematic = true;
                    _begingLerp = true;
                }

                if (_hasReachedDepth)
                {
                    if (Math.Floor((transform.position.y)) >= 0)
                    {
                        _rigidBody.isKinematic = true;
                        _frozen = true;
                        //TODO put in IEnumerator and wait for a few seconds to diable iskientic
                    }
                }
            }

            if (_begingLerp)
            {
                LerpToZero();
            }

        }

        private void LerpToZero()
        {
            var targetPos = new Vector3(transform.position.x, 0, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.2f * DayNightCycle.main.deltaTime);
            if (transform.position == targetPos)
            {
                _begingLerp = false;
            }
        }

        public bool IsAboveWater()
        {
            return transform.position.y > 0.0;
        }

        public bool IsBelowWater()
        {
            return transform.position.y < 0;
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        public bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return false;
        }

        public void OnConstructedChanged(bool constructed)
        {
  
        }
    }
}
