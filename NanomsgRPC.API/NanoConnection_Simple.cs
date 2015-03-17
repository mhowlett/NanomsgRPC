using System;

namespace NanomsgRPC.API
{
    public class NanoConnection_Simple : INanoConnection
    {
        public TimeSpan ConnectionTimeout { get; set; }
        public int Socket { get; set; }
    }
}
