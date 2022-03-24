using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Mono
{
    internal class AutoCrafterCrateController : MonoBehaviour
    {
        // Array of waypoints to walk from one to the next one
        [SerializeField]
        private Transform[] waypoints;

        public Action OnPathComplete;


        private void SetIcon(TechType techType)
        {
            var canvas = gameObject.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                foreach (Transform child in canvas.transform)
                {
                    var icon = child.gameObject.AddComponent<uGUI_Icon>();
                    icon.sprite = SpriteManager.Get(techType);
                }
            }
        }

        // Walk speed that can be set in Inspector
        [SerializeField]
        private float moveSpeed = .1f;

        // Index of current waypoint from which Enemy walks
        // to the next one
        private int waypointIndex = 0;
        private AutoCrafterController _controller;
        //private TechType _techType;
        public int Amount { get; set; }


        // Use this for initialization
        internal void Initialize(Transform[] newWaypoints, AutoCrafterController controller, TechType techType)
        {
            _controller = controller;
            waypoints = newWaypoints;
            SetIcon(techType);
            // Set position of Enemy as position of the first waypoint
            transform.localPosition = waypoints[waypointIndex].transform.localPosition;
            transform.localRotation = waypoints[waypointIndex].transform.localRotation;
            MaterialHelpers.ApplyShaderToMaterial(gameObject, AlterraHub.BaseDecalsExterior);
            //Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            SkyApplier skyApplier = gameObject.EnsureComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.SetSky(Skies.BaseInterior);

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
            if (!_controller.IsBeltMoving()) return;
            // If Enemy didn't reach last waypoint it can move
            // If enemy reached last waypoint then it stops
            if (waypointIndex <= waypoints.Length - 1)
            {

                // Move Enemy from current waypoint to the next one
                // using MoveTowards method
                transform.localPosition = Vector3.MoveTowards(transform.localPosition,
                    waypoints[waypointIndex].transform.localPosition,
                    moveSpeed * Time.deltaTime);


                // If Enemy reaches position of waypoint he walked towards
                // then waypointIndex is increased by 1
                // and Enemy starts to walk to the next waypoint
                if (transform.localPosition == waypoints[waypointIndex].transform.localPosition)
                {
                    waypointIndex += 1;
                }
            }
            else
            {
                OnPathComplete?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
