using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSTechFabricator.Abstract;
using Steamworks;


namespace DataStorageSolutions.Abstract
{
    internal abstract class DataStorageSolutionsController : FCSController, IDataSoluationsSave
    {
        public virtual SubRoot SubRoot { get; set; }
        public abstract void Save(SaveData save);
        public abstract BaseManager Manager { get; set; }
    }
}
