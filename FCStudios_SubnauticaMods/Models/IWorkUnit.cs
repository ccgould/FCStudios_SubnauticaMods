namespace FCS_AlterraHub.Models;
public interface IWorkUnit
{

    void SyncDevice(object device);
    void TurnOffDevice();
    void TurnOnDevice();
    string GetPrefabID();
    
    //void SetSettings(object settings);
    //void DoWork();
    //void StopWork();
    //void SetSpeed(FCSDeviceSpeedModes speed);
    //FCSDeviceSpeedModes GetSpeed();
    //void SetState(FCSDeviceState state);
    //FCSDeviceState GetState();
}
