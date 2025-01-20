using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.HandyLib.Logging.Definitions
{
    public struct StreamScope
    {
        public string IsAsync
        {
            get;
            set;
        }
        public string Client
        {
            get;
            set;
        }
        public string Action
        {
            get;
            set;
        }
        public string RequestId
        {
            get;
            set;
        }
    }
}
