namespace GraphUI3
{
    public class MoveEventArgs
    {
        public bool Move { get; private set; }
        public MoveEventArgs(bool move)
        {
            Move = move;
        }
    }
}
