using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCS_AlterraHub.Models.Enumerators;
/// <summary>
/// The power states for the FCS Power Storage
/// </summary>
public enum FCSPowerStates
{
    None = 0,
    Powered = 1,
    UnPowered = 2,
    Tripped = 3
}
