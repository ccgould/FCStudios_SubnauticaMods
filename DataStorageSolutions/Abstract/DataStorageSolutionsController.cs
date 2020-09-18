using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using FCSTechFabricator.Abstract;


namespace DataStorageSolutions.Abstract
{
    internal abstract class DataStorageSolutionsController : FCSController, IDataSoluationsSave
    {
        public abstract void Save(SaveData save);
        public abstract BaseManager Manager { get; set; }
    }
}
