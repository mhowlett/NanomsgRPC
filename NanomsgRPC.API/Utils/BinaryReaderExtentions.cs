using System.IO;

namespace NanomsgRPC.API.Utils
{
    public static class BinaryReaderExtentions
    {
        public static string ReadNullableString(this BinaryReader reader)
        {
            byte isNull = reader.ReadByte();
            if (isNull == 1)
            {
                return null;
            }
            return reader.ReadString();
        }
    }
}
