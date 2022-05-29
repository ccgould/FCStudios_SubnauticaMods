using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class LightManager : MonoBehaviour
    {
        public PlayerDistanceTracker DistanceTracker { get; set; }
        public Light Light { get; set; }
        
        private void Update()
        {
            if (DistanceTracker != null && Light != null)
            {
                Light.enabled = DistanceTracker.playerNearby;
            }
        }
    }
}