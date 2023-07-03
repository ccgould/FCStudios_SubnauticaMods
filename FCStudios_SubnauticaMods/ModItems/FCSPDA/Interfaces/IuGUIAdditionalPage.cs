using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;

namespace FCS_AlterraHub.ModItems.FCSPDA.Interfaces;

public interface IuGUIAdditionalPage
{
    IFCSObject GetController();
    void Enter(object obj);
}
