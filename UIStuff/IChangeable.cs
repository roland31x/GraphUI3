using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphUI3.UIStuff
{
    public interface IChangeable
    {
        public event ChangedEventHandler? ChangedEvent;

        public delegate void ChangedEventHandler(object sender, EventArgs e);
    }
}
