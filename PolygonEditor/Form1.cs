using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PolygonEditor
{
    enum MenuOption { MoveComponent, AddVertex, DeleteVertex, MovePolygon, RemovePolygon, HalveEdge,
        AddRelation, RemoveRelation }
    enum Relation { None, Equality, Perpendicular }

    public partial class PolygonEditor : Form
    {
        MenuOption menuOption = MenuOption.MoveComponent;
        Relation relation = Relation.None;

        List<Polygon> polygons = new List<Polygon>();
        List<(Edge edge1, Edge edge2)> edgesInRelation = new List<(Edge, Edge)>();
        List<Edge> clickedEdges = new List<Edge>();

        Vertex movingVertex = null;
        Edge movingEdge = null;
        Polygon movingPolygon = null;
        Vertex mouse = new Vertex();
        ToolStripMenuItem lastMenu = null;

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
            EqualEdges(predefinedEdges[0], predefinedEdges[2], predefinedEdges[0].From);
            PerpendiculateEdges(predefinedEdges[3], predefinedEdges[4], predefinedEdges[3].From);
            polygons.Add(new Polygon(predefinedVertices, predefinedEdges));
            edgesInRelation.Add((predefinedEdges[0], predefinedEdges[2]));
            edgesInRelation.Add((predefinedEdges[3], predefinedEdges[4]));
            SetMenuItemColor(MoveComponentMenuItem);
            RepaintPolygon();
        }

        private void OnMoveComponentMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.MoveComponent;
            SetMenuItemColor(MoveComponentMenuItem);
        }

        private void OnAddVertexMenuItemClick(object sender, EventArgs e)
        {
            IsLastPolygonCorrect();
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();
            polygons.Add(new Polygon(vertices, edges));
            menuOption = MenuOption.AddVertex;
            SetMenuItemColor(AddVertexMenuItem);
        }

        private void OnRemoveVertexMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.DeleteVertex;
            SetMenuItemColor(RemoveVertexMenuItem);
        }

        private void OnMovePolygonMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.MovePolygon;
            SetMenuItemColor(MovePolygonMenuItem);
        }

        private void OnRemovePolygonMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.RemovePolygon;
            SetMenuItemColor(RemovePolygonMenuItem);
        }

        private void OnHalveEdgeMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.HalveEdge;
            SetMenuItemColor(HalveEdgeMenuItem);
        }

        private void OnEqualEdgesMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            clickedEdges = new List<Edge>();
            menuOption = MenuOption.AddRelation;
            relation = Relation.Equality;
            SetMenuItemColor(EqualEdgesMenuItem);
        }

        private void OnPerpendiculateEdgesMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            clickedEdges = new List<Edge>();
            menuOption = MenuOption.AddRelation;
            relation = Relation.Perpendicular;
            SetMenuItemColor(PerpendiculateEdgesMenuItem);
        }

        private void OnRemoveRelationMenuItemClick(object sender, EventArgs e)
        {
            if (!IsLastPolygonCorrect())
                return;
            menuOption = MenuOption.RemoveRelation;
            SetMenuItemColor(RemoveRelationMenuItem);
        }

        private void AddVertex(Vertex e)
        {
            List<Vertex> vertices = polygons[polygons.Count - 1].Vertices;
            List<Edge> edges = polygons[polygons.Count - 1].Edges;
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
            polygon.Edges.Add(new Edge(from, to));
            polygon.Vertices.Remove(vertexToRemove);
            RepaintPolygon();
        }

        private void RemovePolygon(Polygon polygon)
        {
            polygons.Remove(polygon);
            foreach (var edge in polygon.Edges)
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
            edge1.Relation = Relation.Equality;
            edge2.Relation = Relation.Equality;
            edge1.InRelation = edge2;
            edge2.InRelation = edge1;
            if (AreEqual(edge1, edge2))
            {
                RepaintPolygon();
                return;
            }
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
                int length1 = CalculateLength(new Edge(new Vertex((int)Math.Round(x1), (int)Math.Round(y1)),
                    new Vertex(movingPoint.X, movingPoint.Y)));
                int length2 = CalculateLength(new Edge(new Vertex((int)Math.Round(x2), (int)Math.Round(y2)),
                    new Vertex(movingPoint.X, movingPoint.Y)));
                movingPoint.Coord = length1 < length2 ? new Point((int)Math.Round(x1), (int)Math.Round(y1)) :
                    new Point((int)Math.Round(x2), (int)Math.Round(y2));
            }
            RepaintPolygon();
        }

        private void PerpendiculateEdges(Edge edge1, Edge edge2, Vertex staticPoint)
        {
            edge1.Relation = Relation.Perpendicular;
            edge2.Relation = Relation.Perpendicular;
            edge1.InRelation = edge2;
            edge2.InRelation = edge1;
            if (ArePerpendicular(edge1, edge2))
            {
                RepaintPolygon();
                return;
            }
            double x1 = edge2.From.X;
            double y1 = edge2.From.Y;
            double x2 = edge2.To.X;
            double y2 = edge2.To.Y;
            Vertex movingPoint = edge1.From.Coord == staticPoint.Coord ? edge1.To : edge1.From;
            double a = (double)(y2 - y1) / (double)(x2 - x1);
            double b = y1 - a * x1;
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
            int length1 = CalculateLength(new Edge(new Vertex((int)Math.Round(x1), (int)Math.Round(y1)),
            new Vertex(movingPoint.X, movingPoint.Y)));
            int length2 = CalculateLength(new Edge(new Vertex((int)Math.Round(x2), (int)Math.Round(y2)),
                new Vertex(movingPoint.X, movingPoint.Y)));
            movingPoint.Coord = length1 < length2 ? new Point((int)Math.Round(x1), (int)Math.Round(y1)) :
                new Point((int)Math.Round(x2), (int)Math.Round(y2));
            RepaintPolygon();
        }

        private void HalveEdge(Polygon polygon, Edge edge, int i)
        {
            if (CalculateLength(edge) < 6)
            {
                MessageBox.Show("Edge is too small to halve");
                return;
            }
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

        private bool CorrectClockwise(Edge e)
        {
            var edge = e;
            int iter = 0;
            while (edge.Relation != Relation.None && iter <= movingPolygon.Edges.Count)
            {
                if (edge.Relation == Relation.Equality && AreEqual(edge, edge.InRelation))
                    break;
                else if (edge.Relation == Relation.Perpendicular && ArePerpendicular(edge, edge.InRelation))
                    break;
                var edgein = edge.From.GetInEdge().To.GetInEdge();
                if (edge.Relation == Relation.Equality)
                {
                    if (edgein.Relation == Relation.Equality || edgein.Relation == Relation.Perpendicular)
                    {
                        EqualEdges(edge, edge.InRelation, edge.To);
                    }
                    else if (edgein.Relation == Relation.Perpendicular)
                    {
                        EqualEdges(edge, edge.InRelation, edge.To);
                        PerpendiculateEdges(edgein, edgein.InRelation, edgein.To);
                    }
                }
                else if (edge.Relation == Relation.Perpendicular)
                {
                    if (edgein.Relation == Relation.None || edgein.Relation == Relation.Perpendicular)
                    {
                        PerpendiculateEdges(edge, edge.InRelation, edge.To);
                    }
                    else
                    {
                        PerpendiculateEdges(edge, edge.InRelation, edge.To);
                        EqualEdges(edgein, edgein.InRelation, edgein.To);
                    }
                }
                edge = edgein;
                iter++;
            }
            return iter == movingPolygon.Edges.Count + 1 ? false : true;
        }

        private bool CorrectCounterclockwise(Edge e)
        {
            var edge = e;
            int iter = 0;
            while (edge.Relation != Relation.None && iter <= movingPolygon.Edges.Count)
            {
                if (edge.Relation == Relation.Equality && AreEqual(edge, edge.InRelation))
                    break;
                else if (edge.Relation == Relation.Perpendicular && ArePerpendicular(edge, edge.InRelation))
                    break;
                var edgeout = edge.From.GetOutEdge().To.GetOutEdge();
                if (edge.Relation == Relation.Equality)
                {
                    if (edgeout.Relation == Relation.None || edgeout.Relation == Relation.Equality)
                    {
                        EqualEdges(edge, edge.InRelation, edge.From);
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
            return iter == movingPolygon.Edges.Count + 1 ? false : true;
        }

        private void MoveVertex(Vertex vertex, int x, int y)
        {
            var edge1 = vertex.GetInEdge();
            var edge2 = vertex.GetOutEdge();
            int oldLength1 = CalculateLength(edge1);
            int oldLength1in = CalculateLength(edge1.From.GetInEdge());
            int oldLength2 = CalculateLength(edge2);
            int oldLength2Out = CalculateLength(edge2.To.GetOutEdge());
            vertex.Coord = new Point(x, y);
            bool correctedClockwise = true, correctedCounterclockwise = true;
            if (edge1.Relation != Relation.None)
                correctedClockwise = CorrectClockwise(edge1);
            if (edge2.Relation != Relation.None)
                correctedCounterclockwise = CorrectCounterclockwise(edge2);
            if (!correctedClockwise && !correctedClockwise)
                InvalidPolygonError();
            RepaintPolygon();
        }

        private void MoveEdge(Edge edge, int x, int y)
        {
            edge.From.Coord = new Point(edge.From.X + x, edge.From.Y + y);
            edge.To.Coord = new Point(edge.To.X + x, edge.To.Y + y);
            RepaintPolygon();
        }

        private void MovePolygon(List<Vertex> vertices, int x, int y)
        {
            foreach (var vertex in vertices)
            {
                vertex.Coord = new Point(vertex.X + x, vertex.Y + y);
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
                Edge edge;
                (edge, movingPolygon) = FindEdge(mouse);
                if (edge != null)
                {
                    if (clickedEdges.IndexOf(edge) == -1 && edge.Relation == Relation.None)
                    { 
                        if (clickedEdges.Count == 0)
                            clickedEdges.Add(edge);
                        else
                        {
                            if (movingPolygon.HasEdge(clickedEdges[0]))
                                clickedEdges.Add(edge);
                            else
                                MessageBox.Show("Cannot add relation between edges from different polygons!");
                        }
                    }
                    if (clickedEdges.Count == 2)
                    {
                        Edge e1 = clickedEdges[0], e2 = clickedEdges[1];
                        Point old = new Point(e1.To.X, e1.To.Y);
                        bool[] corrected = new bool[4];
                        edgesInRelation.Add((e1, e2));
                        if (relation == Relation.Equality)
                            EqualEdges(e1, e2, e1.From);
                        else if (relation == Relation.Perpendicular)
                            PerpendiculateEdges(e1, e2, e1.From);
                        corrected[0] = CorrectClockwise(e1);
                        corrected[1] = CorrectClockwise(e2);
                        corrected[2] = CorrectCounterclockwise(e1.To.GetOutEdge());
                        corrected[3] = CorrectCounterclockwise(e2.To.GetOutEdge());
                        if (!corrected[0] && !corrected[1] && !corrected[2] && !corrected[3])
                        {
                            InvalidPolygonError();
                            RepaintPolygon();
                        }
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
                (movingVertex, movingPolygon) = FindVertex(mouse);
                if (movingVertex == null)
                    (movingEdge, movingPolygon) = FindEdge(mouse);
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
                    bool correctedCounterclockwise = false, correctedClockwise = false;
                    correctedCounterclockwise = CorrectCounterclockwise(movingEdge.To.GetOutEdge());
                    correctedClockwise = CorrectClockwise(movingEdge.From.GetInEdge());
                    if (!correctedClockwise && !correctedCounterclockwise)
                    {
                        InvalidPolygonError();
                        RepaintPolygon();
                    }
                }
            }
            else if (menuOption == MenuOption.MovePolygon && mouseDown)
            {
                if (movingPolygon != null && mouseDown && !mouse.IsEmpty())
                {
                    int diffx = e.X - mouse.Coord.X;
                    int diffy = e.Y - mouse.Y;
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
                    g.DrawString($"({vertex.X}, {vertex.Y})", new Font("Arial", 10), Brushes.Black, vertex.X - 3, vertex.Y - 20);
                }
                if (polygon.Edges.Count == 0)
                    continue;
                foreach (var edge in polygon.Edges)
                {
                    Point from = edge.From.Coord;
                    Point to = edge.To.Coord;
                    if (!from.IsEmpty && !to.IsEmpty)
                        Bresenham(edge, g);
                        //g.DrawLine(new Pen(Brushes.Black, 1), from, to);
                }
            }
            int equal = 1, perpendicular = 1;
            foreach ((Edge e1, Edge e2) in edgesInRelation)
            {
                int x1 = (Math.Max(e1.From.Coord.X, e1.To.Coord.X) + Math.Min(e1.From.Coord.X,
                    e1.To.Coord.X)) / 2;
                int y1 = (Math.Max(e1.From.Y, e1.To.Y) + Math.Min(e1.From.Y,
                    e1.To.Y)) / 2;
                int x2 = (Math.Max(e2.From.Coord.X, e2.To.Coord.X) + Math.Min(e2.From.Coord.X,
                     e2.To.Coord.X)) / 2;
                int y2 = (Math.Max(e2.From.Y, e2.To.Y) + Math.Min(e2.From.Y,
                    e2.To.Y)) / 2;
                Bitmap bitmap;
                if (e1.Relation == Relation.Equality)
                {
                    bitmap = new Bitmap(Properties.Resources.EqualSign);
                    g.DrawString($"({equal})", new Font("Arial", 8), Brushes.Black, x1 + 10, y1 - 8);
                    g.DrawString($"({equal})", new Font("Arial", 8), Brushes.Black, x2 + 10, y2 - 8);
                    equal++;
                }
                else
                {
                    bitmap = new Bitmap(Properties.Resources.PerpendicularSign);
                    g.DrawString($"({perpendicular})", new Font("Arial", 8), Brushes.Black, x1 + 10, y1 - 8);
                    g.DrawString($"({perpendicular})", new Font("Arial", 8), Brushes.Black, x2 + 10, y2 - 8);
                    perpendicular++;
                }
                g.DrawImage(bitmap, x1, y1 - 10, 10, 10);
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
                List<Vertex> vertices = new List<Vertex>();
                List<Edge> edges = new List<Edge>();
                polygons.Add(new Polygon(vertices, edges));
                return false;
            }
            return true;
        }

        private void InvalidPolygonError()
        {
            MessageBox.Show("Couldn't preserve relations while moving polygon - " +
                "removing incorrect polygon");
            mouseDown = false;
            movingVertex = null;
            movingEdge = null;
            foreach (var edge in movingPolygon.Edges)
            {
                if (edge.Relation != Relation.None)
                    RemoveRelation(edge);
            }
            polygons.Remove(movingPolygon);
            movingPolygon = null;
            mouse = new Vertex();
        }

        private bool BelongsToCircle(Vertex vertex, Vertex circle)
        {
            double d = Math.Sqrt((vertex.X - circle.Coord.X) * (vertex.X - circle.Coord.X) +
                    (vertex.Y - circle.Y) * (vertex.Y - circle.Y));
            if (d <= 3)
                return true;
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
                return true;
            double u = (double)((bx - ax) * (x - ax) + (by - ay) * (y - ay)) /
                (double)((bx - ax) * (bx - ax) + (by - ay) * (by - ay));
            int x3 = (int)(ax + u * (bx - ax));
            int y3 = (int)(ay + u * (by - ay));
            if (CalculateLength(new Edge(new Vertex(x3, y3), new Vertex(x, y))) < 3
                && x >= Math.Min(ax, bx) - 1 && x <= Math.Max(ax, bx) + 1 
                && y >= Math.Min(ay, by) - 1 && y <= Math.Max(ay, by) + 1)
                return true;
            return false;
        }

        private bool ArePerpendicular(Edge edge1, Edge edge2)
        {
            double a1, a2;
            if (CalculateLength(edge1) == 0)
            {
                edge1.From.X += 1;
                edge1.To.Y += 1;
            }
            if (CalculateLength(edge2) == 0)
            {
                edge2.From.X += 1;
                edge2.To.Y += 1;
            }
            if (edge1.To.X - edge1.From.X == 0 && edge2.To.X - edge2.From.X == 0)
            {
                edge2.To.X += 1;
                edge2.To.Y += 1;
            }
            if (edge1.To.X - edge1.From.X == 0)
            {
                a2 = edge2.To.Y - edge2.From.Y;
                if (a2 == 0)
                    return true;
                else
                    return false;
            }
            else if (edge2.To.X - edge2.From.X == 0)
            {
                a1 = edge1.To.Y - edge1.From.Y;
                if (a1 == 0)
                    return true;
                else
                    return false;
            }
            else
            {
                a2 = (double)(edge2.To.Y - edge2.From.Y) / (edge2.To.X - edge2.From.X);
                a1 = (double)(edge1.To.Y - edge1.From.Y) / (edge1.To.X - edge1.From.X);
                if (a1 * a2 == -1)
                    return true;
                else
                    return false;
            }
        }

        private bool AreEqual(Edge edge1, Edge edge2)
        {
            if (CalculateLength(edge1) == 0)
            {
                edge1.From.X += 1;
                edge1.To.Y += 1;
            }
            if (CalculateLength(edge2) == 0)
            {
                edge2.From.X += 1;
                edge1.To.Y += 1;
            }
            return CalculateLength(edge1) == CalculateLength(edge2);
        }

        private int CalculateLength(Edge e)
        {
            return (int)Math.Sqrt((Math.Abs(e.To.Y - e.From.Y) * Math.Abs(e.To.Y
                - e.From.Y)) + (Math.Abs(e.To.X - e.From.Coord.X) * Math.Abs(e.To.Coord.X
                - e.From.Coord.X)));
        }

        private void SetMenuItemColor(ToolStripMenuItem menuItem)
        {
            if (lastMenu != null)
                lastMenu.BackColor = Color.Transparent;
            lastMenu = menuItem;
            lastMenu.BackColor = Color.MediumBlue;
        }

        private void Bresenham(Edge edge, Graphics graphics)
        {
            int x0 = edge.From.X;
            int y0 = edge.From.Y;
            int x1 = edge.To.X;
            int y1 = edge.To.Y;
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            for (;;)
            {
                graphics.FillRectangle(Brushes.Black, x0, y0, 1, 1);
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }
        }
    }
}
