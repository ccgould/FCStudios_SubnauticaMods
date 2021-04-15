using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Mono;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class AlienChefStorage : FCSStorage
    {
        public override bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return false;
        }
    }
}
