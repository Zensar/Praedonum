using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praedonum.Modules
{
    public class Prefix
    {
        public string Protocol { get; set; } = "http";

        public string Host { get; set; }

        public int Port { get; set; } = 3389;

        public override string ToString()
        {
            return string.Format("{0}://{1}:{2}/", Protocol, Host, Port);
        }
    }
}
