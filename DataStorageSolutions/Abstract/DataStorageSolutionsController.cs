using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using FCSTechFabricator.Abstract;


namespace DataStorageSolutions.Abstract
{
    internal abstract class DataStorageSolutionsController : FCSController, IDataSoluationsSave
    {
        public abstract void Save(SaveData save);
    }
}
