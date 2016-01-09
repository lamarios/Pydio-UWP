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

        string Node.Icon
        {
            get
            {
                return "\uE8B7";
            }
        }

        public bool Folder
        {
            get
            {
                return true;
            }
        }

        public string Mime
        {
            get
            {
                return "";
            }
        }

        public string Path
        {
            get
            {
                return Id;
            }
        }
    }
}
