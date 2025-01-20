using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.HandyLib.Logging.Definitions
{
    public static partial class EventIds
    {
        public static EventId InfoDefault = new EventId(200);
        public static EventId InfoAction = new EventId(201);
    }
}
