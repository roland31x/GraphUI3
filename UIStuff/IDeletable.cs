using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace GraphUI3
{
#nullable enable
    public interface IDeletable
    {
        public delegate void DeletionRequestHandler(object sender, DeletionEventArgs e);
        public event DeletionRequestHandler? DeleteRequest;
        public void SendDeleteRequest();
    }
}
