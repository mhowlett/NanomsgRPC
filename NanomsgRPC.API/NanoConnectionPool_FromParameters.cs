using System;

namespace NanomsgRPC.API
{
    public class NanoConnectionPool_FromParameters : NanoConnectionPool
    {
        private readonly string _host;
        private readonly int _port;
        private readonly int _connectionPoolSize;
        private readonly string _connectionTypeName;
        private readonly TimeSpan _connectionTimeout;
        private readonly TimeSpan _maxWaitForAvailableConnection;

        public NanoConnectionPool_FromParameters(
            string host,
            int port,
            int connectionPoolSize,
            string connectionTypeName,
            TimeSpan connectionTimeout,
            TimeSpan maxWaitForAvailableConnection)
        {
            _host = host;
            _port = port;
            _connectionPoolSize = connectionPoolSize;
            _connectionTypeName = connectionTypeName;
            _connectionTimeout = connectionTimeout;
            _maxWaitForAvailableConnection = maxWaitForAvailableConnection;
        }

        public override string Host
        {
            get { return _host; }
        }

        public override int Port
        {
            get { return _port; }
        }

        public override int ConnectionPoolSize
        {
            get { return _connectionPoolSize; }
        }

        public override string ConnectionTypeName
        {
            get { return _connectionTypeName; }
        }

        public override TimeSpan ConnectionTimeout
        {
            get { return _connectionTimeout; }
        }

        public override TimeSpan MaxWaitForAvailableConnection
        {
            get { return _maxWaitForAvailableConnection; }
        }
    }
}
