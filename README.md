# NanomsgRPC

This is lightweight remote procedure call (RPC) framework for .NET that
utilizes NNanomsg for robust message delivery.

Request/Response data is specified using BinaryReader/Writers. In 
practice it is straightforward to do this manually, though you 
could pack your data using something like protobuf if you want.
There is no IDL.

On the client side, an example method implementation is as follows:

    public static double AddNumbers(INanoConnection c, double a, double b)
    {
        using (var request = new NanoNetworkCommand.Request(c, interfaceId, (byte)CommandIds.AddNumbers))
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
3. All requests also include a interface id (another byte). This is a useful check in 
   systems where you have more than one RPC interface - An exception will be thrown if
   you attempt to call a command intended for one interface (component) using a connection
   you also might have open to a different one.
4. There is no support for async calls.

On the server side, the command AddNumbers might be implemented as follows:

    public static void AddNumbers(BinaryReader reader, BinaryWriter writer)
    {
        var a = reader.ReadDouble();
        var b = reader.ReadDouble();
        writer.Write(a + b);
    }

To set the server listening, first set everything up:

    var handlers = new Dictionary<byte, NanoNetworkListener.NetworkHandlerDelegate>
    {
        {(byte) CommandIds.SetNumber, Commands.SetNumber},
        {(byte) CommandIds.GetNumber, Commands.GetNumber},
        {(byte) CommandIds.AddNumbers, Commands.AddNumbers}
    };

    NanoNetworkListener.SetupServerSockets(
        // Command handlers [vs port number]
        new Dictionary<int, Dictionary<byte, NanoNetworkListener.NetworkHandlerDelegate>> { { port, handlers } },
        // Interface ids [vs port number]
        new Dictionary<int, byte> { { port, 0 } },
        // Command ids enum [vs port number]. note this is required for logging only.
        new Dictionary<int, object> { { port, typeof(CommandIds) } },
        TimeSpan.FromSeconds(int.Parse(ConfigurationSettings.AppSettings["connection-timeout-seconds"])),
        LogMessage
    );

Then enter the main event loop:

    NanoNetworkListener.Start(null);

Some things to note:

1. NanoNetworkListener supports listening on multiple ports at once. Check out
   the code for more details.
2. Commands are not handled concurrently, so there is no need to worry about
   threading issues.
