using FCS_AlterraHub.ModItems.FCSPDA.Enums;

namespace FCS_AlterraHub.Models.Structs;

internal struct StoreModCategory
{
    public StoreModCategory(string modPackGUID, string iconName, string pageName, PDAPages pdaPage)
    {
        ModPackGUID = modPackGUID;
        IconName = iconName;
        PageName = pageName;
        PDAPage = pdaPage;
    }

    public string ModPackGUID { get; }
    public string IconName { get; }
    public string PageName { get; }
    public PDAPages PDAPage { get; }
}