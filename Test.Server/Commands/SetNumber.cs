using System.IO;

namespace Test.Server
{
    public partial class Commands
    {
        public static void SetNumber(BinaryReader reader, BinaryWriter writer)
        {
            DataStore.SetNumber(reader.ReadInt32());
        }
    }
}
