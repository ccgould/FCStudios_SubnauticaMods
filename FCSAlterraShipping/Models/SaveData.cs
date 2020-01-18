using FCSAlterraShipping.Enums;
using FCSCommon.Objects;
using System.Collections.Generic;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace FCSAlterraShipping.Models
{
    internal class SaveData
    {
        [JsonProperty] internal bool HasBreakerTripped { get; set; }
        [JsonProperty] internal float TimeTillDeletion { get; set; }
        [JsonProperty] internal float TimeLeft { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> ContainerItems { get; set; } = new Dictionary<TechType, int>();
        [JsonProperty] internal string Target { get; set; }
        [JsonProperty] internal int CurrentPage { get; set; }
        [JsonProperty] internal bool CurrentDoorState { get; set; }
        [JsonProperty] internal ShippingContainerStates ContainertMode { get; set; }
        [JsonProperty] internal string ContainerName { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
    }
}
