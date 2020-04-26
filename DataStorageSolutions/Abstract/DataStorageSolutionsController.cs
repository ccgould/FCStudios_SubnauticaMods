using DataStorageSolutions.Configuration;
using FCSTechFabricator.Abstract;


namespace DataStorageSolutions.Abstract
{
    internal abstract class DataStorageSolutionsController : FCSController
    {
        public abstract void Save(SaveData save);
    }
}
