using FCSCommon.Objects.Modules;
using UnityEngine;

namespace FCSCommon.Controllers
{
    public class AIDeepDrillerSolarController : DeepDrillerModule
    {
        public float maxDepth = 200f;
        private PowerSource powerSource;
        private AnimationCurve depthCurve;
        public bool Inserted { get; set; }


        public void Awake()
        {
            depthCurve = new AnimationCurve();
            depthCurve.AddKey(0, 0);
            depthCurve.AddKey(0.5001081f, 0.4245796f);
            depthCurve.AddKey(1, 1);

            powerSource = gameObject.GetComponent<PowerSource>();
        }

        private float GetDepthScalar()
        {
            if (DeepDrillerObject != null)
            {
                return this.depthCurve.Evaluate(Mathf.Clamp01((this.maxDepth - Ocean.main.GetDepthOf(this.DeepDrillerObject)) / this.maxDepth));
            }
            else
            {
                return 0f;
            }
        }


        private float GetSunScalar()
        {
            return DayNightCycle.main.GetLocalLightScalar();
        }

        private float GetRechargeScalar()
        {
            return this.GetDepthScalar() * this.GetSunScalar();
        }

        private void Update()
        {
            this.powerSource.power = Mathf.Clamp(this.powerSource.power + (float)((double)this.GetRechargeScalar() * (double)DayNightCycle.main.deltaTime * 0.25 * 5.0), 0.0f, this.powerSource.maxPower);
        }
    }
}
