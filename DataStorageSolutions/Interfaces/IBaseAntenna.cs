using DataStorageSolutions.Model;
using DataStorageSolutions.Mono;
using FCSCommon.Interfaces;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace DataStorageSolutions.Interfaces
{
    internal interface IBaseAntenna
    {
        ColorManager ColorManager { get; set; }
        BaseManager Manager { get; set; }
        void ChangeColorMask(Color color);
        bool IsVisible();
    }
}
