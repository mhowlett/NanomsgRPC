using System;
using NanomsgRPC.API;

namespace Test.Client
{
    class Program
    {
        static void Main()
        {
            // Note: Quite unnecesaary to have connection pooling for this simple example 
            //   - NanoConnection_Simple would have sufficed.
            TestConnectionPool.Instance = new NanoConnectionPool_FromConfig(
                "TestComponent", "test-port", "test-host",
                "test-connection-pool-size", "test-connection-timeout-seconds",
                null);
            TestConnectionPool.Instance.OpenConnections();

            using (var c = TestConnectionPool.Instance.AcquireConnection())
            {
                Console.WriteLine(Test.API.Commands.AddNumbers(c, 10, 20));
                Test.API.Commands.SetMessage(c, 42);
                Console.WriteLine(Test.API.Commands.GetNumber(c));
            }
        }
    }
}
