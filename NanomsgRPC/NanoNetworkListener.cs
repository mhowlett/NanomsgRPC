using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using NNanomsg;

namespace NanomsgRPC
{
    public class NanoNetworkListener
    {
        public delegate void LogMessageDelegate(TraceLevel level, string message);

        private static NanomsgListener _listener = new NanomsgListener();
        private static Dictionary<int, int> _socketPorts = new Dictionary<int, int>();

        public static void DestroyServerSockets()
        {
            foreach (var kvp in _socketPorts)
            {
                NN.Close(kvp.Key);
            }

            _socketPorts = new Dictionary<int, int>();
            _listener = new NanomsgListener();
        }

        // temporary hack whilst NN.Bind aborts rather than returns -1
        // possible race condition.
        public static bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }

            return inUse;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static int SetupServerSocketsFindPort(
            int portRangeStart, int portRangeEnd, byte componentId,
            Dictionary<byte, NetworkHandlerDelegate> handlers,
            object debug_commandIdType,
            TimeSpan commsTimeout,
            LogMessageDelegate logMessage)
        {
            _listener.ReceivedMessage += delegate(int s)
                {
                    byte component = 255;
                    byte command = 255;

                    try
                    {
                        byte[] data;

                        NN.SetSockOpt(s, SocketOption.RCVTIMEO, (int)commsTimeout.TotalMilliseconds);
                        NN.Recv(s, out data, SendRecvFlags.NONE);

                        if (data == null || data.Length == 0)
                        {
                            throw new Exception("received empty command.");
                        }

                        using (var msi = new MemoryStream(data))
                        using (var br = new BinaryReader(msi))
                        using (var mso = new MemoryStream())
                        using (var bw = new BinaryWriter(mso))
                        {
                            component = br.ReadByte();
                            if (component != componentId)
                            {
                                throw new Exception("Component with id " + componentId +
                                                    " errantly received a command for component " + component);
                            }
                            command = br.ReadByte();
                            if (debug_commandIdType != null)
                            {
                                logMessage(TraceLevel.Verbose, ">> " + Enum.GetName((Type)debug_commandIdType, command));
                            }
                            handlers[command](br, bw);
                            NN.SetSockOpt(s, SocketOption.SNDTIMEO, (int)commsTimeout.TotalMilliseconds);
                            var rc = NN.Send(s, mso.ToArray(), SendRecvFlags.NONE);

                            if (rc < 0)
                            {
                                if (debug_commandIdType != null)
                                {
                                    logMessage(TraceLevel.Verbose, "!< ERROR sending reply: " + Enum.GetName((Type)debug_commandIdType, command));
                                }
                                else
                                {
                                    logMessage(TraceLevel.Verbose, "!< ERROR sending reply, ignoring.");
                                }
                            }
                            else
                            {
                                if (debug_commandIdType != null)
                                {
                                    logMessage(TraceLevel.Verbose, " < " + Enum.GetName((Type)debug_commandIdType, command));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logMessage(TraceLevel.Error, "Error handling command [component id: " + component + "] [command id: " + command + "]: " + Utils.UnrolledExceptionMessage(e));
                    }
                };


            var socket = NN.Socket(Domain.SP, Protocol.REP);
            NN.SetSockOpt(socket, SocketOption.SNDBUF, 5000000);
            NN.SetSockOpt(socket, SocketOption.RCVBUF, 5000000);

            for (int portTry = portRangeStart; portTry <= portRangeEnd; ++portTry)
            {
                if (PortInUse(portTry))
                {
                    continue;
                }

                try
                {
                    var rc = NN.Bind(socket, "tcp://*:" + portTry);
                    if (rc < 0)
                    {
                        continue;
                    }
                }
                catch
                {
                    Thread.ResetAbort();
                    continue;
                }

                _socketPorts.Add(socket, portTry);
                _listener.AddSocket(socket);

                return portTry;
            }
            
            throw new Exception("no free ports available in range " + portRangeStart + " -> " + portRangeEnd);
        }

        public static void SetupServerSockets(
            List<int> ports,
            Dictionary<int, Dictionary<byte, NetworkHandlerDelegate>> handlers,
            Dictionary<int, byte> componentIds,
            Dictionary<int, object> debug_commandIdTypes,
            TimeSpan commsTimeout,
            LogMessageDelegate logMessage)
        {
            _listener.ReceivedMessage += delegate(int s)
                {
                    byte component = 255;
                    byte command = 255;
                    int port = -1;

                    try
                    {
                        byte[] data;

                        NN.SetSockOpt(s, SocketOption.RCVTIMEO, (int)commsTimeout.TotalMilliseconds);
                        NN.Recv(s, out data, SendRecvFlags.NONE);

                        if (data == null || data.Length == 0)
                        {
                            throw new Exception("data was ready to be received but wasn't. I don't expect this should happen.");
                        }

                        using (var msi = new MemoryStream(data))
                        using (var br = new BinaryReader(msi))
                        using (var mso = new MemoryStream())
                        using (var bw = new BinaryWriter(mso))
                        {
                            component = br.ReadByte();
                            command = br.ReadByte();
                            port = _socketPorts[s];
                            if (componentIds[port] != component)
                            {
                                throw new Exception("Component with id " + componentIds[port] +
                                                    " errantly received a command for component " + component);
                            }
                            if (debug_commandIdTypes[port] != null)
                            {
                                logMessage(TraceLevel.Verbose, ">> " + Enum.GetName((Type)debug_commandIdTypes[port], command));
                            }
                            handlers[port][command](br, bw);

                            NN.SetSockOpt(s, SocketOption.SNDTIMEO, (int)commsTimeout.TotalMilliseconds);
                            var rc = NN.Send(s, mso.ToArray(), SendRecvFlags.NONE);

                            if (rc < 0)
                            {
                                if (debug_commandIdTypes[port] != null)
                                {
                                    logMessage(TraceLevel.Verbose, "!< ERROR sending reply: " +
                                                      Enum.GetName((Type)debug_commandIdTypes[port], command));
                                }
                                else
                                {
                                    logMessage(TraceLevel.Verbose, "!< ERROR sending reply, ignoring.");
                                }
                            }
                            else
                            {
                                if (debug_commandIdTypes[port] != null)
                                {
                                    logMessage(TraceLevel.Verbose, " < " + Enum.GetName((Type)debug_commandIdTypes[port], command));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logMessage(TraceLevel.Error, "Error handling command [component id: " + component + "] [command id: " + command + "] [port " + port + "]: " + Utils.UnrolledExceptionMessage(e));
                    }
                };

            // Create the server socket on which to listen, bind them, and start listening.
            for (int i = 0; i < ports.Count; ++i)
            {
                var socket = NN.Socket(Domain.SP, Protocol.REP);
                NN.SetSockOpt(socket, SocketOption.SNDBUF, 5000000);
                NN.SetSockOpt(socket, SocketOption.RCVBUF, 5000000);

                NN.Bind(socket, "tcp://*:" + ports[i]);

                _listener.AddSocket(socket);
                _socketPorts.Add(socket, ports[i]);
            }
        }

        public delegate void NetworkHandlerDelegate(BinaryReader reader, BinaryWriter writer);

        public enum LoopFunctionAction
        {
            ExitLoop,
            NoAction
        }

        public static void Start(TimeSpan? duration)
        {
            DateTime startTime = DateTime.UtcNow;
            while (true)
            {
                if (duration == null)
                {
                    _listener.Listen(null);
                }
                else
                {
                    var elapsedTime = DateTime.UtcNow - startTime;
                    if (elapsedTime > duration.Value)
                    {
                        break;
                    }
                    var pollTime = duration.Value - elapsedTime;
                    _listener.Listen(pollTime);
                }
            }
        }

        public static class Utils
        {
            public static string UnrolledExceptionMessage(Exception e)
            {
                int cnt = 1;
                string result = "" + cnt + ": " + e.Message;
                while (e.InnerException != null)
                {
                    result += " " + cnt++ + ": " + e.InnerException.Message;
                    e = e.InnerException;
                    if (cnt > 10)
                    {
                        break;
                    }
                }
                return result;
            }
        }
    }
}
