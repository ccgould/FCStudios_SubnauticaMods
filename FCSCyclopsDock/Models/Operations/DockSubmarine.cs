using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCSCyclopsDock.Models.Operations
{
    public class DockSubmarine : MonoBehaviour
    {
        public bool enter = true;
        public bool stay = true;
        public bool exit = true;
        private float stayCount = 0.0f;
        public float interpolationTime = 1f;
        private float timeDockingStarted;
        public Transform dockingEndPos;
        private SubRoot _sub;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private readonly bool powered;
        private readonly Vehicle dockedVehicle;
        private SubControl _subControl;
        private Vector3 _velocity;

        public float movementSpeed = 5f; //for instance
        private bool _docked;

        // Use this for initialization
        void Start()
        {
            Debug.Log("start");
            _velocity = new Vector3(1.75f, 1.1f, 1.75f);

            dockingEndPos = gameObject.FindChild("model").FindChild("Stop")?.transform;
            if (dockingEndPos == null)
            {
                QuickLogger.Error($"Stop GameObject not found", true);
                throw new NullReferenceException("Stop GameObject not found");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (enter)
            {
                QuickLogger.Debug(dockingEndPos.position.ToString(), true);
                QuickLogger.Debug("entered", true);
                SubRoot componentInHierarchy = UWE.Utils.GetComponentInHierarchy<SubRoot>(other.gameObject);
                if (componentInHierarchy == null || GetDockedVehicle() ||
                    (GameModeUtils.RequiresPower() && !this.powered || _sub != null))
                {
                    QuickLogger.Debug("Docking procedure canceled");

                    QuickLogger.Debug($"componentInHierarchy {componentInHierarchy}");
                    QuickLogger.Debug($"GetDockedVehicle {GetDockedVehicle()}");
                    QuickLogger.Debug($"RequiresPower {GameModeUtils.RequiresPower()}");
                    QuickLogger.Debug($"powered {powered}");
                    QuickLogger.Debug($"_sub {_sub}");
                    return;
                }

                _sub = componentInHierarchy;
                _subControl = UWE.Utils.GetComponentInHierarchy<SubControl>(other.gameObject);
                this.startPosition = this._sub.transform.position;
                this.startRotation = this._sub.transform.rotation;

                QuickLogger.Debug($"Enter Position = {startPosition} || Enter Rot = {startRotation}", true);

                this.timeDockingStarted = Time.time;

                //_subControl.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX |
                //                                                    RigidbodyConstraints.FreezeRotationY |
                //                                                    RigidbodyConstraints.FreezeRotationZ |
                //                                                    RigidbodyConstraints.FreezePositionX |
                //                                                    RigidbodyConstraints.FreezePositionY |
                //                                                    RigidbodyConstraints.FreezePositionZ;

                if (!_subControl.GetComponent<Rigidbody>().isKinematic)
                {
                    QuickLogger.Debug($"IsKinematic is false setting to true", true);
                    _subControl.GetComponent<Rigidbody>().isKinematic = true;
                }

                InvokeRepeating("CurrentPosition", 1, 1);
            }
        }

        private void CurrentPosition()
        {
            QuickLogger.Debug($"Current Position {_sub.transform.position} || Current Rot = {_sub.transform.rotation}",
                true);
        }

        // stayCount allows the OnTriggerStay to be displayed less often
        // than it actually occurs.


        private void OnTriggerStay(Collider other)
        {
            if (stay)
            {
                if (stayCount > 0.25f)
                {
                    //QuickLogger.Debug("staying");
                    stayCount = stayCount - 0.25f;
                }
                else
                {
                    stayCount = stayCount + Time.deltaTime;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (exit)
            {
                QuickLogger.Debug("exit", true);
                _sub = null;
                CancelInvoke("CurrentPosition");
            }
        }

        private void FixedUpdate()
        {
            if (_sub != null)
            {
                var interpfraction = 1f;
                if (interpolationTime > 0.0)
                    interpfraction = Mathf.Clamp01((Time.time - timeDockingStarted) / interpolationTime);
                if (!_docked)
                {
                    _subControl.GetComponent<Rigidbody>().isKinematic = true;
                }
                if (_subControl.GetComponent<Rigidbody>().isKinematic)
                {
                    UpdateDockedPosition(interpfraction);
                }
            }
        }

        private void UpdateDockedPosition(float interpfraction)
        {
            if (_sub.isCyclops)
            {
                //Transform transform = new RectTransform();
                //transform.position = new Vector3(0f, 11.1f, -10f);
                //transform.rotation = new Quaternion(0,0,0,0);

                //Vector3 moveDir = (new Vector3(0f, 11.1f, -10f) - _sub.transform.position).normalized;
                //Vector3 nextFramePos = moveDir * 10f * DayNightCycle.main.deltaTime;
                //_subControl.GetComponent<Rigidbody>().MovePosition(nextFramePos);

                Vector3 direction = (dockingEndPos.position - _sub.transform.position).normalized;
                _subControl.GetComponent<Rigidbody>()
                    .MovePosition(_sub.transform.position + direction * movementSpeed * DayNightCycle.main.deltaTime);
                //_subControl.GetComponent<Rigidbody>().MoveRotation(Quaternion.Lerp(this.startRotation, new Quaternion(0, 0, 0, 0), interpfraction));

                QuickLogger.Debug($"Current Position:{_sub.transform.position}", true);

                if (Mathf.Approximately(_sub.transform.position.x, dockingEndPos.position.x) && Mathf.Approximately(_sub.transform.position.z, dockingEndPos.position.z))
                {
                    _subControl.GetComponent<Rigidbody>().isKinematic = false;
                    QuickLogger.Debug($"Kinectic {_subControl.GetComponent<Rigidbody>().isKinematic}", true);

                    _docked = true;
                    _subControl.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    _subControl.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    #region Omit
                    //_subControl.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX |
                    //                                                    RigidbodyConstraints.FreezeRotationY |
                    //                                                    RigidbodyConstraints.FreezeRotationZ |
                    //                                                    RigidbodyConstraints.FreezePositionX |
                    //                                                    RigidbodyConstraints.FreezePositionY |
                    //                                                    RigidbodyConstraints.FreezePositionZ;

                    //if (_subControl.GetComponent<Rigidbody>().velocity.magnitude < .01)
                    //{
                    //    _subControl.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    //    _subControl.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    //}

                    // _sub.transform.position = Vector3.Lerp(this.startPosition, new Vector3(0f, 11.1f, -10f), interpfraction);
                    // _sub.transform.rotation = Quaternion.Lerp(this.startRotation, new Quaternion(0, 0, 0, 0), interpfraction); 
                    #endregion
                }
            }


        }

        public Vehicle GetDockedVehicle()
        {
            return this.dockedVehicle;
        }
    }
}
