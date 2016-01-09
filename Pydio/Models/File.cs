using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pydio.Models
{
    class File : Node
    {

        public string Id { get; set; }
        public string Label { get; set; }
        public bool Folder { get; set;  }
        public string Mime { get; set; }
        public string WorkSpace { get; set; }

        string Node.Icon
        {
            get
            {
                if (Folder)
                {
                    //return "&#xE8B7;";
                    return "\uE8B7";
                }
                else {
                    switch (Mime) {
                        case "MOV File":
                        case "AVI File":
                        case "MPEG File":
                            return "\uE8B2";
                        case "JPG picture":
                        case "PNG picture":
                        case "BMP picture":
                            return "\uEB9F";
                        case "WAV file":
                        case "MP3 File":
                            return "\uE189";
                        case "Recycle Bin":
                            return "\uE107 ";
                        case "ZIP file":
                        case "GZ File":
                        case "RAR File":
                        case "Text File":
                        default:
                            return "\uE160";
                    }
                }
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
