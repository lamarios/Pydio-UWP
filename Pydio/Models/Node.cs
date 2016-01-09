using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pydio.Models
{
    interface Node
    {
        string Label { get; }
        string Id { get; }
        string Icon { get;  }
        bool Folder { get; }
        string Mime { get; }
        string Path { get; }

    }
}
