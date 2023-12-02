namespace GraphUI3
{
    public class SelectionEventArgs
    {
        public bool isSelected { get; private set; }
        public SelectionEventArgs(bool isSelected)
        {
            this.isSelected = isSelected;
        }
    }
}
