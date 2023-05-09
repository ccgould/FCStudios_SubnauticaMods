using System.Collections.Generic;


namespace FCS_AlterraHub.Core.Extensions;

public static  class CollectionExtentions
{
    public static string GetValueAndRemove<TKey, TValue>(this Dictionary<int, string> dict, int key)
    {
        string val = dict[key];
        dict.Remove(key);
        return val;
    }
}
