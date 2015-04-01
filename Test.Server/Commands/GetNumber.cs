using System.IO;

namespace Test.Server
{
    public partial class Commands
    {
        public static void GetNumber(BinaryReader reader, BinaryWriter writer)
        {
            writer.Write(DataStore.GetNumber());
        }
    }
}
