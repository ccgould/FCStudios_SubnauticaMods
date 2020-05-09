using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Model;

namespace DataStorageSolutions.Structs
{
    internal struct ButtonData
    {
        internal BaseManager Manager { get; set; }
        internal Vehicle Vehicle { get; set; }
        internal TechType TechType { get; set; }
        internal int Amount { get; set; }
    }
}
