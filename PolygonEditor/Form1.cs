using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PolygonEditor
{
    enum MenuOption { MoveComponent, AddVertex, DeleteVertex, MovePolygon, RemovePolygon, HalveEdge,
        AddRelation, RemoveRelation }
    enum Relation { None, Equality, Perpendicular }

    public partial class PolygonEditor : Form
    {
        MenuOption menuOption;
        Relation relation;

        List<Edge> edges = new List<Edge>();
        List<Vertex> vertices = new List<Vertex>();
        List<Polygon> polygons = new List<Polygon>();
        List<(Edge e1, Edge e2)> edgesInRelation = new List<(Edge, Edge)>();
        List<Edge> clickedEdges = new List<Edge>();
        HashSet<Edge> correctedEdges = new HashSet<Edge>();

        Vertex movingVertex = null;
        Edge movingEdge = null;
        Polygon movingPolygon = null;
        Vertex mouse = new Vertex();

        bool mouseDown = false;

        public PolygonEditor()
        {
            InitializeComponent();
            List<Vertex> predefinedVertices = new List<Vertex>(new Vertex[]
            {
                new Vertex(268, 223),
                new Vertex(313, 340),
                new Vertex(442, 340),
                new Vertex(489, 221),
                new Vertex(380, 112),
            });
            List<Edge> predefinedEdges = new List<Edge>();
            for (int i = 0; i < 5; i++)
            {
                predefinedEdges.Add(new Edge(predefinedVertices[i], predefinedVertices[(i + 1) % 5]));
            }
            edgesInRelation.Add((predefinedEdges[0], predefinedEdges[2]));
            edgesInRelation.Add((predefinedEdges[3], predefinedEdges[4]));
            EqualEdges(predefinedEdges[0], predefinedEdges[2], predefinedEdges[0].From);
            PerpendiculateEdges(predefinedEdges[3], predefinedEdges[4], predefinedEdges[3].From);
            polygons.Add(new Polygon(predefinedVertices, predefinedEdges));
            RepaintPolygon();
        }

        private void OnMoveComponentMenuItemClick(object sender, EventArgs e)
        {
            IsLastPolygonCorrect();
            vertices = new List<Vertex>();
            edges = new List<Edge>();
            polygons.Add(new Polygon(vertices, edges));
            menuOption = MenuOption.AddVertex;
        }

        private void OnAddVertexMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.MoveComponent;
        }

        private void OnRemoveVertexMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.DeleteVertex;
        }

        private void OnMovePolygonMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.MovePolygon;
        }

        private void OnRemovePolygonMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.RemovePolygon;
        }

        private void OnHalveEdgeMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.HalveEdge;
        }

        private void OnEqualEdgesMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            clickedEdges = new List<Edge>();
            menuOption = MenuOption.AddRelation;
            relation = Relation.Equality;
        }

        private void OnPerpendiculateEdgesMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            clickedEdges = new List<Edge>();
            menuOption = MenuOption.AddRelation;
            relation = Relation.Perpendicular;
        }

        private void OnRemoveRelationMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.RemoveRelation;
        }

        private void AddVertex(Vertex e)
        {
            if (FindVertex(e).vertex != null)
            {
                MessageBox.Show("Cannot place vertex on another vertex!");
                return;
            }
            vertices.Add(new Vertex(e.Coord.X, e.Y));
            if (vertices.Count > 1)
            {
                Vertex from = vertices[vertices.Count - 2];
                Vertex to = vertices[vertices.Count - 1];
                edges.Add(new Edge(from, to));
            }
            if (vertices.Count > 2)
            {
                Vertex to = vertices[0];
                Vertex from = vertices[vertices.Count - 1];
                if (vertices.Count > 3)
                {
                    Edge edge = edges[edges.Count - 2];
                    edge.From.Edges.Remove(edge);
                    edge.To.Edges.Remove(edge);
                    edges.Remove(edge);
                }
                edges.Add(new Edge(from, to));
            }
            RepaintPolygon();
        }

        private void RemoveVertex(Vertex soughtVertex)
        {
            (Vertex vertexToRemove, Polygon polygon) = FindVertex(soughtVertex);
            if (vertexToRemove == null)
                return;
            if (polygon.Vertices.Count == 3)
            {
                MessageBox.Show("A polygon has to have at least 3 vertices!");
                return;
            }
            Vertex to = new Vertex(), from = new Vertex();
            List<(List<Edge> removeFrom, Edge toRemove)> list = new List<(List<Edge> removeFrom,
                Edge toRemove)>();
            foreach (var edge in vertexToRemove.Edges)
            {
                if (edge.From == vertexToRemove)
                {
                    if (edge.Relation != Relation.None)
                        RemoveRelation(edge);
                    list.Add((edge.To.Edges, edge));
                    to = edge.To;
                    polygon.Edges.Remove(edge);
                }
                else if (edge.To == vertexToRemove)
                {
                    if (edge.Relation != Relation.None)
                        RemoveRelation(edge);
                    list.Add((edge.From.Edges, edge));
                    polygon.Edges.Remove(edge);
                    from = edge.From;
                }
            }
            foreach ((List<Edge> removeFrom, Edge toRemove) in list)
            {
                removeFrom.Remove(toRemove);
            }
            edges.Add(new Edge(from, to));
            polygon.Vertices.Remove(vertexToRemove);
            RepaintPolygon();
        }

        private void RemovePolygon(Polygon polygon)
        {
            polygons.Remove(polygon);
            foreach(var edge in polygon.Edges)
            {
                if (edge.Relation != Relation.None)
                    RemoveRelation(edge);
            }
            RepaintPolygon();
        }

        private void RemoveRelation(Edge edge)
        {
            if (!edgesInRelation.Remove((edge, edge.InRelation)))
                edgesInRelation.Remove((edge.InRelation, edge));
            edge.InRelation.Relation = Relation.None;
            edge.Relation = Relation.None;
            edge.InRelation.InRelation = null;
            edge.InRelation = null;
        }

        private void EqualEdges(Edge edge1, Edge edge2, Vertex staticPoint = null)
        {
            if (AreEqual(edge1, edge2))
                return;
            edge1.Relation = Relation.Equality;
            edge2.Relation = Relation.Equality;
            edge1.InRelation = edge2;
            edge2.InRelation = edge1;
            int e1Length = CalculateLength(edge1);
            int e2Length = CalculateLength(edge2);
            int diff = Math.Abs(e1Length - e2Length);
            if (diff > 2)
            {
                int r = e2Length;
                int x0 = staticPoint.X;
                int y0 = staticPoint.Y;
                Vertex movingPoint = staticPoint.Coord == edge1.From.Coord ? edge1.To : edge1.From;
                double a, b, delta;
                double a1, b1, c1, x1, y1, x2, y2;
                if (edge1.To.X != edge1.From.X)
                {
                    a = (double)(staticPoint.Y - movingPoint.Y) / (double)(staticPoint.X - movingPoint.X);
                    b = movingPoint.Y - a * movingPoint.X;
                    a1 = a * a + 1;
                    b1 = 2 * (a * b - x0 - a * y0);
                    c1 = x0 * x0 + b * b - 2 * b * y0 - r * r + y0 * y0;
                    delta = b1 * b1 - 4 * a1 * c1;
                    x1 = ((-1) * b1 - Math.Sqrt(delta)) / (2 * a1);
                    x2 = ((-1) * b1 + Math.Sqrt(delta)) / (2 * a1);
                    y1 = a * x1 + b;
                    y2 = a * x2 + b;
                }
                else
                {
                    int x = edge1.From.X;
                    a1 = 1;
                    b1 = (-2) * y0;
                    c1 = x * x - 2 * x * x0 + x0 * x0 + y0 * y0 - r * r;
                    delta = b1 * b1 - 4 * a1 * c1;
                    y1 = ((-1) * b1 - Math.Sqrt(delta)) / (2 * a1);
                    y2 = ((-1) * b1 + Math.Sqrt(delta)) / (2 * a1);
                    x1 = x2 = x;
                }
                int length1 = CalculateLength(new Edge(new Vertex((int)x1, (int)y1),
                    new Vertex(movingPoint.X, movingPoint.Y)));
                int length2 = CalculateLength(new Edge(new Vertex((int)x2, (int)y2),
                    new Vertex(movingPoint.X, movingPoint.Y)));
                    movingPoint.Coord = length1 < length2 ? new Point((int)x1, (int)y1) :
                        new Point((int)x2, (int)y2);
            }
            RepaintPolygon();
        }

        private void PerpendiculateEdges(Edge edge1, Edge edge2, Vertex staticPoint)
        {
            if (ArePerpendicular(edge1, edge2))
                return;
            double x1 = edge2.From.X;
            double y1 = edge2.From.Y;
            double x2 = edge2.To.X;
            double y2 = edge2.To.Y;
            Vertex movingPoint = edge1.From.Coord == staticPoint.Coord ? edge1.To : edge1.From;
            double a = (double)(y2 - y1) / (double)(x2 - x1);
            double b = y1 - a * x1;
            //a = (-1) * (1 / a);
            //b = staticPoint.Y - a * staticPoint.X;
            edge1.Relation = Relation.Perpendicular;
            edge2.Relation = Relation.Perpendicular;
            edge1.InRelation = edge2;
            edge2.InRelation = edge1;
            int e1Length = CalculateLength(edge1);
            int e2Length = CalculateLength(edge2);
            int r = e1Length;
            int x0 = staticPoint.X;
            int y0 = staticPoint.Y;
            double delta;
            double a1, b1, c1;
            if (edge2.To.X == edge2.From.X)
            {
                a1 = 1;
                b1 = (-2) * x0;
                c1 = x0 * x0 - r * r;
                delta = b1 * b1 - 4 * a1 * c1;
                x1 = ((-1) * b1 - Math.Sqrt(delta)) / (2 * a1);
                x2 = ((-1) * b1 + Math.Sqrt(delta)) / (2 * a1);
                y1 = y2 = y0;
            }
            else if (edge2.To.Y == edge2.From.Y)
            {
                a1 = 1;
                b1 = (-2) * y0;
                c1 = y0 * y0 - r * r;
                delta = b1 * b1 - 4 * a1 * c1;
                y1 = ((-1) * b1 - Math.Sqrt(delta)) / (2 * a1);
                y2 = ((-1) * b1 + Math.Sqrt(delta)) / (2 * a1);
                x1 = x2 = x0;
            }
            else
            {
                a = (-1) * (1 / a);
                b = staticPoint.Y - a * staticPoint.X;
                a1 = a * a + 1;
                b1 = 2 * (a * b - x0 - a * y0);
                c1 = x0 * x0 + b * b - 2 * b * y0 - r * r + y0 * y0;
                delta = b1 * b1 - 4 * a1 * c1;
                x1 = ((-1) * b1 - Math.Sqrt(delta)) / (2 * a1);
                x2 = ((-1) * b1 + Math.Sqrt(delta)) / (2 * a1);
                y1 = a * x1 + b;
                y2 = a * x2 + b;
            }
            //if (Double.IsNaN(x1) || Double.IsNaN(y1) || Double.IsNaN(x2) || Double.IsNaN(y2))
            //{
            //    int elo = 4;
            //}
            //Debug.WriteLine($"movpoint1({x1}, {y1})->({x0},{y0})");
            //Debug.WriteLine($"movpoint1({x2}, {y2})->({x0},{y0})");
            int length1 = CalculateLength(new Edge(new Vertex((int)x1, (int)y1),
            new Vertex(movingPoint.X, movingPoint.Y)));
            int length2 = CalculateLength(new Edge(new Vertex((int)x2, (int)y2),
                new Vertex(movingPoint.X, movingPoint.Y)));
            //if (x1 < 0 || y1 < 0)
            //    movingPoint.Coord = new Point((int)x2, (int)y2);
            //else if (x2 < 0 || y2 < 0)
            //    movingPoint.Coord = new Point((int)x1, (int)y1);
            //else
                movingPoint.Coord = length1 < length2 ? new Point((int)x1, (int)y1) :
                    new Point((int)x2, (int)y2);
            RepaintPolygon();
        }

        private void HalveEdge(Polygon polygon, Edge edge, int i)
        {
            int x = (Math.Max(edge.From.Coord.X, edge.To.Coord.X) + Math.Min(edge.From.Coord.X,
                edge.To.Coord.X)) / 2;
            int y = (Math.Max(edge.From.Y, edge.To.Y) + Math.Min(edge.From.Y,
                edge.To.Y)) / 2;
            Vertex newVertex = new Vertex(x, y);
            if (edge.Relation != Relation.None)
                RemoveRelation(edge);
            polygon.Edges.Remove(edge);
            edge.From.Edges.Remove(edge);
            edge.To.Edges.Remove(edge);
                polygon.Edges.Insert(i, new Edge(edge.From, newVertex));
                polygon.Edges.Insert(i + 1, new Edge(newVertex, edge.To));
                if (edge.To == polygon.Vertices[0])
                {
                    polygon.Vertices.Add(newVertex);
                }
                else
                {
                    int j1 = polygon.Vertices.FindIndex(v => v == edge.From);
                    int j2 = polygon.Vertices.FindIndex(v => v == edge.To);
                    polygon.Vertices.Insert(Math.Min(j1, j2) + 1, newVertex);
                }
            RepaintPolygon();
        }

        //private void CorrectRight(Edge e)
        //{
        //    var iter = e;
        //    while(iter.Relation != Relation.None && iter.To != e.From)
        //    {
        //        if (correctedEdges.Add(iter))
        //        {
        //            Debug.WriteLine($"CR: ({e.From.X},{e.From.Y})->({e.To.X},{e.To.Y})");
        //            Debug.WriteLine($"MR: ({iter.InRelation.From.X},{iter.InRelation.From.Y})");
        //            if (iter.InRelation.To != e.From && iter.InRelation.To != e.To)
        //            {
        //                if (iter.Relation == Relation.Equality)
        //                    EqualEdges(iter.InRelation, iter, iter.InRelation.To);
        //                else
        //                    PerpendiculateEdges(iter.InRelation, iter, iter.InRelation.To);
        //            }
        //            else
        //            {
        //                if (iter.Relation == Relation.Equality)
        //                    EqualEdges(iter.InRelation, iter, iter.InRelation.From);
        //                else
        //                    PerpendiculateEdges(iter.InRelation, iter, iter.InRelation.From);
        //            }
        //        }
        //        iter = iter.To.Edges[0] == iter ? iter.To.Edges[1] : iter.To.Edges[0];
        //    }
        //}

        //private void CorrectLeft(Edge e)
        //{
        //    var iter = e;
        //    while (iter.Relation != Relation.None && iter.From != e.To)
        //    {
        //        if (correctedEdges.Add(iter))
        //        {
        //            Debug.WriteLine($"CL: ({e.From.X},{e.From.Y})->({e.To.X},{e.To.Y})");
        //            Debug.WriteLine($"ML: ({iter.InRelation.To.X},{iter.InRelation.To.Y})");
        //            if (iter.InRelation.To != e.From && iter.InRelation.To != e.To)
        //            {
        //                if (iter.Relation == Relation.Equality)
        //                    EqualEdges(iter.InRelation, iter, iter.InRelation.From);
        //                else
        //                    PerpendiculateEdges(iter.InRelation, iter, iter.InRelation.From);
        //            }
        //            else
        //            {
        //                if (iter.Relation == Relation.Equality)
        //                    EqualEdges(iter.InRelation, iter, iter.InRelation.To);
        //                else
        //                    PerpendiculateEdges(iter.InRelation, iter, iter.InRelation.To);
        //            }
        //        }
        //        iter = iter.From.Edges[0] == iter ? iter.From.Edges[1] : iter.From.Edges[0];
        //    }
        //}

        private (Vertex vertex, Polygon polygon) FindVertex(Vertex soughtVertex)
        {
            foreach (var polygon in polygons)
            {
                foreach (var vertex in polygon.Vertices)
                {
                    if (BelongsToCircle(soughtVertex, vertex))
                        return (vertex, polygon);
                }
            }
            return (null, null);
        }

        private (Edge edge, Polygon polygon) FindEdge(Vertex soughtEdge)
        {
            foreach (var polygon in polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    if (BelongsToSegment(soughtEdge, edge))
                        return (edge, polygon);
                }
            }
            return (null, null);
        }

        private (Edge edge, Polygon polygon, int i) FindEdgeWithIndex(Vertex soughtEdge)
        {
            foreach (var polygon in polygons)
            {
                int i = 0;
                foreach (var edge in polygon.Edges)
                {
                    if (BelongsToSegment(soughtEdge, edge))
                        return (edge, polygon, i);
                    i++;
                }
            }
            return (null, null, -1);
        }

        private Point? GetCircleLineIntersect(int x0, int y0, int r, Edge line, Vertex staticPoint)
        {
            Vertex movingPoint = staticPoint.Coord == line.From.Coord ? line.To : line.From;
            double a, b, delta;
            double a1, b1, c1, x1, y1, x2, y2;
            if (line.To.X != line.From.X)
            {
                a = (double)(staticPoint.Y - movingPoint.Y) / (double)(staticPoint.X - movingPoint.X);
                b = movingPoint.Y - a * movingPoint.X;
                a1 = a * a + 1;
                b1 = 2 * (a * b - x0 - a * y0);
                c1 = x0 * x0 + b * b - 2 * b * y0 - r * r + y0 * y0;
                delta = b1 * b1 - 4 * a1 * c1;
                if (delta < 0)
                    return null;
                x1 = ((-1) * b1 - Math.Sqrt(delta)) / (2 * a1);
                x2 = ((-1) * b1 + Math.Sqrt(delta)) / (2 * a1);
                y1 = a * x1 + b;
                y2 = a * x2 + b;
            }
            else
            {
                int x = line.From.X;
                a1 = 1;
                b1 = (-2) * y0;
                c1 = x * x - 2 * x * x0 + x0 * x0 + y0 * y0 - r * r;
                delta = b1 * b1 - 4 * a1 * c1;
                if (delta < 0)
                    return null;
                y1 = ((-1) * b1 - Math.Sqrt(delta)) / (2 * a1);
                y2 = ((-1) * b1 + Math.Sqrt(delta)) / (2 * a1);
                x1 = x2 = x;
            }
            int length1 = CalculateLength(new Edge(new Vertex((int)x1, (int)y1),
            new Vertex(movingPoint.X, movingPoint.Y)));
            int length2 = CalculateLength(new Edge(new Vertex((int)x2, (int)y2),
                new Vertex(movingPoint.X, movingPoint.Y)));
            return length1 < length2 ? new Point((int)x1, (int)y1) :
                new Point((int)x2, (int)y2);
        }

        private (Point p1, Point p2)? GetCirclesIntersect(int x1, int y1, int r1, int x2, int y2, int r2)
        {
            int d = CalculateLength(new Edge(x1, y1, x2, y2));
            if (r1 + r2 < d)
                return null;
            double a = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
            double h = Math.Sqrt(r1 * r1 - a * a);
            Point p = new Point((int)(x1 + (a / d) * (x2 - x1)), (int)(y1 + (a / d) * (y2 - y1)));
            Point inters1 = new Point((int)(p.X + (h / d) * (y2 - y1)), (int)(p.Y - (h / d) * (x2 - x1)));
            Point inters2 = new Point((int)(p.X - (h / d) * (y2 - y1)), (int)(p.Y + (h / d) * (x2 - x1)));
            if (inters1.X < 0 || inters2.X < 0 || inters2.Y < 0 || inters1.Y < 0)
                return null;
            return (inters1, inters2);
        }

        private void CorrectClockwise(Edge e)
        {
            var edge = e;
            int iter = 0;
            while (edge.Relation != Relation.None && iter < edges.Count)
            {
                if (edge.Relation == Relation.Equality && AreEqual(edge, edge.InRelation))
                    break;
                else if (edge.Relation == Relation.Perpendicular && ArePerpendicular(edge, edge.InRelation))
                    break;
                var edgein = edge.From.GetInEdge().To.GetInEdge();
                if (edge.Relation == Relation.Equality)
                {
                    Debug.WriteLine($"edgeC{iter}: ({edge.From.X},{edge.From.Y})->({edge.To.X},{edge.To.Y})");
                    Debug.WriteLine($"inC{iter}: ({edgein.From.X},{edgein.From.Y})->({edgein.To.X},{edgein.To.Y})");
                    if (edgein.Relation == Relation.Equality)
                    {
                        EqualEdges(edge, edge.InRelation, edge.To);
                    }
                    else if (edgein.Relation == Relation.None)
                    {
                        //(Point p1, Point p2)? points;
                        //points = GetCirclesIntersect(edge.To.X, edge.To.Y, old,
                        //        edgein.From.X, edgein.From.Y, oldin);
                        //old = CalculateLength(edgein);
                        //oldin = CalculateLength(edgein.From.GetInEdge());
                        //if (points == null)
                        //{
                            EqualEdges(edge, edge.InRelation, edge.To);
                        //}
                        //else
                        //{
                        //    var valuePoints = points.Value;
                        //    if (CalculateLength(new Edge(valuePoints.p1.X, valuePoints.p1.Y, edge.From.X, edge.From.Y))
                        //        < CalculateLength(new Edge(valuePoints.p2.X, valuePoints.p2.Y, edge.From.X, edge.From.Y)))
                        //        edge.From.Coord = valuePoints.p1;
                        //    else
                        //        edge.From.Coord = valuePoints.p2;
                        //    Debug.WriteLine($"p1 ({valuePoints.p1.X}, {valuePoints.p1.Y})");
                        //    Debug.WriteLine($"p2 ({valuePoints.p2.X}, {valuePoints.p2.Y})");
                        //    Debug.WriteLine($"obracanko punktem ({edge.From.X}, {edge.From.Y})");
                        //}
                    }
                    else if (edgein.Relation == Relation.Perpendicular)
                    {
                        //Point? p = GetCircleLineIntersect(edge.To.X, edge.To.Y, CalculateLength(edge), edgein, edgein.From);
                        //if (p.HasValue)
                        //{
                        //    edgein.To.X = p.Value.X;
                        //    edgein.To.Y = p.Value.Y;
                        //}
                        EqualEdges(edge, edge.InRelation, edge.To);
                        PerpendiculateEdges(edgein, edgein.InRelation, edgein.To);
                    }
                }
                else if (edge.Relation == Relation.Perpendicular)
                {
                    //if (edgein.Relation == Relation.None)
                    //{
                    //    old = CalculateLength(edgein);
                    //    oldin = CalculateLength(edgein.From.GetInEdge());
                    //    PerpendiculateEdges(edge, edge.InRelation, edge.To);
                    //}
                    //else if (edgein.Relation == Relation.Equality)
                    //{
                    //    Point? p = GetCircleLineIntersect(edgein.To.X, edgein.To.Y, CalculateLength(edge), edgein, edgein.From);
                    //    if (p.HasValue)
                    //    {
                    //        edgein.To.X = p.Value.X;
                    //        edgein.To.Y = p.Value.Y;
                    //    }
                    //}
                    if (edgein.Relation == Relation.None || edgein.Relation == Relation.Perpendicular)
                        PerpendiculateEdges(edge, edge.InRelation, edge.To);
                    else
                    {
                        PerpendiculateEdges(edge, edge.InRelation, edge.To);
                        EqualEdges(edgein, edgein.InRelation, edgein.To);
                    }
                }
                edge = edgein;
                iter++;
            }
        }

        private void CorrectCounterclockwise(Edge e)
        {
            var edge = e;
            int iter = 0;
            while (edge.Relation != Relation.None && iter < edges.Count)
            {
                if (edge.Relation == Relation.Equality && AreEqual(edge, edge.InRelation))
                    break;
                else if (edge.Relation == Relation.Perpendicular && ArePerpendicular(edge, edge.InRelation))
                    break;
                var edgeout = edge.From.GetOutEdge().To.GetOutEdge();
                if (edge.Relation == Relation.Equality)
                {
                    Debug.WriteLine($"edgeCC{iter}: ({edge.From.X},{edge.From.Y})->({edge.To.X},{edge.To.Y})");
                    Debug.WriteLine($"outCC{iter}: ({edgeout.From.X},{edgeout.From.Y})->({edgeout.To.X},{edgeout.To.Y})");
                    if (edgeout.Relation == Relation.None)
                    {
                        EqualEdges(edge, edge.InRelation, edge.From);
                    }
                    else if (edgeout.Relation == Relation.Equality)
                    {
                        //(Point p1, Point p2)? points;
                        //points = GetCirclesIntersect(edge.From.X, edge.From.Y, old,
                        //        edgeout.To.X, edgeout.To.Y, oldOut);
                        //old = CalculateLength(edgeout);
                        //oldOut = CalculateLength(edgeout.To.GetOutEdge());
                        //if (points == null)
                        //{
                            EqualEdges(edge, edge.InRelation, edge.From);
                        //}
                        //else
                        //{
                        //    var valuePoints = points.Value;
                        //    if (CalculateLength(new Edge(valuePoints.p1.X, valuePoints.p1.Y, edge.To.X, edge.To.Y))
                        //        < CalculateLength(new Edge(valuePoints.p2.X, valuePoints.p2.Y, edge.To.X, edge.To.Y)))
                        //        edge.To.Coord = valuePoints.p1;
                        //    else
                        //        edge.To.Coord = valuePoints.p2;
                        //    Debug.WriteLine($"p1 ({valuePoints.p1.X}, {valuePoints.p1.Y})");
                        //    Debug.WriteLine($"p2 ({valuePoints.p2.X}, {valuePoints.p2.Y})");
                        //    Debug.WriteLine($"obracanko punktem ({edge.To.X}, {edge.To.Y})");
                        //}
                    }
                    else if (edgeout.Relation == Relation.Perpendicular)
                    {
                        EqualEdges(edge, edge.InRelation, edge.From);
                        PerpendiculateEdges(edgeout, edgeout.InRelation, edgeout.From);
                    }
                }
                else if (edge.Relation == Relation.Perpendicular)
                {
                    if (edgeout.Relation == Relation.None || edgeout.Relation == Relation.Perpendicular)
                    {
                        PerpendiculateEdges(edge, edge.InRelation, edge.From);
                    }
                    else
                    {
                        PerpendiculateEdges(edge, edge.InRelation, edge.From);
                        EqualEdges(edgeout, edgeout.InRelation, edgeout.From);
                    }
                }
                edge = edgeout;
                iter++;
            }
        }

        private void MoveVertex(Vertex vertex, int x, int y)
        {
            //Point newPoint = new Point(vertex.X, vertex.Y);
            var edge1 = vertex.GetInEdge();
            var edge2 = vertex.GetOutEdge();
            int oldLength1 = CalculateLength(edge1);
            int oldLength1in = CalculateLength(edge1.From.GetInEdge());
            int oldLength2 = CalculateLength(edge2);
            int oldLength2Out = CalculateLength(edge2.To.GetOutEdge());
            vertex.Coord = new Point(x, y);
            if (edge1.Relation != Relation.None)
            {
                CorrectClockwise(edge1);
            }
            if (edge2.Relation != Relation.None)
            {
                CorrectCounterclockwise(edge2);
            }
            RepaintPolygon();
        }

        private void MoveEdge(Edge edge, int x, int y)
        {
            edge.From.Coord = new Point(edge.From.X+ x, edge.From.Y + y);
            edge.To.Coord = new Point(edge.To.X+ x, edge.To.Y + y);
            //correctedEdges = new HashSet<Edge>();
            //for (int i = 0; i < edge.From.Edges.Count; i++)
            //{
            //    var correctEdge = edge.From.Edges[i];
            //    if (correctEdge.Relation != Relation.None)
            //    {
            //        if (correctEdge != edge)
            //        {
            //            if (correctEdge.To == edge.From)
            //                CorrectClockwise(correctEdge);
            //            else
            //                CorrectCounterclockwise(correctEdge);
            //        }
            //    }
            //}
            //for (int i = 0; i < edge.To.Edges.Count; i++)
            //{
            //    var correctEdge = edge.To.Edges[i];
            //    if (correctEdge.Relation != Relation.None)
            //    {
            //        if (correctEdge != edge)
            //        {
            //            if (correctEdge.From == edge.To)
            //                CorrectCounterclockwise(correctEdge);
            //            else
            //                CorrectClockwise(correctEdge);
            //        }
            //    }
            //}
            RepaintPolygon();
        }

        private void MovePolygon(List<Vertex> vertices, int x, int y)
        {
            foreach (var vertex in vertices)
            {
                vertex.Coord = new Point(vertex.X+ x, vertex.Y + y);
            }
            RepaintPolygon();
        }

        private void OnBoadMouseClick(object sender, MouseEventArgs e)
        {
            Vertex mouse = new Vertex(e.X, e.Y);
            if (menuOption == MenuOption.AddVertex)
            {
                AddVertex(mouse);
            }
            else if (menuOption == MenuOption.DeleteVertex)
            {
                RemoveVertex(mouse);
            }
            else if (menuOption == MenuOption.AddRelation)
            {
                Edge edge = FindEdge(mouse).edge;
                if (edge != null)
                {
                    if (clickedEdges.IndexOf(edge) == -1 && edge.Relation == Relation.None)
                        clickedEdges.Add(edge);
                    if (clickedEdges.Count == 2)
                    {
                        Edge e1 = clickedEdges[0], e2 = clickedEdges[1];
                        Point old = new Point(e1.To.X, e1.To.Y);
                        edgesInRelation.Add((e1, e2));
                        int olde1 = CalculateLength(e1);
                        int olde1Out = CalculateLength(e1.To.GetOutEdge());
                        int olde1In = CalculateLength(e1.From.GetInEdge());
                        int olde2 = CalculateLength(e2);
                        int olde2Out = CalculateLength(e2.To.GetOutEdge());
                        int olde2In = CalculateLength(e2.From.GetInEdge());
                        if (relation == Relation.Equality)
                            EqualEdges(e1, e2, e1.From);
                        else if (relation == Relation.Perpendicular)
                            PerpendiculateEdges(e1, e2, e1.From);
                        CorrectClockwise(e1);
                        CorrectCounterclockwise(e1.To.GetOutEdge());
                        CorrectClockwise(e2);
                        CorrectCounterclockwise(e2.To.GetOutEdge());
                        clickedEdges = new List<Edge>();
                    }
                    return;
                }
            }
            else if (menuOption == MenuOption.RemovePolygon)
            {
                Polygon polygonToRemove = polygonToRemove = FindVertex(mouse).polygon;
                if (polygonToRemove == null)
                    polygonToRemove = FindEdge(mouse).polygon;
                if (polygonToRemove != null)
                    RemovePolygon(polygonToRemove);
            }
            else if (menuOption == MenuOption.RemoveRelation)
            {
                Edge edge = FindEdge(mouse).edge;
                if (edge != null && edge.Relation != Relation.None)
                    RemoveRelation(edge);
                RepaintPolygon();
            }
        }

        private void OnBoadMouseDown(object sender, MouseEventArgs e)
        {
            mouse = new Vertex(e.X, e.Y);
            mouseDown = true;
            if (menuOption == MenuOption.MoveComponent)
            {
                Polygon pl;
                (movingVertex, pl) = FindVertex(mouse);
                if (movingVertex != null && pl != null)
                {
                    edges = pl.Edges;
                    vertices = pl.Vertices;
                }
                if (movingVertex == null)
                {
                    (movingEdge, pl) = FindEdge(mouse);
                    if (movingEdge != null && pl != null)
                    {
                        edges = pl.Edges;
                        vertices = pl.Vertices;
                    }
                }
            }
            else if (menuOption == MenuOption.MovePolygon)
            {
                movingPolygon = FindEdge(mouse).polygon;
                if (movingPolygon == null)
                    movingPolygon = FindVertex(mouse).polygon;
            }
            else if (menuOption == MenuOption.HalveEdge)
            {
                (Edge edgeToHalve, Polygon polygon, int index) = FindEdgeWithIndex(mouse);
                if (edgeToHalve != null)
                    HalveEdge(polygon, edgeToHalve, index);
            }
        }

        private void OnBoardMouseMove(object sender, MouseEventArgs e)
        {
            if (menuOption == MenuOption.MoveComponent)
            {
                if (mouseDown && movingVertex != null)
                {
                    MoveVertex(movingVertex, e.X, e.Y);
                }
                else if (mouseDown && movingEdge != null && !mouse.IsEmpty())
                {
                    Point old = new Point(e.X, e.Y);
                    MoveEdge(movingEdge, e.X - mouse.Coord.X, e.Y - mouse.Y);
                    CorrectCounterclockwise(movingEdge.To.GetOutEdge());
                    CorrectClockwise(movingEdge.From.GetInEdge());
                }
            }
            else if (menuOption == MenuOption.MovePolygon && mouseDown)
            {
                if (movingPolygon != null && mouseDown && !mouse.IsEmpty())
                {
                    int diffx = e.X - mouse.Coord.X;
                    int diffy = e.Y - mouse.Y;
                    Debug.WriteLine($"{diffx} {diffy}");
                    MovePolygon(movingPolygon.Vertices, diffx, diffy);
                }
            }
            mouse = new Vertex(e.X, e.Y);
        }

        private void OnBoardMouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            movingVertex = null;
            movingEdge = null;
            movingPolygon = null;
            mouse = new Vertex();
        }

        private void OnBoardPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (polygons.Count == 0)
                return;
            foreach (var polygon in polygons)
            {
                foreach (var vertex in polygon.Vertices)
                {
                    g.FillEllipse(Brushes.Red, vertex.X - 3, vertex.Y - 3, 6, 6);
                    g.DrawString($"({vertex.X}, {vertex.Y})", new Font("Arial", 10), Brushes.Black, vertex.X- 3, vertex.Y + 15);
                }
                if (polygon.Edges.Count == 0)
                    continue;
                foreach (var edge in polygon.Edges)
                {
                    Point from = edge.From.Coord;
                    Point to = edge.To.Coord;
                    if (!from.IsEmpty && !to.IsEmpty)
                        g.DrawLine(new Pen(Brushes.Black, 1), from, to);
                }
            }
            foreach ((Edge e1, Edge e2) in edgesInRelation)
            {
                int x = (Math.Max(e1.From.Coord.X, e1.To.Coord.X) + Math.Min(e1.From.Coord.X,
                    e1.To.Coord.X)) / 2;
                int y = (Math.Max(e1.From.Y, e1.To.Y) + Math.Min(e1.From.Y,
                    e1.To.Y)) / 2;
                Bitmap bitmap;
                if (e1.Relation == Relation.Equality)
                    bitmap = new Bitmap(Properties.Resources.EqualSign);
                else
                    bitmap = new Bitmap(Properties.Resources.PerpendicularSign);
                int x2 = (Math.Max(e2.From.Coord.X, e2.To.Coord.X) + Math.Min(e2.From.Coord.X,
                    e2.To.Coord.X)) / 2;
                int y2 = (Math.Max(e2.From.Y, e2.To.Y) + Math.Min(e2.From.Y,
                    e2.To.Y)) / 2;
                g.DrawImage(bitmap, x, y - 10, 10, 10);
                g.DrawImage(bitmap, x2, y2 - 10, 10, 10);
            }
        }

        private void RepaintPolygon()
        {
            Board.Invalidate();
        }

        private bool IsLastPolygonCorrect()
        {
            if (polygons.Count > 0 && polygons[polygons.Count - 1].Vertices.Count < 3
                && polygons[polygons.Count - 1].Vertices.Count > 0)
            {
                MessageBox.Show("A figure has to have more than 2 vertices - deleting last polygon");
                polygons.RemoveAt(polygons.Count - 1);
                RepaintPolygon();
                vertices = new List<Vertex>();
                edges = new List<Edge>();
                polygons.Add(new Polygon(vertices, edges));
                return false;
            }
            return true;
        }

        private bool BelongsToCircle(Vertex vertex, Vertex circle)
        {
            double d = Math.Sqrt((vertex.X- circle.Coord.X) * (vertex.X- circle.Coord.X) +
                    (vertex.Y - circle.Y) * (vertex.Y - circle.Y));
            if (d <= 3)
            {
                return true;
            }
            return false;
        }

        private bool BelongsToSegment(Vertex vertex, Edge segment)
        {
            int x = vertex.Coord.X;
            int y = vertex.Y;
            int ax = segment.From.Coord.X;
            int ay = segment.From.Y;
            int bx = segment.To.Coord.X;
            int by = segment.To.Y;
            double dx = bx - ax;
            double dy = by - ay;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double d = (dy * (y - ay) - dx * (x - ax)) / len;
            if (Math.Abs(d) < 4 && x >= Math.Min(ax, bx) && x <= Math.Max(ax, bx) &&
                y >= Math.Min(ay, by) && y <= Math.Max(ay, by))
            {
                Debug.WriteLine($"({ax}, {ay}) -> ({bx}, {by})");
                return true;
            }
            double u = (double)((bx - ax) * (x - ax) + (by - ay) * (y - ay)) /
                (double)((bx - ax) * (bx - ax) + (by - ay) * (by - ay));
            int x3 = (int)(ax + u * (bx - ax));
            int y3 = (int)(ay + u * (by - ay));
            if (CalculateLength(new Edge(new Vertex(x3, y3), new Vertex(x,y))) < 3 && x >= Math.Min(ax, bx) - 1 && x <= Math.Max(ax, bx) + 1 &&
                y >= Math.Min(ay, by) - 1 && y <= Math.Max(ay, by) + 1)
            {
                Debug.WriteLine($"({ax}, {ay}) -> ({bx}, {by})");
                return true;
            }
            return false;
        }

        private bool ArePerpendicular(Edge edge1, Edge edge2)
        {
            int a1, a2;
            if (edge1.To.X - edge1.From.X == 0 && edge2.To.X - edge2.From.X == 0)
                return false;
            else if (edge1.To.X - edge1.From.X == 0)
            {
                a2 = (edge2.To.Y - edge2.From.Y) / (edge2.To.X - edge2.From.X);
                if (a2 == 0)
                    return true;
                else
                    return false;
            }
            else if (edge2.To.X - edge2.From.X == 0)
            {
                a1 = (edge1.To.Y - edge1.From.Y) / (edge1.To.X - edge1.From.X);
                if (a1 == 0)
                    return true;
                else
                    return false;
            }
            else
            {
                a2 = (edge2.To.Y - edge2.From.Y) / (edge2.To.X - edge2.From.X);
                a1 = (edge1.To.Y - edge1.From.Y) / (edge1.To.X - edge1.From.X);
                if (a1 * a2 == -1)
                    return true;
                else
                    return false;
            }
        }

        private bool AreEqual(Edge edge1, Edge edge2)
        {
            return CalculateLength(edge1) == CalculateLength(edge2);
        }

        private int CalculateLength(Edge e)
        {
            int wynik = (Math.Abs(e.To.Y - e.From.Y) * Math.Abs(e.To.Y
                - e.From.Y))
                + (Math.Abs(e.To.X- e.From.Coord.X) * Math.Abs(e.To.Coord.X
                - e.From.Coord.X));
            return (int)Math.Sqrt(wynik);
        }
    }
}
