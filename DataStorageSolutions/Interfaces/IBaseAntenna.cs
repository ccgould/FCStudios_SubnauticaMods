using DataStorageSolutions.Model;
using FCSCommon.Interfaces;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace DataStorageSolutions.Interfaces
{
    internal interface IBaseAntenna: IBaseUnit
    {
        ColorManager ColorManager { get; set; }
        BaseManager Manager { get; set; }
        bool IsVisible();
        void ChangeColorMask(Color color);
    }
}
