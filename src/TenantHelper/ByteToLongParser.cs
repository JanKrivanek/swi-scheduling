using System;
using SolarWinds.Messaging.Utils.Abstractions;

namespace TenantHelper
{
    internal class ByteToLongParser : IDataParser<long, byte[]>
    {
        private ByteToLongParser()  { }
        public static readonly ByteToLongParser Instance = new ByteToLongParser();

        public long ConvertToContent(byte[] message)
        {
            return BitConverter.ToInt64(message, 0);
        }

        public byte[] ConvertToMessage(long content)
        {
            return BitConverter.GetBytes(content);
        }
    }
}