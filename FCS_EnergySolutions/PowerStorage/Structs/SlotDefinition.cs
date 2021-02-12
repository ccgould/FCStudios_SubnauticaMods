using FCS_EnergySolutions.PowerStorage.Mono;
using UnityEngine.UI;

namespace FCS_EnergySolutions.PowerStorage.Structs
{
    internal struct SlotDefinition
    {
        public string id;
        public BatteryDummyController battery;
        public Image bar;
        public Text text;

        public bool IsOccupied()
        {
            return battery.GetIsVisible();
        }

    }
}