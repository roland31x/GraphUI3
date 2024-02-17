namespace Graphing
{
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
