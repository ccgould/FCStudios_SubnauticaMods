using System;
using DataStorageSolutions.Buildables;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    public class DSSAutoCrafterCrateController : MonoBehaviour
    {

        // Array of waypoints to walk from one to the next one
        [SerializeField]
        private Transform[] waypoints;

        public TechType TechType
        {
            get => _techType;
            set
            {
                _techType = value;
                SetIcon();
            }
        }

        private void SetIcon()
        {
            var canvas = GameObjectHelpers.FindGameObject(gameObject, "Canvas");
            if (canvas != null)
            {
                foreach (Transform child in canvas.transform)
                {
                    var icon = child.gameObject.AddComponent<uGUI_Icon>();
                    icon.sprite = SpriteManager.Get(TechType);
                }
            }
        }

        // Walk speed that can be set in Inspector
        [SerializeField]
        private float moveSpeed = .1f;

        // Index of current waypoint from which Enemy walks
        // to the next one
        private int waypointIndex = 0;
        private TechType _techType;
        private BaseManager _manager;

        public Action OnEndOfPath { get; set; }
        public Func<Status> Status { get; set; }
        public int Amount { get; set; }

        // Use this for initialization
        internal void Initialize(BaseManager manager, Transform[] newWaypoints)
        {
            _manager = manager;
            waypoints = newWaypoints;
            // Set position of Enemy as position of the first waypoint
            transform.position = waypoints[waypointIndex].transform.position;
            transform.rotation = waypoints[waypointIndex].transform.rotation;

            //========== Allows the building animation and material colors ==========// 
            //DSSModelPrefab.ApplyShaders(gameObject,DSSModelPrefab.Bundle);
            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            SkyApplier skyApplier = gameObject.EnsureComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;
                }
            }
            //========== Allows the building animation and material colors ==========// 


        }

        // Update is called once per frame
        private void Update()
        {
            if (waypoints == null) return;
            // Move Enemy
            Move();
        }

        // Method that actually make Enemy walk
        private void Move()
        {
            if (Status?.Invoke() != Mono.Status.Running) return;
            // If Enemy didn't reach last waypoint it can move
            // If enemy reached last waypoint then it stops
            if (waypointIndex <= waypoints.Length - 1)
            {

                // Move Enemy from current waypoint to the next one
                // using MoveTowards method
                transform.position = Vector3.MoveTowards(transform.position,
                    waypoints[waypointIndex].transform.position,
                    moveSpeed * Time.deltaTime);


                // If Enemy reaches position of waypoint he walked towards
                // then waypointIndex is increased by 1
                // and Enemy starts to walk to the next waypoint
                if (transform.position == waypoints[waypointIndex].transform.position)
                {
                    waypointIndex += 1;
                    // Makes the craft start another at the second waypoint disabled because the system was crafting 2 if the max is one
                    //if (waypointIndex == 2 && !_secondPointArrived)
                    //{
                    //    OnEndOfPath?.Invoke();
                    //    _secondPointArrived = true;
                    //}
                }
            }
            else
            {
                OnEndOfPath?.Invoke();
                for (int i = 0; i < Amount; i++)
                {
                    _manager.StorageManager.AddItemToContainer(TechType.ToInventoryItem());
                }

                Destroy(gameObject);
            }
        }
    }

}
