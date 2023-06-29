using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseTransmitter.Mono;
internal class ConnectedGridExtensionController : FCSDevice, IFCSSave<SaveData>
{
    private bool _isBaseManagerBuilt;

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override bool IsDeconstructionObstacle()
    {
        return true;
    }

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;   
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {

    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {

    }

    public override void ReadySaveData()
    {

    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saves Connection Grid Extension", true);


        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            //_savedData = new ConnectionGridExtensionSaveData();
        }

        //var save = _savedData as CubeGeneratorSaveData;

        //save.Id = GetPrefabID();
        //save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        //save.ColorTemplate = _colorManager.SaveTemplate();

        //save.NumberOfCubes = NumberOfCubes();
        //save.Progress = cubeGeneratorStateManager.GetCurrentProgress();
        //save.State = cubeGeneratorStateManager.GetCurrentStateIndex();
        //save.CurrentSpeedMode = CurrentSpeedMode();

        //newSaveData.Data.Add(save);

        QuickLogger.Debug($"Saves Connection Grid Extension {newSaveData.Data.Count}", true);
    }

    public override string[] GetDeviceStats()
    {
        var status = _isBaseManagerBuilt ? "Connected" : "Base Manager Not Found";
        return new string[]
        {
            $"[EPM: {GetPowerUsage() * 60:F2}] [Status: {status}]",
        };
    }

    private void ChangeState()
    {
        if(CachedHabitatManager == null)
        {
            FindBaseManager();
            SetState(FCSDeviceState.Off);
            return;
        }


        _isBaseManagerBuilt = CachedHabitatManager.HasDevice(BaseManagerBuildable.PatchedTechType);
        
        if (powerRelay?.powerStatus == PowerSystem.Status.Offline)
        {
            SetState(FCSDeviceState.NoPower);
            return;
        }

        if (!_isBaseManagerBuilt)
        {
            SetState(FCSDeviceState.NotConnected);
            return;
        }
        else
        {
            SetState(FCSDeviceState.Running);
        }
    }

    public override void Start()
    {
        base.Start();
        InvokeRepeating(nameof(ChangeState), 0f, 1f);
    }
}
