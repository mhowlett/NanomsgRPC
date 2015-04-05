using System.IO;
using System.Text;

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

        private static readonly UTF8Encoding Encoding = new UTF8Encoding();

        /// <summary>
        ///     Refer to WriteInteroperableString summary.
        /// </summary>
        public static string ReadInteroprableString(this BinaryReader reader)
        {
            byte isNull = reader.ReadByte();
            if (isNull == 0)
            {
                return null;
            }

            var length = reader.ReadInt32();
            var bytes = reader.ReadBytes(length);
            return Encoding.GetString(bytes);
        }
    }
}
