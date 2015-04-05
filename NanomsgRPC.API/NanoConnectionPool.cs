using System;
using System.Collections.Generic;
using System.Threading;
using NNanomsg;

namespace NanomsgRPC.API
{
    public abstract class NanoConnectionPool
    {
        public abstract string Host { get; }

        public abstract int Port { get; }

        public abstract int ConnectionPoolSize { get; }

        public abstract string ConnectionTypeName { get; }

        public abstract TimeSpan ConnectionTimeout { get; }

        public abstract TimeSpan MaxWaitForAvailableConnection { get; }

        public void CloseOpenAllConnections()
        {
            foreach (var c in _clients)
            {
                NN.Close(c.Socket);
                c.Socket = NN.Socket(Domain.SP, Protocol.REQ);
                NN.Connect(c.Socket, c.Address);
            }
        }

        public void CloseOpenConnection(NanoConnection connection)
        {
            NN.Close(connection.Socket);
            connection.Socket = NN.Socket(Domain.SP, Protocol.REQ);
            NN.Connect(connection.Socket, connection.Address);
        }

        private Queue<NanoConnection> _clients;
        private readonly object _inUseLock = new object();

        public void OpenConnections()
        {
            if (ConnectionPoolSize <= 0)
            {
                throw new Exception("connection pool size must be greater than 0");
            }

            _clients = new Queue<NanoConnection>();
            for (int i = 0; i < ConnectionPoolSize; ++i)
            {
                string address = "tcp://" + Host + ":" + Port;
                int s = NN.Socket(Domain.SP, Protocol.REQ);

                NN.Connect(s, address);
                _clients.Enqueue(new NanoConnection(s, this, address));
            }
        }

        public NanoConnection AcquireConnection()
        {
            const int loopWaitMilliseconds = 20;
            int loopWaitMaxMilliseconds = (int)MaxWaitForAvailableConnection.TotalMilliseconds;
            int loopMax = loopWaitMaxMilliseconds / loopWaitMilliseconds;

            while (true)
            {
                if (loopMax-- < 0)
                {
                    throw new Exception("Could not acquire connection of type: " + ConnectionTypeName);
                }

                lock (_inUseLock)
                {
                    if (_clients.Count != 0)
                    {
                        return _clients.Dequeue();
                    }
                }

                Thread.Sleep(20);
            }
        }

        public void UsageGiveBack(NanoConnection connection)
        {
            lock (_inUseLock)
            {
                _clients.Enqueue(connection);
            }
        }

    }
}
