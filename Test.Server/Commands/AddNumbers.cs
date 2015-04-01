using System.IO;

namespace Test.Server
{
    public partial class Commands
    {
        public static void AddNumbers(BinaryReader reader, BinaryWriter writer)
        {
            var a = reader.ReadDouble();
            var b = reader.ReadDouble();
            writer.Write(a + b);
        }
    }
}
