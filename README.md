# NanomsgRPC

This is lightweight remote procedure call (RPC) framework for .NET that
utilizes NNanomsg for robust message delivery.

The API is straight forward to use. It doesn't impose any particular
serialization method on you, but you specify message data using 
BinaryReaders and BinaryWriters so it's really easy to use just these
(and there is some overhead in providing this interface, so it's kind
of wasted if you don't take advantage of it!).
 
On the client side, an example method implementation is as follows:

    public static double AddNumbers(INanoConnection c, double a, double b)
    {
        using (var request = new NanoNetworkCommand.Request(c, Component.ComponentId, (byte)CommandIds.AddNumbers))
        {
            request.BinaryWriter.Write(a);
            request.BinaryWriter.Write(b);
        }

        using (var response = new NanoNetworkCommand.Response(c))
        {
            return response.BinaryReader.ReadDouble();
        }
    }

Some things to note:

1. The request message is sent when the NanoNetworkCommand.Request object is disposed of
   (at the end of the using block).
2. Each command has a unique id. This is specified as a byte, allowing for a maximum of
   256 methods per interface.
3. All requests also include a component id (another byte). This is a useful check in 
   systems where you have more than one RPC interface - An exception will be thrown if
   you attempt to call a command intended for one component using a connection to a 
   different one.

On the client side, you first set everything up:

    var handlers = new Dictionary<byte, NanoNetworkListener.NetworkHandlerDelegate>
    {
        {(byte) CommandIds.SetNumber, Commands.SetNumber},
        {(byte) CommandIds.GetNumber, Commands.GetNumber},
        {(byte) CommandIds.AddNumbers, Commands.AddNumbers}
    };

    NanoNetworkListener.SetupServerSockets(
        new List<int> { port },
        new Dictionary<int, Dictionary<byte, NanoNetworkListener.NetworkHandlerDelegate>> { { port, handlers } },
        new Dictionary<int, byte> { { port, 0 } },
        new Dictionary<int, object> { { port, typeof(CommandIds) } },
        TimeSpan.FromSeconds(int.Parse(ConfigurationSettings.AppSettings["connection-timeout-seconds"])),
        LogMessage
    );

Then enter the main event loop:

    NanoNetworkListener.Start(null);

Some things to note:

[work in progress, more coming soone!]