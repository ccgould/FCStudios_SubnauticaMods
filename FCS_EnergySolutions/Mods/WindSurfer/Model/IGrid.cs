using System.Collections.Generic;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Model
{
    public interface IGrid<T> : IEnumerable<T>
    {
        int Size { get; }
        Vector2Int Center { get; }

        bool Add(T item, Vector2Int position);
        bool Remove(T item);
        bool Contains(Vector2Int position);
        T ElementAt(Vector2Int position);
        Vector2Int Position(T item);
    }
}