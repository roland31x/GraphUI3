using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace GraphUI3
{
    public class DeletionEventArgs
    {
        public List<UIElement> ToRemove { get; private set; }
        public DeletionEventArgs(List<UIElement> toRemove)
        {
            ToRemove = toRemove;
        }
    }
}
