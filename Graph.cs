using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;


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
            sb.Append(Nodes.Count).AppendLine();
            for (int i = 0; i < Nodes.Count; i++)
                sb.Append(i).Append(' ').Append(Nodes[i].Name == "" ? "_" : Nodes[i].Name.Replace(" ","_")).Append(' ').Append(Nodes[i].Value).Append(' ').Append(Nodes[i].X).Append(' ').Append(Nodes[i].Y).AppendLine();
            HashSet<(int, int, int)> hs = new HashSet<(int, int, int)>();
            foreach (Edge e in Edges)
                hs.Add((Math.Min(Nodes.IndexOf(e.A), Nodes.IndexOf(e.B)), Math.Max(Nodes.IndexOf(e.A), Nodes.IndexOf(e.B)), e.Weight));
            foreach ((int a, int b, int w) in hs)
                sb.Append(a).Append(' ').Append(b).Append(' ').Append(w).AppendLine();

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
                int Xpos = int.Parse(buffer[i].Split(' ')[3].Trim());
                int Ypos = int.Parse(buffer[i].Split(' ')[4].Trim());
                Node toadd = new Node()
                {
                    Name = name,
                    Value = value,
                    X = Xpos,
                    Y = Ypos,
                };
                toreturn.Nodes.Add(toadd);              
            }
            for(int i = n + 1; i < buffer.Length; i++)
            {
                int a = int.Parse(buffer[i].Split(' ')[0].Trim());
                int b = int.Parse(buffer[i].Split(' ')[1].Trim());
                int weight = int.Parse(buffer[i].Split(' ')[2].Trim());
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
        public static Dictionary<Node, int> GraphColor(this Graph g)
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

            return colored;
        }
        public static List<List<Node>> Hamilton(this Graph g, bool returnallpaths, bool cycle)
        {
            List<Node> curr = new List<Node>();
            List<List<Node>> res = new List<List<Node>>();
            foreach (Node node in g.Nodes)
            {
                curr.Add(node);
                HamDFS(g, curr, node, res, returnallpaths, cycle);
                curr.Remove(node);
            }
            return res;
        }
        static void HamDFS(Graph g, List<Node> curr, Node current, List<List<Node>> res, bool returnallpaths, bool cycle)
        {
            if (res.Any() && !returnallpaths)
                return;
            if (curr.Count == g.Nodes.Count)
            {
                List<Node> found = new List<Node>(curr);
                if (cycle)
                {
                    List<Node> neighbors = g.Edges.Where(x => x.A == current || x.B == current).Select(x => x.A == current ? x.B : x.A).ToList();
                    if (neighbors.Contains(curr.First()))
                        res.Add(found);
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
        public static List<List<Edge>> Euler(this Graph g, bool returnallpaths, bool cycle)
        {
            List<Edge> curr = new List<Edge>();
            List<List<Edge>> res = new List<List<Edge>>();
            foreach (Edge node in g.Edges)
            {
                curr.Add(node);
                EulerDFS(g, curr, node, res, returnallpaths, cycle);
                curr.Remove(node);
            }
            return res;
        }
        static void EulerDFS(Graph g, List<Edge> curr, Edge current, List<List<Edge>> res, bool returnallpaths, bool cycle)
        {
            if (res.Any() && !returnallpaths)
                return;
            if (curr.Count == g.Nodes.Count)
            {
                List<Edge> found = new List<Edge>(curr);
                if (cycle)
                {
                    List<Edge> neighbors = g.Edges.Where(x => x.A == current.A || x.B == current.B).ToList();
                    if (neighbors.Contains(curr.First()))
                        res.Add(found);
                }
                else
                    res.Add(found);
            }
            else
            {
                List<Edge> neighbors = g.Edges.Where(x => x.A == current.A || x.B == current.B).ToList();
                foreach (Edge e in neighbors)
                {
                    if (curr.Contains(e))
                        continue;
                    curr.Add(e);
                    EulerDFS(g, curr, e, res, returnallpaths, cycle);
                    curr.Remove(e);
                }
            }
        }
    }
    public class Node
    {
        public int Value { get; set; } = 0;
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Node(int value = 0, string name = "")
        {
            Value = value;
            Name = name;
        }
    }
    public class Edge
    {
        public int Weight { get; set; }
        public Node A { get; private set; }
        public Node B { get; private set; }
        public Edge(Node a, Node b)
        {
            A = a;
            B = b;
        }
    }
}
