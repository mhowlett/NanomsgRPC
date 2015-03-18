using NanomsgRPC.API;

namespace Test.API
{
    public class Commands
    {
        public static void Shutdown(INanoConnection c)
        {
            using (new NanoNetworkCommand.Request(c, 0, (byte) CommandIds.Shutdown))
            {
            }

            using (new NanoNetworkCommand.Response(c))
            {
            }
        }

        public static void SetMessage(INanoConnection c, int message)
        {
            using (var request = new NanoNetworkCommand.Request(c, 0, (byte)CommandIds.Shutdown))
            {
                request.BinaryWriter.Write(message);
            }

            using (new NanoNetworkCommand.Response(c))
            {
            }
        }

        public static int SetMessage(INanoConnection c)
        {
            using (new NanoNetworkCommand.Request(c, 0, (byte)CommandIds.Shutdown))
            {
            }

            using (var response = new NanoNetworkCommand.Response(c))
            {
                return response.BinaryReader.ReadInt32();
            }
        }
    }
}
