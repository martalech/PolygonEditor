using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    public enum EdgeType { In, Out }
    class Edge
    {
        public Vertex From { get; set; }
        public Vertex To { get; set; }
        public Edge InRelation { get; set; }
        public Relation Relation { get; set; }
        public Edge(Vertex from, Vertex to, Edge inRelation = null)
        {
            Relation = Relation.None;
            InRelation = inRelation;
            from.Edges.Add(this);
            to.Edges.Add(this);
            From = from;
            To = to;
        }
        public Edge(int x1, int y1, int x2, int y2)
        {
            From = new Vertex(x1, y1);
            To = new Vertex(x2, y2);
        }
    }
}
