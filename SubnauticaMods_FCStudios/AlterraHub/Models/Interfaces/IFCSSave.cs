using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCS_AlterraHub.Models.Interfaces
{
    public interface IFCSSave<T>
    {
        void Save(T newSaveData, ProtobufSerializer serializer = null);
    }
}
