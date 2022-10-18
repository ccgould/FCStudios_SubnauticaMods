using System.Collections;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers.FCSAlterraHub;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubPod.Mono
{
    internal class AlterraHubPodController : SubRoot
    {
        private InteriorTrigger _interiorTrigger;

        public override void Awake()
        {
            _interiorTrigger = GameObjectHelpers.FindGameObject(gameObject ,"door_split").EnsureComponent<InteriorTrigger>();

            var canvas = gameObject.GetComponentsInChildren<Canvas>();
            foreach (Canvas canva in canvas)
            {
                var screen = canva.gameObject.EnsureComponent<FCSAlterraHubGUI>();
                screen.SetInstance();

            }

            this.LOD = GetComponent<BehaviourLOD>();
            this.rb = GetComponent<Rigidbody>();
            this.isBase = true;
            this.lightControl = GetComponentInChildren<LightingController>();
            this.modulesRoot = gameObject.transform;
            this.powerRelay = GetComponent<BasePowerRelay>();
            BaseManager.FindManager(this);

            StartCoroutine(TryApplyMaterial());
        }


        private IEnumerator TryApplyMaterial()
        {
            while (MaterialHelpers._waterMaterial == null)
            {
                yield return null;
            }


            var waterPlane = GameObjectHelpers.FindGameObject(gameObject, "water");
            waterPlane.SetActive(false);
            var gam = waterPlane.AddComponent<WaterPlane>();
            gam.material = MaterialHelpers._waterMaterial;
            gam.size = new Vector2(1, 1);
            waterPlane.SetActive(true);

            //var waterPlane = GameObjectHelpers.FindGameObject(gameObject, "WaterPlane");

            //QuickLogger.Info($"Water Plane Found {waterPlane is not null}");

            //MaterialHelpers.ApplyWaterShader(waterPlane);

            yield break;

        }

        internal class InteriorTrigger : HandTarget, IHandTarget
        {
            private SubRoot _subRoot;


            public override void Awake()
            {
                base.Awake();
                _subRoot = gameObject.GetComponentInParent<SubRoot>();
            }
            
            public void OnHandHover(GUIHand hand)
            {
                
            }

            public void OnHandClick(GUIHand hand)
            {
                if (_subRoot != null)
                {
                    Player.main.SetCurrentSub(Player.main.currentSub == null ? _subRoot : null);
                }
            }
        }
    }
}
