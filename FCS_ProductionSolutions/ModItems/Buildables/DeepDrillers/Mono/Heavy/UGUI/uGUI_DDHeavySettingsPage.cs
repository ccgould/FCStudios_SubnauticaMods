using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy;
using FCSCommon.Utilities;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DDHeavySettingsPage : Page, IuGUIAdditionalPage
{
    private DeepDrillerOperatorController _sender;

    public override void Enter(object obj)
    {
        QuickLogger.Debug("Entering Settings Page", true);

        base.Enter(obj);

        if (obj is not null)
        {
            _sender = obj as DeepDrillerOperatorController;
        }
    }

    public IFCSObject GetController()
    {
        return _sender;
    }
}
