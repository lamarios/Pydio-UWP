using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pydio.Models
{
    interface Node
    {
        string GetPath();
        string GetLabel();
        string GetIcon();
    }
}
