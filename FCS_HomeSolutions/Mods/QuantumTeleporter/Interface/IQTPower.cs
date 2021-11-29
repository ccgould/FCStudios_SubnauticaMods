using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Interface
{
    internal interface IQTPower
    {
        bool TakePower(QTTeleportTypes tab);
        bool HasEnoughPower(QTTeleportTypes selectedTab);
        float PowerAvailable();
        void FullReCharge();
        void SetCharge(float charge);
        void ModifyCharge(float amount);
        bool IsFull();
    }
}