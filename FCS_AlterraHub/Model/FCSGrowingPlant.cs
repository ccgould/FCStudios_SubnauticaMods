using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Interfaces;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_AlterraHub.Model
{
    public class FCSGrowingPlant : MonoBehaviour
    {

        private void SaveSnapShot()
        {
            if(Mod.HeightRestrictions.ContainsKey(seed.plantTechType)) return;
            Mod.HeightRestrictions.Add(seed.plantTechType,_y);
            using (StreamWriter sw = new StreamWriter(@"f:\json.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                _serializer.Serialize(writer, Mod.HeightRestrictions);
            }
        }

        public void Initialize(GrowingPlant growingPlant, IFCSGrowBed growBed, Vector3 slotBounds,Action<TechType> callback)
        {
            _serializer = new JsonSerializer();
            _callback = callback;
            _growBed = growBed;
            _slotBounds = slotBounds;
  

            seed = growingPlant.seed;
            
            positionOffset = growingPlant.positionOffset;

            heightProgressFactor = growingPlant.heightProgressFactor;

            growingTransform = growingPlant.growingTransform;

            grownModelPrefab = growingPlant.grownModelPrefab;
            
            _minScale = Mod.HeightRestrictions.ContainsKey(seed.plantTechType) ? Mod.HeightRestrictions[seed.plantTechType] : 1f;

            QuickLogger.Debug($"Setting minScale: {_minScale} || TechType: {seed.plantTechType} || bounds {slotBounds} || Get Minscale: {GetMinScale()}",true);

            growthWidth = new AnimationCurve(new Keyframe(0, 0.01f), new Keyframe(1, _minScale));
            growthWidth.SmoothTangents(0,0.8f);
            growthWidth.SmoothTangents(1,0.8f);


            growthHeight = new AnimationCurve(new Keyframe(0, 0.01f), new Keyframe(1, _minScale));
            growthHeight.SmoothTangents(0, 0.8f);
            growthHeight.SmoothTangents(1, 0.8f);
            
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
            ShowGrowingTransform();
        }

        private void OnDisable()
        {
            growingTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            float progress = GetProgress();
            SetScale(growingTransform, progress);
            SetPosition(growingTransform);
            if (progress == 1f)
            {
                SpawnGrownModel();
            }
        }

        private void SpawnGrownModel()
        {
            try
            {
                QuickLogger.Debug("1");
                growingTransform.gameObject.SetActive(false);
                QuickLogger.Debug("2");
                GameObject go = Instantiate<GameObject>(grownModelPrefab, growingTransform.position,growingTransform.rotation);
                QuickLogger.Debug("3");
                SetScale(go.transform, 1f);
                QuickLogger.Debug("4");
                if (isPickupable)
                {
                    QuickLogger.Debug("5");
                    Plantable component = go.GetComponent<Plantable>();
                    QuickLogger.Debug("6");
                    if (component != null && seed.ReplaceSeedByPlant(component))
                    {
                        QuickLogger.Debug("7");
                        go.SetActive(false);
                        QuickLogger.Debug("8");
                        return;
                    }
                }
                QuickLogger.Debug("9");
                GrownPlant grownPlant = go.AddComponent<GrownPlant>();
                QuickLogger.Debug("10");
                grownPlant.seed = seed;
                QuickLogger.Debug("11");
                grownPlant.SendMessage("OnGrown", SendMessageOptions.DontRequireReceiver);
                QuickLogger.Debug("12");
                go.transform.parent = _growBed.grownPlantsRoot.transform;
                QuickLogger.Debug("13");
                _growBed.SetupRenderers(go, _growBed.IsInBase());
                QuickLogger.Debug("14");

                if (seed != null)
                {
                    QuickLogger.Debug("15");
                    var pickPrefab = go.GetComponentInChildren<PickPrefab>();
                    QuickLogger.Debug("16");
                    if (pickPrefab != null)
                    {
                        QuickLogger.Debug("16.1");
                        QuickLogger.Debug($"Adding {pickPrefab.pickTech.AsString()} to samples", true);

                        _callback?.Invoke(pickPrefab.pickTech);
                        QuickLogger.Debug("17");
                    }
                    else
                    {
                        QuickLogger.Debug("16.2");
                        var techTag = seed.GetComponent<TechTag>();
                        
                        if(techTag != null)
                        {
                            QuickLogger.Debug($"Adding {seed.GetComponent<TechTag>().type.AsString()} to samples", true);
                            _callback?.Invoke(seed.GetComponent<TechTag>().type);
                        }
                        
                        QuickLogger.Debug("18");
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
            if (growingTransform.gameObject.activeSelf)
            {
                return;
            }

            passYbounds = growingTransform.GetComponent<VFXPassYboundsToMat>();

            if (passYbounds == null)
            {
                wavingScaler = growingTransform.gameObject.EnsureComponent<VFXScaleWaving>();
            }

            growingTransform.gameObject.SetActive(true);
        }

        public void SetScale(Transform tr, float progress)
        {

            _num = Mathf.Clamp(growthWidth.Evaluate(progress),0,_minScale);
            _y = Mathf.Clamp(growthHeight.Evaluate(progress),0, _minScale);
            tr.localScale = new Vector3(_num, _y, _num);
            if (passYbounds != null)
            {
                passYbounds.UpdateWavingScale(tr.localScale);
                return;
            }
            if (wavingScaler != null)
            {
                wavingScaler.UpdateWavingScale(tr.localScale);
            }
        }

        public void SetPosition(Transform tr)
        {
            Vector3 localScale = tr.localScale;
            Vector3 position = new Vector3(localScale.x * positionOffset.x, localScale.y * positionOffset.y, localScale.z * positionOffset.z);
            tr.position = base.transform.TransformPoint(position);
        }

        private float GetGrowthDuration()
        {
            float num = NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f;
            return growthDuration * num;
        }

        public float GetProgress()
        {
            if (timeStartGrowth == -1f)
            {
                SetProgress(0f);
                return 0f;
            }
            return Mathf.Clamp((float)(DayNightCycle.main.timePassed - (double)timeStartGrowth) / GetGrowthDuration(), 0f, maxProgress);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp(progress, 0f, maxProgress);
            SetScale(growingTransform, progress);
            timeStartGrowth = DayNightCycle.main.timePassedAsFloat - GetGrowthDuration() * progress;
        }
        
        public float growthDuration = 1200f;

        public AnimationCurve growthWidth;

        public AnimationCurve growthHeight;
        
        public Vector3 positionOffset = Vector3.zero;

        public float heightProgressFactor;

        public Transform growingTransform;

        public GameObject grownModelPrefab;

        public Plantable seed;

        public bool isPickupable;

        private float timeStartGrowth = -1f;

        private float maxProgress = 1f;

        private VFXPassYboundsToMat passYbounds;
        private VFXScaleWaving wavingScaler;
        private IFCSGrowBed _growBed;
        private Vector3 _slotBounds;
        private float _minScale;
        private Action<TechType> _callback;
        private float _y;
        private float _num;
        private JsonSerializer _serializer;
    }
}
