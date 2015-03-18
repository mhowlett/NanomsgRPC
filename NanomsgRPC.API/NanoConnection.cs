using System;
using NanomsgRPC.API;

namespace NanomsgRPC
{
    public class NanoConnection : IDisposable, INanoConnection
    {
        private readonly NanoConnectionPool _connectionPool;

        public NanoConnection(int socket, NanoConnectionPool pool, string address)
        {
            Socket = socket;
            _connectionPool = pool;
            Address = address;
        }

        public string Address { get; set; }

        public TimeSpan ConnectionTimeout
        {
            get { return _connectionPool.ConnectionTimeout; }
        }

        public int Socket { get; set; }

        public void Dispose()
        {
            _connectionPool.UsageGiveBack(this);
        }
    }
}
