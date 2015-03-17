using System;

namespace NanomsgRPC.API
{
    public interface INanoConnection
    {
        TimeSpan ConnectionTimeout { get; }
        int Socket { get; }
    }
}
