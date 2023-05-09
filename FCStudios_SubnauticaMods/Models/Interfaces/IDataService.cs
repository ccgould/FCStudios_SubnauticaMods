using System;

namespace FCS_AlterraHub.Models.Interfaces;

public interface IDataService
{
    bool SaveData<T>(string modName, T Data, bool Encrypted, Action callBack);
    void LoadData<T>(string modName, bool Encrypted, Action<T> callBack);
}
