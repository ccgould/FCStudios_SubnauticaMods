using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FCS_EnergySolutions.Mods.WindSurfer.Model
{
    public class Graph<T> : IEnumerable<T> where T : class
    {
        public class Vertex
        {
            public List<Edge> Neighbours = new List<Edge>();
            public T Item;

            public Vertex(T item) => Item = item;
            public override string ToString() => $"Vertex(Item: {Item}, Neighbours: {Neighbours})";
        }

        public class Edge
        {
            public Vertex From { get; private set; }
            public Vertex To { get; private set; }
            public int Weight { get; private set; }

            public Edge(Vertex from, Vertex to, int weight)
            {
                From = from;
                To = to;
                Weight = weight;
            }

            public override string ToString() => $"Edge(From: {From}, To: {To}, Weight: {Weight})";
        }

        public List<Vertex> Vertices = new List<Vertex>();

        public void AddVertex(T item)
        {
            if (!Vertices.Any(v => TEquals(v.Item, item)))
            {
                Vertices.Add(new Vertex(item));
            }
        }
        public Vertex GetVertex(T item) => Vertices.SingleOrDefault(x => TEquals(x.Item, item));
        public void RemoveVertex(T item)
        {
            Vertex vertex = GetVertex(item);
            if (vertex != null)
            {
                foreach (var neighbour in vertex.Neighbours.Select(n => GetVertex(n.To.Item)))
                {
                    neighbour.Neighbours.RemoveAll(edge => edge.To == vertex);
                }
                Vertices.Remove(vertex);
            }
        }

        protected static bool TEquals(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);

        public void AddEdge(T from, T to, int weight)
        {
            Vertex vFrom = GetVertex(from);
            Vertex vTo = GetVertex(to);

            if (!vFrom.Neighbours.Any(e => e.From == vFrom && e.To == vTo))
            {
                vFrom.Neighbours.Add(new Edge(vFrom, vTo, weight));
            }

            if (!TEquals(from, to) && !vTo.Neighbours.Any(e => e.From == vTo && e.To == vFrom))
            {
                vTo.Neighbours.Add(new Edge(vTo, vFrom, weight));
            }
        }

        public Edge GetEdge(T from, T to)
        {
            Vertex vFrom = GetVertex(from);
            Vertex vTo = GetVertex(to);

            foreach (Edge e in vFrom.Neighbours)
            {
                if (e.From == vFrom && e.To == vTo)
                {
                    return e;
                }
            }

            return null;
        }

        /// <summary>
        /// Minimum spanning tree
        /// </summary>
        /// <returns></returns>
        public Graph<T> MST(Vertex vertex = null)
        {
            Graph<T> MST = new Graph<T>();

            if (Vertices.Any())
            {
                T item = vertex == null ? Vertices.ElementAt(0).Item : vertex.Item;
                MST.AddVertex(item);

                while (MST.Vertices.Count() < Vertices.Count())
                {
                    // Find the set of links L connecting one node in the tree with a node not in the tree
                    List<Edge> L = new List<Edge>();

                    foreach (Edge edge in Vertices.SelectMany(v => v.Neighbours))
                    {
                        if (MST.GetVertex(edge.From.Item) != null &&
                            GetVertex(edge.To.Item) != null &&
                            MST.GetVertex(edge.To.Item) == null)
                        {
                            L.Add(edge);
                        }
                    }

                    // Find, in L, the minimum weight edge
                    Edge minimumWeightEdge = null;
                    foreach (Edge edge in L)
                    {
                        if (minimumWeightEdge == null || edge.Weight < minimumWeightEdge.Weight)
                        {
                            minimumWeightEdge = edge;
                        }
                    }

                    if (minimumWeightEdge != null)
                    { // Add that edge, and the new vertex, to the spanning tree
                        MST.AddVertex(minimumWeightEdge.To.Item);
                        MST.AddEdge(minimumWeightEdge.From.Item, minimumWeightEdge.To.Item, minimumWeightEdge.Weight);
                    }

                    if (minimumWeightEdge == null)
                    {
                        break;
                    }
                }
            }

            return MST;
        }

        /// <summary>
        /// Cost of the minimum spanning tree
        /// </summary>
        /// <returns></returns>
        public int MSTCost() => MST().Vertices.SelectMany(v => v.Neighbours).Sum(e => e.Weight) / 2;

        public IEnumerator<T> GetEnumerator() => Vertices.Select(v => v.Item).GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}