using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace HorizontalDuck.Config
{
    internal class SaveData
    {
        [JsonProperty] internal Vec3 Size { get; set; }
    }
}
