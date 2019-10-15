using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    class Vertex
    {
        public List<Edge> Edges { get; set; } //0 - in, 1 - out
        public Edge GetInEdge()
        {
            return Edges[0].From == this ? Edges[1] : Edges[0];
        }
        public Edge GetOutEdge()
        {
            return Edges[0].To == this ? Edges[1] : Edges[0];
        }
        public Point Coord
        {
            get
            {
                return coord;
            }
            set
            {
                coord = value;
            }
        }
        Point coord;
        public int X
        {
            get
            {
                return Coord.X;
            }
            set
            {
                coord.X = value;
            }
        }
        public int Y
        {
            get
            {
                return Coord.Y;
            }
            set
            {
                coord.Y = value;
            }
        }

        public Vertex()
        {
            Edges = new List<Edge>();
            Coord = new Point();
        }

        public Vertex(int X, int Y)
        {
            Coord = new Point(X, Y);
            Edges = new List<Edge>();
        }

        public bool IsEmpty()
        {
            return Coord.IsEmpty;
        }
    }
}
