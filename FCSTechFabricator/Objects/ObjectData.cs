using System.Collections.Generic;
using FCSCommon.Enums;
using FCSTechFabricator.Enums;
using Oculus.Newtonsoft.Json;

namespace FCSTechFabricator.Objects
{
    public class ObjectData
    {
        [JsonProperty] public SaveDataObjectType DataObjectType { get; set; }
        [JsonProperty] public TechType TechType { get; set; }
        [JsonProperty] public int Slot { get; set; }
        [JsonProperty] public EatableEntities EatableEntity { get; set; }
        [JsonProperty] public PlayerToolData PlayToolData { get; set; }
        [JsonProperty] public HashSet<ObjectData> ServerData { get; set; }
        [JsonProperty] public List<Filter> Filters { get; set; }

        public ObjectData()
        {
            
        }

        public ObjectData(ObjectData obj)
        {
            if (obj == null) return;
            DataObjectType = obj.DataObjectType;
            TechType = obj.TechType;
            Slot = obj.Slot;
            EatableEntity = obj.EatableEntity;
            PlayToolData = obj.PlayToolData;
            ServerData = obj.ServerData;
            Filters = obj.Filters;
        }
    }
}
