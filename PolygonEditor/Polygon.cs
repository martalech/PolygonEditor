using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    class Polygon
    {
        public List<Vertex> Vertices { get; set; }
        public List<Edge> Edges { get; set; }

        public Polygon()
        {
            Vertices = new List<Vertex>();
            Edges = new List<Edge>();
        }

        public bool HasEdge(Edge e)
        {
            foreach (var edge in Edges)
                if (edge == e)
                    return true;
            return false;
        }

        public Polygon(List<Vertex> vertices, List<Edge> edges)
        {
            Vertices = vertices;
            Edges = edges;
        }
    }
}