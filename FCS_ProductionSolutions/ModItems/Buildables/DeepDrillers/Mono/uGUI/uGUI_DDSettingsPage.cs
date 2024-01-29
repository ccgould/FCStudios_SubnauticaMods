using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DDSettingsPage : Page, IuGUIAdditionalPage
{
    private DrillSystem _sender;

    public override void Enter(object obj)
    {
        base.Enter(obj);


        if (obj is not null)
        {
            _sender = obj as DrillSystem;
        }
    }

    public IFCSObject GetController()
    {
        return _sender;
    }
}
