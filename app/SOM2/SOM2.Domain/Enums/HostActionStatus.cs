using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Domain.Enums
{
    public enum HostActionStatus
    {
        Pending = 1,
        Running = 2,
        Success = 3,
        Failed = 4,
        Cancelled = 5
    }
}
