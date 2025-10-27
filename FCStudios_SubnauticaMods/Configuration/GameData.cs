using SQLite;
using static FCS_AlterraHub.Core.Services.SaveLoadDataService;

namespace FCS_AlterraHub.Configuration;
[Table("OreConsumer")]
public class GameData : IDBEntity
{
    [PrimaryKey]
    public string Id { get; set; }
}
