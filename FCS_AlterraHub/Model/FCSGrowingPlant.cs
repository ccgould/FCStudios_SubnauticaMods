using System;
using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Model
{
    public class FCSGrowingPlant : MonoBehaviour
    {
        private readonly List<TechType> _invalidAdjustTechTypes = new List<TechType>
        {
            TechType.CreepvineSeedCluster,
            TechType.Creepvine,
            TechType.CreepvinePiece,
            TechType.BloodOil,
            TechType.BloodGrass,
            TechType.BloodRoot,
            TechType.BloodVine
        };

        public void Initialize(GrowingPlant growingPlant, IFCSGrowBed growBed, Vector3 slotBounds,Action<TechType> callback)
        {
            _callback = callback;
            _growBed = growBed;
            _slotBounds = slotBounds;
  

            seed = growingPlant.seed;
            
            positionOffset = growingPlant.positionOffset;

            heightProgressFactor = growingPlant.heightProgressFactor;

            growingTransform = growingPlant.growingTransform;

            grownModelPrefab = growingPlant.grownModelPrefab;


            _minScale = !_invalidAdjustTechTypes.Contains(seed.plantTechType) ? Mathf.Clamp(GetMinScale() * 0.3f, 0, 1) : Mathf.Clamp(GetMinScale(), 0, 1);


            growthWidth = new AnimationCurve(new Keyframe(0, 0.01f), new Keyframe(1, _minScale));
            growthWidth.SmoothTangents(0,0.8f);
            growthWidth.SmoothTangents(1,0.8f);


            growthHeight = new AnimationCurve(new Keyframe(0, 0.01f), new Keyframe(1, _minScale));
            growthHeight.SmoothTangents(0, 0.8f);
            growthHeight.SmoothTangents(1, 0.8f);

            growthWidthIndoor = new AnimationCurve(new Keyframe(0, 0.01f), new Keyframe(1, _minScale));
            growthWidthIndoor.SmoothTangents(0, 0.8f);
            growthWidthIndoor.SmoothTangents(1, 0.8f);

            growthHeightIndoor = new AnimationCurve(new Keyframe(0, 0.01f), new Keyframe(1, _minScale));
            growthHeightIndoor.SmoothTangents(0, 0.8f);
            growthHeightIndoor.SmoothTangents(1, 0.8f);

            growthDuration = growingPlant.growthDuration;

            //growthWidth = growingPlant.growthWidth;

            //growthHeight = growingPlant.growthHeight;

            //growthWidthIndoor = growingPlant.growthWidthIndoor;

            //growthHeightIndoor = growingPlant.growthHeightIndoor;
            
            Destroy(growingPlant);

            ShowGrowingTransform();
        }

        private float GetMinScale()
        {
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            var bounds = colliders[0].bounds;
            foreach (var c in colliders)
            {
                bounds.Encapsulate(c.bounds);
            }

            var szA = _slotBounds;
            var szB = bounds.size;

            var targetX = szA.x / szB.x;
            var targetY = szA.y / szB.y;
            var targetZ = szA.z / szB.z;

            return Mathf.Min(targetX, targetY, targetZ);
        }

        private void OnEnable()
        {
            this.ShowGrowingTransform();
        }

        private void OnDisable()
        {
            this.growingTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            float progress = this.GetProgress();
            this.SetScale(this.growingTransform, progress);
            this.SetPosition(this.growingTransform);
            if (progress == 1f)
            {
                this.SpawnGrownModel();
            }
        }

        private void SpawnGrownModel()
        {
            try
            {
                this.growingTransform.gameObject.SetActive(false);
                GameObject gameObject = Instantiate<GameObject>(this.grownModelPrefab, this.growingTransform.position,
                    this.growingTransform.rotation);
                this.SetScale(gameObject.transform, 1f);
                if (this.isPickupable)
                {
                    Plantable component = gameObject.GetComponent<Plantable>();
                    if (component != null && this.seed.ReplaceSeedByPlant(component))
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                }

                GrownPlant grownPlant = gameObject.AddComponent<GrownPlant>();
                grownPlant.seed = this.seed;
                grownPlant.SendMessage("OnGrown", SendMessageOptions.DontRequireReceiver);
                gameObject.transform.parent = _growBed.grownPlantsRoot.transform;
                _growBed.SetupRenderers(gameObject, _growBed.IsInBase());
                
                if (seed != null)
                {
                    var pickPrefab = gameObject.GetComponentInChildren<PickPrefab>();
                    if (pickPrefab != null)
                    {
                        QuickLogger.Debug($"Adding {pickPrefab.pickTech.AsString()} to samples", true);

                        _callback?.Invoke(pickPrefab.pickTech);
                    }
                    else
                    {
                        QuickLogger.Debug($"Adding {seed.GetComponent<TechTag>().type.AsString()} to samples", true);

                        _callback?.Invoke(seed.GetComponent<TechTag>().type);
                    }
                }
            }
            finally
            {
                base.enabled = false;
            }
        }

        private void ShowGrowingTransform()
        {
            if (growingTransform == null) return;
            if (this.growingTransform.gameObject.activeSelf)
            {
                return;
            }

            this.passYbounds = this.growingTransform.GetComponent<VFXPassYboundsToMat>();

            if (this.passYbounds == null)
            {
                this.wavingScaler = this.growingTransform.gameObject.EnsureComponent<VFXScaleWaving>();
            }

            this.growingTransform.gameObject.SetActive(true);
        }

        public void SetScale(Transform tr, float progress)
        {

            float num = Mathf.Clamp(this.growthWidth.Evaluate(progress),0,_minScale);
            float y = Mathf.Clamp(this.growthHeight.Evaluate(progress),0, _minScale);
            tr.localScale = new Vector3(num, y, num);
            if (this.passYbounds != null)
            {
                this.passYbounds.UpdateWavingScale(tr.localScale);
                return;
            }
            if (this.wavingScaler != null)
            {
                this.wavingScaler.UpdateWavingScale(tr.localScale);
            }
        }

        public void SetPosition(Transform tr)
        {
            Vector3 localScale = tr.localScale;
            Vector3 position = new Vector3(localScale.x * this.positionOffset.x, localScale.y * this.positionOffset.y, localScale.z * this.positionOffset.z);
            tr.position = base.transform.TransformPoint(position);
        }

        public void EnableIndoorState()
        {
            this.isIndoor = true;
        }

        private float GetGrowthDuration()
        {
            float num = NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f;
            return this.growthDuration * num;
        }

        public float GetProgress()
        {
            if (this.timeStartGrowth == -1f)
            {
                this.SetProgress(0f);
                return 0f;
            }
            return Mathf.Clamp((float)(DayNightCycle.main.timePassed - (double)this.timeStartGrowth) / this.GetGrowthDuration(), 0f, this.maxProgress);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp(progress, 0f, this.maxProgress);
            this.SetScale(this.growingTransform, progress);
            this.timeStartGrowth = DayNightCycle.main.timePassedAsFloat - this.GetGrowthDuration() * progress;
        }

        public void SetMaxHeight(float height)
        {
            if (this.heightProgressFactor <= 0f)
            {
                return;
            }
            if (this.GetProgress() >= this.maxProgress)
            {
                this.SetProgress(this.maxProgress);
            }
            this.maxProgress = Mathf.Clamp01(height * this.heightProgressFactor);
        }


        public float growthDuration = 1200f;

        public AnimationCurve growthWidth;

        public AnimationCurve growthHeight;

        public AnimationCurve growthWidthIndoor;

        public AnimationCurve growthHeightIndoor;

        public Vector3 positionOffset = Vector3.zero;

        public float heightProgressFactor;

        public Transform growingTransform;

        public GameObject grownModelPrefab;

        public Plantable seed;

        public bool isPickupable;

        private float timeStartGrowth = -1f;

        private float maxProgress = 1f;

        private bool isIndoor;

        private VFXPassYboundsToMat passYbounds;

        private VFXScaleWaving wavingScaler;
        private IFCSGrowBed _growBed;
        private Vector3 _slotBounds;
        private float _minScale;
        private Action<TechType> _callback;
    }
}
