using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Model
{
    public class Grid2<T> : IGrid<T> where T : class
    {
        public int Size { get; private set; }

        private List<List<T>> grid = new List<List<T>>();

        public Vector2Int Center => new Vector2Int(
            Mathf.FloorToInt(Size / 2),
            Mathf.FloorToInt(Size / 2)
        );

        public Grid2(int size)
        {
            Size = size;
            InitialiseGrid();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="original"></param>
        /// <param name="size"></param>
        /// <param name="relativeToCenter"></param>
        public Grid2(Grid2<T> original, int size, bool relativeToCenter = true)
        {
            if (size < original.Size)
            {
                throw new ArgumentOutOfRangeException("Size cannot be smaller than the original Grid");
            }

            Size = size;
            InitialiseGrid();

            if (!relativeToCenter)
            {
                foreach (T item in original)
                {
                    Vector2Int position = original.Position(item);
                    Add(item, position);
                }
            }
            else
            {
                foreach (T item in original)
                {
                    Vector2Int originalPosition = original.Position(item);
                    Vector2Int shift = Center - original.Center;
                    Vector2Int position = originalPosition + shift;
                    Add(item, position);
                }
            }
        }

        protected static bool TEquals(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);
        protected static bool TIsNull(T value) => TEquals(value, null);

        protected virtual void InitialiseGrid()
        {
            for (int x = 0; x < Size; x++)
            {
                grid.Add(new List<T>());
                for (int y = 0; y < Size; y++)
                {
                    grid[x].Add(null);
                }
            }
        }

        public bool Add(T item, Vector2Int position)
        {
            if (!this.Contains(position))
            { // Position is invalid
                return false;
            }

            if (TIsNull(ElementAt(position)))
            { // Item can be added as the space is free
                grid[position.x][position.y] = item;
                return true;
            }
            else
            { // Item cannot be added at this position, the space is taken
                return false;
            }
        }

        public virtual bool Remove(T item)
        {
            Vector2Int position = Position(item);
            if (position.x < 0 || position.y < 0)
            { // Item is not in the grid
                return false;
            }

            grid[position.x][position.y] = null;
            return true;
        }

        public bool Contains(Vector2Int position) => !(position.x < 0 || position.x >= Size || position.y < 0 || position.y >= Size);
        public T ElementAt(Vector2Int position) => grid[position.x][position.y];

        protected class SearchResult
        {
            public int x = -1;
            public int y = -1;
            public Vector2Int position => new Vector2Int(x, y);
            public T item;
        }

        public Vector2Int Position(T item)
        {
            if (!this.Contains(item))
            {
                return new Vector2Int(-1, -1);
            }

            var search = grid.Select((xList, x) =>
            {
                var ySearch = xList.Select((yItem, y) => new SearchResult { y = TEquals(yItem, item) ? y : -1, item = yItem })
                    .FirstOrDefault(yList => TEquals(yList.item, item));
                if (ySearch != null)
                {
                    ySearch.x = x;
                    return ySearch;
                }
                return new SearchResult();
            }).FirstOrDefault(xList => TEquals(xList.item, item));
            return search?.position ?? new Vector2Int(-1, -1);
        }

        public IEnumerator<T> GetEnumerator() => grid.SelectMany(x => x).Where(x => !TIsNull(x)).GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Graph<T> ToGraph()
        {
            Graph<T> graph = new Graph<T>();

            foreach (var item in this)
            {
                graph.AddVertex(item);
                var position = Position(item);
                foreach (var direction in new Vector2Int[]
                {
                    Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right
                })
                {
                    var neighbourPosition = position + direction;
                    if (Contains(neighbourPosition))
                    {
                        var neighbour = ElementAt(neighbourPosition);
                        if (!TIsNull(neighbour))
                        {
                            graph.AddVertex(neighbour);
                            graph.AddEdge(item, neighbour, 1);
                        }
                    }
                }
            }

            return graph;
        }
    }
}