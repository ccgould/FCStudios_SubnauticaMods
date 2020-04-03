namespace FCSCommon.Interfaces
{
    internal interface IFCSController
    {
        bool IsInitialized { get; set; }
        void Initialize();
        void OnProtoSerialize(ProtobufSerializer serializer);
        void OnProtoDeserialize(ProtobufSerializer serializer);
        bool CanDeconstruct(out string reason);
        void OnConstructedChanged(bool constructed);
    }
}