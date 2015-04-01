using System.IO;

namespace NanomsgRPC.API.Utils
{
    public static class BinaryWriterExtentions
    {
        public static void WriteNullableString(this BinaryWriter writer, string s)
        {
            if (s == null)
            {
                writer.Write((byte) 1);
                return;
            }
            writer.Write((byte) 0);
            writer.Write(s);
        }
    }
}
