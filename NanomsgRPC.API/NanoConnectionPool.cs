using System;
using System.Collections.Generic;
using System.Threading;
using NNanomsg;

namespace NanomsgRPC.API
{
    public abstract class NanoConnectionPool : IDisposable
    {
        public abstract string Host { get; }

        public abstract int Port { get; }

        public abstract int ConnectionPoolSize { get; }

        public abstract string ConnectionTypeName { get; }

        public abstract TimeSpan ConnectionTimeout { get; }

        public abstract TimeSpan MaxWaitForAvailableConnection { get; }

        private void CreateAndConnectSocket(NanoConnection c)
        {
            c.Socket = NN.Socket(Domain.SP, Protocol.REQ);
            if (c.Socket < 0)
            {
                Dispose();
                throw new Exception("failed to create SP/REQ socket.");
            }

            int ret = NN.Connect(c.Socket, c.Address);
            if (ret < 0)
            {
                Dispose();
                throw new Exception("failed to open connection to: " + c.Address);
            }
        }

        public void CloseOpenAllConnections()
        {
            foreach (var c in _clients)
            {
                NN.Close(c.Socket); // could return -1 (error). todo: handle better.
                CreateAndConnectSocket(c);
            }
        }

        public void CloseOpenConnection(NanoConnection c)
        {
            NN.Close(c.Socket); // could return -1 (error). todo: handle better.
            CreateAndConnectSocket(c);
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
                var c = new NanoConnection(-1, this, address);
                CreateAndConnectSocket(c);
                _clients.Enqueue(c);
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

        public void Dispose()
        {
            foreach (var c in _clients)
            {
                try
                {
                    NN.Close(c.Socket);
                }
                catch { }
            }
        }
    }
}
