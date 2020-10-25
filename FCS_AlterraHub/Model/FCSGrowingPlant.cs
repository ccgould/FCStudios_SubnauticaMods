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

        public void Initialize(GrowingPlant growingPlant, IFCSGrowBed growBed, Vector3 slotBounds)
        {

            QuickLogger.Debug($"Slot Bounds {slotBounds}", true);

            var colliders = gameObject.GetComponentsInChildren<Collider>();
            var bounds = colliders[0].bounds;
            foreach (var c in colliders)
            {
                bounds.Encapsulate(c.bounds);
            }

            var szA = slotBounds;
            var szB = bounds.size;

            var targetX = szA.x / szB.x;
            var targetY = szA.y / szB.y;
            var targetZ = szA.z / szB.z;

            var minScale = Mathf.Min(new[] { targetX, targetY, targetZ });

            

            QuickLogger.Debug($"Min Scale {minScale}", true);


            _growBed = growBed;

            growthDuration = growingPlant.growthDuration;

            growthWidth = growingPlant.growthWidth;

            growthHeight = growingPlant.growthHeight;

            growthWidthIndoor = growingPlant.growthWidthIndoor;

            growthHeightIndoor = growingPlant.growthHeightIndoor;

            positionOffset = growingPlant.positionOffset;

            growingTransform = growingPlant.growingTransform;

            grownModelPrefab = growingPlant.grownModelPrefab;

            seed = growingPlant.seed;

            growingPlant.SetProgress(1f);
            growingPlant.SetScale(gameObject.transform, 1f);

            Destroy(gameObject.GetComponent<Pickupable>());
            Destroy(gameObject.GetComponent<UniqueIdentifier>());

            Destroy(growingPlant);
            
            SpawnGrownModel(minScale);
        }

        private void OnEnable()
        {
            this.ShowGrowingTransform();
        }

        private void OnDisable()
        {
            //this.growingTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            //float progress = this.GetProgress();
            //this.SetScale(this.growingTransform, progress);
            this.SetPosition(this.growingTransform);
            //if (progress == 1f)
            //{
            //    this.SpawnGrownModel();
            //}
        }

        private void SpawnGrownModel(float minScale)
        {
            this.growingTransform.gameObject.SetActive(false);
            GameObject gameObject = Instantiate<GameObject>(this.grownModelPrefab, this.growingTransform.position, this.growingTransform.rotation);
            if (this.isPickupable)
            {
                Plantable component = gameObject.GetComponent<Plantable>();
                if (component != null && this.seed.ReplaceSeedByPlant(component))
                {
                    gameObject.SetActive(false);
                    ScaleObject(minScale, gameObject);
                    Destroy(component);
                    return;
                }
            }
            GrownPlant grownPlant = gameObject.AddComponent<GrownPlant>();
            grownPlant.seed = this.seed;
            grownPlant.SendMessage("OnGrown", SendMessageOptions.DontRequireReceiver);
            gameObject.transform.parent = _growBed.grownPlantsRoot.transform;
            _growBed.SetupRenderers(gameObject, true);
            ScaleObject(minScale, gameObject);

            base.enabled = false;
        }

        private void ScaleObject(float minScale, GameObject gameObject)
        {
            gameObject.transform.localScale = _invalidAdjustTechTypes.Contains(seed.plantTechType)
                ? new Vector3(minScale, minScale, minScale)
                : new Vector3(minScale * 0.3f, minScale * 0.3f, minScale * 0.3f);
        }

        private void ShowGrowingTransform()
        {
            if(growingTransform == null) return;
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

            float num = this.isIndoor ? this.growthWidthIndoor.Evaluate(progress) : this.growthWidth.Evaluate(progress);
            float y = this.isIndoor ? this.growthHeightIndoor.Evaluate(progress) : this.growthHeight.Evaluate(progress);
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
            //progress = Mathf.Clamp(progress, 0f, this.maxProgress);
            //this.SetScale(this.growingTransform, progress);
            //this.timeStartGrowth = DayNightCycle.main.timePassedAsFloat - this.GetGrowthDuration() * progress;
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

        public float heightProgressFactor = 0.5f;

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
    }
}
