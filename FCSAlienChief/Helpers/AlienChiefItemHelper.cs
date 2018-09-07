using System;
using FCSAlienChief.Model;
using System.Collections.Generic;

namespace FCSAlienChief.Helpers
{
    public class AlienChiefItemHelper
    {
        /// <summary>
        /// Returns decoration item TechType giving the list of decoration items and the item's classID.
        /// </summary>
        public static TechType GetTechType(List<IAlienChiefItem> decorationItems, string classID)
        {
            foreach (var item in decorationItems)
            {
                if (string.Compare(item.ClassID_I, classID, StringComparison.Ordinal) == 0)
                    return item.TechType_I;
            }
            return 0;
        }
    }
}
