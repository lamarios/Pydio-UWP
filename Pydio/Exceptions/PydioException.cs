using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pydio.Exceptions
{
    class PydioException : Exception
    {
        public new string Message;
        public PydioException(string message)
        {
           System.Diagnostics.Debug.WriteLine("Pydi Exception: " + message);

            this.Message = message;
        }


    }
}
