using UnityEngine;

namespace FCS_AlterraHub.Interfaces
{
    public interface IFCSGrowBed
    {
        void SetupRenderers(GameObject gameObject, bool interior);
        GameObject grownPlantsRoot { get; set; }
    }
}
