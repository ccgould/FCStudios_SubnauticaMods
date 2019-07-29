using FCS_DeepDriller.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Managers
{
    internal class VFXManager : MonoBehaviour
    {
        public VFXController fxControl;
        public FMOD_CustomLoopingEmitter bubblesSound;
        private FCSDeepDrillerController _mono;


        internal void Initialize(FCSDeepDrillerController mono)
        {
            GameObject pipe = CraftData.GetPrefabForTechType(TechType.Pipe);

            GameObject go = null;

            if (pipe != null)
            {
                QuickLogger.Debug("Making a copy of pipe");
                go = GameObject.Instantiate(pipe);
                go.transform.SetParent(_mono.transform);

                go.transform.localPosition = Vector3.zero;

                QuickLogger.Debug("Made a copy of pipe");
            }
            else
            {
                QuickLogger.Debug($"{nameof(VFXManager)} Pipe gameobject not found");
                return;
            }

            var pickupable = go.GetComponent<Pickupable>();

            if (pickupable != null)
            {
                Destroy(pickupable);
            }
            else
            {
                QuickLogger.Debug($"{nameof(VFXManager)} Not pickable found");
                return;

            }

            var oxygenPipe = go.GetComponent<OxygenPipe>();

            if (oxygenPipe != null)
            {
                fxControl = oxygenPipe.fxControl;

                bubblesSound = oxygenPipe.bubblesSound;
            }
            else
            {
                QuickLogger.Debug($"{nameof(VFXManager)} Not oyxgen pipe found");
                return;
            }

            _mono = mono;
        }

        internal void UpdateVFX()
        {
            if (fxControl == null || bubblesSound == null) return;

            bool flag2 = fxControl.emitters[0].instanceGO != null;

            if (_mono.LavaPitHandler.IsLavaActive())
            {
                if (!flag2)
                {
                    this.fxControl.Play();
                }
                bubblesSound.Play();
            }
            else
            {
                bubblesSound.Stop();
                if (flag2)
                {
                    fxControl.Stop();
                }
            }
        }



    }
}
