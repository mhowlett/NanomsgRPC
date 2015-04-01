using NanomsgRPC.API;

namespace Test.API
{
    public class Commands
    {
        public static void SetMessage(INanoConnection c, int message)
        {
            using (var request = new NanoNetworkCommand.Request(c, Component.ComponentId, (byte)CommandIds.SetNumber))
            {
                request.BinaryWriter.Write(message);
            }

            using (new NanoNetworkCommand.Response(c))
            {
            }
        }

        public static int GetNumber(INanoConnection c)
        {
            using (new NanoNetworkCommand.Request(c, Component.ComponentId, (byte) CommandIds.GetNumber))
            {
            }

            using (var response = new NanoNetworkCommand.Response(c))
            {
                return response.BinaryReader.ReadInt32();
            }
        }

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
    }
}
