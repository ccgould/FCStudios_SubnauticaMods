using FCSAIPowerCellSocket.Model;
using UnityEngine;

namespace FCSAIPowerCellSocket.Mono
{
    internal class AIPowerCellSocketController : MonoBehaviour
    {
        private void Awake()
        {
            PowerManager = gameObject.AddComponent<AIPowerCellSocketPowerManager>();
            PowerManager.Initialize(this);
        }

        public AIPowerCellSocketPowerManager PowerManager { get; set; }

        private void OnDestroy()
        {

        }
    }
}
