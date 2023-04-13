using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using System.Collections.Generic;

namespace FCS_AlterraHub.Models.Interfaces
{
    internal class EncyclopediaComparer : IComparer<EncyclopediaEntryData>
    {
        public int Compare(EncyclopediaEntryData x, EncyclopediaEntryData y)
        {
            return x.Title.CompareTo(y.Title);
        }
    }
}
