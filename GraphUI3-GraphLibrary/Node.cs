namespace Graphing
{
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
}
