using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Net
{
    [Flags]
    public enum PackageHeadFlags : ushort
    {
        Partial = 0x0001,     // 包不完全标志，即实际包长 > ushort.MaxValue需要拆包发送
    }
}
