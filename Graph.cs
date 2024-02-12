using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Globalization;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using System.Threading.Tasks;


namespace Graphing
{
    public class Graph
    {
        public string Name { get; set; }
        public List<Node> Nodes { get; private set; }
        public List<Edge> Edges { get; private set; }
        public Graph(string name) 
        {
            Name = name;
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Nodes.Count)
              .AppendLine();
            for (int i = 0; i < Nodes.Count; i++)
            {
                sb.Append(i)
                .Append(' ')
                .Append(Nodes[i].Name == "" ? "_" : Nodes[i].Name
                .Replace(" ", "_"))
                .Append(' ')
                .Append(Nodes[i].X.ToString(CultureInfo.InvariantCulture))
                .Append(' ')
                .Append(Nodes[i].Y.ToString(CultureInfo.InvariantCulture))
                .AppendLine();
            }
                
            HashSet<(int, int, double)> hs = new HashSet<(int, int, double)>();
            foreach (Edge e in Edges)
                hs.Add((Math.Min(Nodes.IndexOf(e.A), Nodes.IndexOf(e.B)), Math.Max(Nodes.IndexOf(e.A), Nodes.IndexOf(e.B)), e.Weight));

            foreach ((int a, int b, double w) in hs)
            {
                sb.Append(a)
                .Append(' ')
                .Append(b)
                .Append(' ')
                .Append(w.ToString(CultureInfo.InvariantCulture))
                .AppendLine();
            }
                

            return sb.ToString();
        }

        public static Graph ParseFile(string path)
        {
            Graph toreturn = new Graph("New");
            string[] buffer = File.ReadAllLines(path);
            int n = int.Parse(buffer[0]);
            for(int i = 1; i < n + 1; i++)
            {
                string name = buffer[i].Split(' ')[1].Trim().Replace("_"," ");
                int value = int.Parse(buffer[i].Split(' ')[2].Trim());
                double Xpos = double.Parse(buffer[i].Split(' ')[3].Trim(), CultureInfo.InvariantCulture);
                double Ypos = double.Parse(buffer[i].Split(' ')[4].Trim(), CultureInfo.InvariantCulture);
                Node toadd = new Node()
                {
                    Name = name,
                    X = Xpos,
                    Y = Ypos,
                };
                toreturn.Nodes.Add(toadd);              
            }
            for(int i = n + 1; i < buffer.Length; i++)
            {
                int a = int.Parse(buffer[i].Split(' ')[0].Trim());
                int b = int.Parse(buffer[i].Split(' ')[1].Trim());
                double weight = double.Parse(buffer[i].Split(' ')[2].Trim(), CultureInfo.InvariantCulture);
                Edge toadd = new Edge(toreturn.Nodes[a], toreturn.Nodes[b])
                {
                    Weight = weight,
                };
                toreturn.Edges.Add(toadd);
            }

            return toreturn;
        }

    }
    public static class GraphExt
    {
        public static double EuclidDist(Node a, Node b) => EuclidDist(new Point(a.X, a.Y), new Point(b.X, b.Y));
        public static double EuclidDist(Point a, Point b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        public static Task<List<Edge>> Kruskal(this Graph g)
        {
            List<Edge> MinimumSpanningTree = new List<Edge>();

            if (!g.Nodes.Any())
                return Task.FromResult(MinimumSpanningTree);

            List<Edge> SortedEdges = new List<Edge>();

            if (g.Edges.Any())
                SortedEdges.AddRange(g.Edges);
            else
                for(int i = 0; i < g.Nodes.Count; i++)
                    for(int j = i + 1; j < g.Nodes.Count; j++)
                        SortedEdges.Add(new Edge(g.Nodes[i], g.Nodes[j]) { Weight = EuclidDist(new Point(g.Nodes[i].X, g.Nodes[i].Y), new Point(g.Nodes[j].X, g.Nodes[j].Y)) } );

            SortedEdges.Sort((x,y) => x.Weight.CompareTo(y.Weight));

            List<List<Node>> SubGraphs = new List<List<Node>>();
            MinimumSpanningTree.Add(SortedEdges.First());
            SubGraphs.Add(new List<Node>() { SortedEdges.First().A, SortedEdges.First().B });

            SortedEdges.Remove(SortedEdges.First());

            while (SortedEdges.Any())
            {
                if (SubGraphs.First().Count == g.Nodes.Count)
                    break;

                Edge next = SortedEdges.First();
                SortedEdges.Remove(next);
                MinimumSpanningTree.Add(next);

                int A_ParentSubgraph = -1;
                int B_ParentSubgraph = -1;
                
                SubGraphs.ForEach((x) => { 
                    if (x.Contains(next.A))
                        A_ParentSubgraph = SubGraphs.IndexOf(x);
                    if (x.Contains(next.B))
                        B_ParentSubgraph = SubGraphs.IndexOf(x);
                });
                if(A_ParentSubgraph == -1 && B_ParentSubgraph == -1)
                    SubGraphs.Add(new List<Node>() { next.A, next.B });
                else if (A_ParentSubgraph != -1 && B_ParentSubgraph == -1)
                    SubGraphs[A_ParentSubgraph].Add(next.B);
                else if (B_ParentSubgraph != -1 && A_ParentSubgraph == -1)
                    SubGraphs[B_ParentSubgraph].Add(next.A);
                else if((B_ParentSubgraph != -1 && A_ParentSubgraph != -1) && A_ParentSubgraph != B_ParentSubgraph)
                {
                    SubGraphs[A_ParentSubgraph] = SubGraphs[A_ParentSubgraph].Union(SubGraphs[B_ParentSubgraph]).ToList();
                    SubGraphs.Remove(SubGraphs[B_ParentSubgraph]);
                }
                else
                    MinimumSpanningTree.Remove(next);

            }

            if(SubGraphs.First().Count != g.Nodes.Count)
                MinimumSpanningTree.Clear();

            return Task.FromResult(MinimumSpanningTree);
        }
        public static Task<Dictionary<Node, int>> GraphColor(this Graph g)
        {
            Dictionary<Node, int> colored = new Dictionary<Node, int>();           

            Dictionary<Node, List<Node>> nodeneighbors = new Dictionary<Node, List<Node>>();
            List<(Node,int)> ns = new List<(Node,int)>();
            foreach (Node n in g.Nodes)
            {
                colored.Add(n, -1);
                nodeneighbors.Add(n, g.Edges.Where(x => x.A == n || x.B == n).Select(x => x.A == n ? x.B : x.A).ToList());
                ns.Add((n, nodeneighbors[n].Count));
            }
            ns.Sort((x, y) => -1 * x.Item2.CompareTo(y.Item2));

            foreach ((Node n, _) in ns)
            {
                if (colored[n] >= 0)
                    continue;

                bool[] used = new bool[g.Nodes.Count];
                nodeneighbors[n].Where(x => colored[x] >= 0).ToList().ForEach(x => used[colored[x]] = true);

                colored[n] = used.TakeWhile(x => x == true).Count();
                        
            }

            return Task.FromResult(colored);
        }
        public static Task<List<List<Node>>> Hamilton(this Graph g, bool returnallpaths, bool cycle)
        {
            List<Node> curr = new List<Node>();
            List<List<Node>> res = new List<List<Node>>();
            foreach (Node node in g.Nodes)
            {
                curr.Add(node);
                HamDFS(g, curr, node, res, returnallpaths, cycle);
                curr.Remove(node);
            }
            return Task.FromResult(res);
        }
        static void HamDFS(Graph g, List<Node> curr, Node current, List<List<Node>> res, bool returnallpaths, bool cycle)
        {
            if (res.Any() && !returnallpaths)
                return;
            else if (res.Count > 100)
                return;
            if (curr.Count == g.Nodes.Count)
            {
                List<Node> found = new List<Node>(curr);
                if (cycle)
                {
                    List<Node> neighbors = g.Edges.Where(x => x.A == current || x.B == current).Select(x => x.A == current ? x.B : x.A).ToList();
                    if (neighbors.Contains(curr.First()))
                    {
                        found.Add(curr.First());
                        res.Add(found);
                    }
                        
                }
                else
                    res.Add(found);
            }
            else
            {
                List<Node> neighbors = g.Edges.Where(x => x.A == current || x.B == current).Select(x => x.A == current ? x.B : x.A).ToList();
                foreach (Node n in neighbors)
                {
                    if (curr.Contains(n))
                        continue;
                    curr.Add(n);
                    HamDFS(g, curr, n, res, returnallpaths, cycle);
                    curr.Remove(n);
                }
            }
        }
        public static Task<List<List<Edge>>> Euler(this Graph g, bool returnallpaths, bool cycle)
        {
            List<Edge> curr = new List<Edge>();
            List<List<Edge>> res = new List<List<Edge>>();
            foreach (Node n in g.Nodes)
                EulerDFS(g, curr, n, res, returnallpaths, cycle, n);

            return Task.FromResult(res);
        }
        static void EulerDFS(Graph g, List<Edge> curr, Node current, List<List<Edge>> res, bool returnallpaths, bool cycle, Node start)
        {
            if (res.Any() && !returnallpaths)
                return;
            else if (res.Count > 100)
                return;
            if (curr.Count == g.Edges.Count)
            {
                List<Edge> found = new List<Edge>(curr);
                if (cycle)
                {
                    if (current == start)
                    {
                        found.Add(curr.First()); 
                        res.Add(found);
                    }                   
                }
                else
                    res.Add(found);
            }
            else
            {
                List<Edge> neighbors = g.Edges.Where(x => (x.A == current || x.B == current)).ToList();
                foreach (Edge e in neighbors)
                {
                    if (curr.Contains(e))
                        continue;
                    curr.Add(e);
                    EulerDFS(g, curr, e.A == current ? e.B : e.A, res, returnallpaths, cycle, start);
                    curr.Remove(e);
                }
            }
        }

        public static Task<(List<Node>,double)> Dijkstra(this Graph g, Node start, Node target)
        {
            List<Node> result = new List<Node>();
            decimal bestdist = -1;

            Dictionary<Edge,decimal> edgew = new Dictionary<Edge,decimal>();
            foreach (Edge e in g.Edges)
            {
                decimal weight = (decimal)Math.Round(e.Weight, 3);
                if (weight == 0)
                    weight = 1;
                edgew.Add(e, weight);
            }
                

            Dictionary<Node, decimal> distarray = new Dictionary<Node, decimal>();
            foreach(Node n in g.Nodes)
                distarray.Add(n, decimal.MaxValue);

            PriorityQueue<(Node,decimal), decimal> pq = new PriorityQueue<(Node,decimal), decimal>();
            pq.Enqueue((start,0), 0);
            while(pq.Count > 0)
            {
                (Node deq, decimal dist) = pq.Dequeue();
                if (distarray[deq] != decimal.MaxValue)
                    continue;
                distarray[deq] = dist;
                List<Edge> neighbors = g.Edges.Where(x => (x.A == deq || x.B == deq)).ToList();
                foreach(Edge e in neighbors)
                {
                    decimal nextdist = dist + edgew[e];
                    pq.Enqueue((e.A == deq ? e.B : e.A, nextdist), nextdist);
                }
            }            

            if (distarray[target] == decimal.MaxValue)
                return Task.FromResult((result, (double)bestdist));


            bestdist = distarray[target];
            Node back = target;
            while(back != start)
            {
                List<Edge> neighbors = g.Edges.Where(x => (x.A == back || x.B == back)).ToList();
                foreach(Edge e in neighbors)
                {
                    Node other = e.A == back ? e.B : e.A;
                    decimal distbetween = distarray[back] - distarray[other];
                    if (edgew[e] == distbetween)
                    {
                        result.Add(back);
                        back = other;
                    }
                }
            }
            result.Add(start);

            return Task.FromResult((result, (double)bestdist));
        }
    }
    public class Node
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Node(string name = "")
        {
            Name = name;
        }
    }
    public class Edge
    {
        public double Weight { get; set; }
        public Node A { get; private set; }
        public Node B { get; private set; }
        public Edge(Node a, Node b)
        {
            A = a;
            B = b;
        }
    }
}
