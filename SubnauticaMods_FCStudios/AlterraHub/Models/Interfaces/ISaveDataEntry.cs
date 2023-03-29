namespace FCS_AlterraHub.Models.Interfaces;

public interface ISaveDataEntry
{
    string Id { get; set; }
    string BaseId { get; set; }
    ColorTemplateSave ColorTemplate { get; set; }
}
