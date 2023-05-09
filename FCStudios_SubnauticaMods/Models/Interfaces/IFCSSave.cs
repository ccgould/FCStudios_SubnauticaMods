namespace FCS_AlterraHub.Models.Interfaces;

public interface IFCSSave<T>
{
    void Save(T newSaveData, ProtobufSerializer serializer = null);
}
