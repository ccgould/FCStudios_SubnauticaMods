using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
internal class SkyApplierFixer : MonoBehaviour
{
    [SerializeField] private SkyApplier skyApplier;
    [SerializeField] private Skies sky;

    private void Update()
    {
        var sky = skyApplier.applySky;



        if (sky is not null && !sky.name.Contains("SkyBaseInterior"))
        {
            FixSky();
        }
        else
        {
            enabled = false;
        }
    }

    public void FixSky()
    {
        if(skyApplier is not null)
        {
            skyApplier.applySky = null;

            skyApplier.Start();
        }
    }
}
