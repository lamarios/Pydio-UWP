using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pydio.Models
{
    class Repository : Node
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Icon = "&#xE8B7;";

        public string GetPath()
        {
            return Id;
        }

        public string GetLabel()
        {
            return Label;
        }

        public string GetIcon()
        {
            return Icon;
        }
    }
}
