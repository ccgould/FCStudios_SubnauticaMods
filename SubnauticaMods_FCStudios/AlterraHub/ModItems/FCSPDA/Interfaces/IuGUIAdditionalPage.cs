using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using System;

namespace FCS_AlterraHub.ModItems.FCSPDA.Interfaces;

public interface IuGUIAdditionalPage
{
    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;

    void Hide();
    void Initialize(object obj);
}
