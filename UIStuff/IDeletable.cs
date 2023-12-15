using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace GraphUI3
{
#nullable enable
    public interface IDeletable
    {
        public event DeletionRequestHandler? DeleteRequest;

        public delegate void DeletionRequestHandler(object sender, DeletionEventArgs e);        
        public void SendDeleteRequest();
    }
}
