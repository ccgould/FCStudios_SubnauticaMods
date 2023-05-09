namespace FCS_AlterraHub.Models.Interfaces;

/// <summary>
/// This interface defines an FCStudios mod all mods must contain a class with this interface.
/// </summary>
public interface IModSettingsBase
{
    string AssetBundleName { get; }  
    string ModPackID { get; }

}
