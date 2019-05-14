using FCSCommon.Helpers;
using UnityEngine;

namespace FCSCyclopsDock.Models.Controllers
{
    public class CyclopsDockController : MonoBehaviour
    {
        public string ID { get; private set; }

        private void Awake()
        {
            MaterialHelpers.ApplyCustomShader("No Name", "clouds1", transform, LoadItems.ASSETBUNDLE, LoadItems.DissolveShader, new Color(0.8627452f, 0.6784314f, 0.3098039f));
        }
    }
}
