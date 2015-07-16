using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJI
{
    public enum SyncronizerStatus
    {
        Unknown = 0,
        Stopped = 1,
        Processing = 2,
        Sleeping = 3,
        Error = 4
    }
}
