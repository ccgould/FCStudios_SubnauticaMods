using FCS_AlterraHub.Core.Extensions;
using FCSCommon.Utilities;
using System.Collections.Generic;

namespace FCS_AlterraHub.ModItems.FCSPDA.Data.Models;

public class EncyclopediaData
{
    public string ModPackID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public List<EncyclopediaEntryData> Data { get; set; } = new();
}

public class EncyclopediaEntryData
{
    public string AudioName { get; set; }
    public string Path { get; set; }
    public string TabTitle { get; set; }
    public string Body { get; set; }
    public string Title { get; set; }
    public string ImageName { get; set; }
    public bool Unlocked { get; set; }
    public string UnlockedBy { get; set; }
    public string Blueprint { get; set; }
    public bool DestroyFragmentAfterScan { get; set; }
    public bool HasFragment { get; set; }
    public int TotalFragmentsToUnlock { get; set; }
    public int ScanTime { get; set; }
    private TechType _techType;
    public string TechTypeString { get; set; }


    public bool IsSame(TechType techType)
    {
        if (_techType == TechType.None)
        {
            _techType = TechTypeString.ToTechType();
        }

        QuickLogger.Debug($"Current TechType{_techType} || Comparison {techType}");

        if(_techType == TechType.None)
        {
            return false;
        }

        return _techType == techType;
    }

    public bool IsSame(string techType)
    {
        return techType.Equals(TechTypeString);
    }

    public string GetCategory()
    {
        var paths = Path.Split('/');
        return paths.GetLast();
    }

    public override string ToString()
    {
        return $"Title: {Title} | Tab Title: {TabTitle}";
    }
}