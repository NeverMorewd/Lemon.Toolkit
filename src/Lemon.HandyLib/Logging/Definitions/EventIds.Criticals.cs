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
        public static EventId CriticalDefault = new EventId(500);
        public static EventId CriticalCrash = new EventId(550);
    }
}
