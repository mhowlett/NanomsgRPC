using System.IO;
using System.Text;

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

        private static readonly UTF8Encoding Encoding = new UTF8Encoding();

        /// <summary>
        ///     Writes a string value in a format that is easy to interpret without 
        ///     using a BinaryReader. The first byte is 0 if the string is null,
        ///     1 otherwise. The Next 4 bytes are an integer value equal to the 
        ///     length of the UTF8 encoded string. The remaining bytes are the 
        ///     UTF8 encoding of the string.
        /// </summary>
        public static void WriteInteroperableString(this BinaryWriter writer, string s)
        {
            if (s == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)1);
            var bytes = Encoding.GetBytes(s);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
    }
}
