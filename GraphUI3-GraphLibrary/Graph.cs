using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Globalization;


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
                double Xpos = double.Parse(buffer[i].Split(' ')[2].Trim(), CultureInfo.InvariantCulture);
                double Ypos = double.Parse(buffer[i].Split(' ')[3].Trim(), CultureInfo.InvariantCulture);
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
}
