using DataStorageSolutions.Model;
using FCSCommon.Interfaces;
using FCSTechFabricator.Managers;

namespace DataStorageSolutions.Interfaces
{
    internal interface IBaseAntenna: IBaseUnit
    {
        ColorManager ColorManager { get; set; }
        BaseManager Manager { get; set; }
        string GetName();
        void ChangeBaseName();
        bool IsVisible();
    }
}
