using FCS_AlterraHub.API;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using System;
using System.Text;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono;
internal class BaseOxygenTankController : FCSDevice, IFCSSave
{
    private bool _runStartUpOnEnable;
    private bool _isFromSave;
    private OxygenTankAttachPoint _oxygenAttachPoint;
    private StringBuilder _sb = new StringBuilder();
    private HoverInteraction hoverInteraction;

    public override void Awake()
    {
        base.Awake();
        hoverInteraction = gameObject.GetComponent<HoverInteraction>();
        _oxygenAttachPoint = gameObject.GetComponent<OxygenTankAttachPoint>();
        hoverInteraction.onSettingsKeyPressed += HoverInteraction_onSettingsKeyPressed;
    }

    private void HoverInteraction_onSettingsKeyPressed(TechType obj)
    {
        _oxygenAttachPoint.allowConnection = !_oxygenAttachPoint.allowConnection;
        if (_oxygenAttachPoint.parentPipeUID != null)
            _oxygenAttachPoint.SetParent(null);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (_isFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                var save = _savedData as BaseOxygenTankEntry;

                //_colorManager.LoadTemplate(save.ColorTemplate);

                if (!string.IsNullOrEmpty(save.ParentID))
                    _oxygenAttachPoint.parentPipeUID = save.ParentID;
            }

            _runStartUpOnEnable = false;
        }
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        QuickLogger.Debug("In OnProtoDeserialize");

        if (_savedData == null)
        {
            ReadySaveData();
        }

        _isFromSave = true;
    }

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;
        if (constructed)
        {
            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                IsInitialized = true;
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }
        else
        {
            if (_oxygenAttachPoint != null)
                _oxygenAttachPoint.SetParent(null);
        }
    }

    public IPipeConnection GetRootOxygenProvider()
    {
        return _oxygenAttachPoint.GetParent()?.GetRoot();
    }
    public override void ReadySaveData()
    {

    }

    public void SaveDevice()
    {
        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new BaseOxygenTankEntry();
        }

        var save = _savedData as BaseOxygenTankEntry;

        save.Id = GetPrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();
        save.ParentID = _oxygenAttachPoint.parentPipeUID;
        FCSModsAPI.PublicAPI.PushSaveData(save);
        QuickLogger.Debug($"Saving ID {save.Id}");
        //newSaveData.Data.Add(_savedData);
    }
}
