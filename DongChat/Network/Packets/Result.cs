using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network.Packets
{
    [Flags]
    public enum Result : int
    {
        None = 0,               // Explicitly set 0
        Success = 1 << 0,       // 1
        NotAuthorized = 1 << 1, // 2
        Failed = 1 << 2,        // 4
        NotFound = 1 << 3       // 8
    }
}
