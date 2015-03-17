using System;
using System.IO;
using NNanomsg;

namespace NanomsgRPC.API
{
    public class NanoNetworkCommand
    {
        public class NanoNetworkCommandException : Exception
        {
            public NanoNetworkCommandException() {}
            public NanoNetworkCommandException(string message) : base(message) {}
            public NanoNetworkCommandException(string message, Exception inner) : base(message, inner) {}
        }

        public class Request : IDisposable
        {
            public Request(INanoConnection c, byte componentId, byte commandId)
            {
                _connection = c;
                MemoryStream = new MemoryStream();
                BinaryWriter = new BinaryWriter(MemoryStream);
                BinaryWriter.Write(componentId);
                BinaryWriter.Write(commandId);
            }

            private INanoConnection _connection;
            private MemoryStream MemoryStream;
            public BinaryWriter BinaryWriter;

            public void Dispose()
            {
                NN.SetSockOpt(_connection.Socket, SocketOption.SNDTIMEO, (int)_connection.ConnectionTimeout.TotalMilliseconds);
                var rc = NN.Send(_connection.Socket, MemoryStream.ToArray(), SendRecvFlags.NONE);

                if (rc < 0)
                {
                    throw new NanoNetworkCommandException("Error sending request");
                }

                BinaryWriter.Close();
                MemoryStream.Dispose();
            }
        }

        public class Response : IDisposable
        {
            public Response(INanoConnection c)
            {
                byte[] buf;
                NN.SetSockOpt(c.Socket, SocketOption.RCVTIMEO, (int) c.ConnectionTimeout.TotalMilliseconds);
                if (NN.Recv(c.Socket, out buf, SendRecvFlags.NONE) < 0)
                {
                    throw new NanoNetworkCommandException("Error receiving response");
                }
                MemoryStream = new MemoryStream(buf);
                BinaryReader = new BinaryReader(MemoryStream);
            }

            private MemoryStream MemoryStream;
            public BinaryReader BinaryReader;

            public void Dispose()
            {
                BinaryReader.Close();
                MemoryStream.Dispose();
            }
        }

        public delegate T Del<T>();

        public delegate void CloseOpenDelegate(INanoConnection connection);

        public static T RetryUntilNoTimeout<T>(Del<T> del, INanoConnection connection, CloseOpenDelegate closeOpen)
        {
            while (true)
            {
                try
                {
                    return del();
                }
                catch (Exception e)
                {
                    if (!(e is NanoNetworkCommandException))
                    {
                        throw e;
                    }

                    // nanomsg should really never get stuck. Arbitrarily wait a bit.
                    closeOpen(connection);
                }
            }
        }

    }
}
