using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using NanomsgRPC;
using Test.API;

namespace Test.Server
{
    class Program
    {
        static void LogMessage(TraceLevel level, string message)
        {
            Console.WriteLine(message);
        }

        static void Main()
        {
            var handlers = new Dictionary<byte, NanoNetworkListener.NetworkHandlerDelegate>
            {
                {(byte) CommandIds.SetNumber, Commands.SetNumber},
                {(byte) CommandIds.GetNumber, Commands.GetNumber},
                {(byte) CommandIds.AddNumbers, Commands.AddNumbers}
            };

            int port = int.Parse(ConfigurationSettings.AppSettings["port"]);

            NanoNetworkListener.SetupServerSockets(
                new List<int> { port },
                new Dictionary<int, Dictionary<byte, NanoNetworkListener.NetworkHandlerDelegate>> { { port, handlers } },
                new Dictionary<int, byte> { { port, 0 } },
                new Dictionary<int, object> { { port, typeof(CommandIds) } },
                TimeSpan.FromSeconds(int.Parse(ConfigurationSettings.AppSettings["connection-timeout-seconds"])),
                LogMessage
            );

            NanoNetworkListener.Start(null);
        }
    }
}
