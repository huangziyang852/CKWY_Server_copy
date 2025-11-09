using System.Text;
using Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using IGrains.Handler;

namespace GateServer.Net
{
     public class WebSocketServerDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                if (input.ReadableBytes < 2)
                    return;

                input.MarkReaderIndex();

                byte b1 = input.ReadByte();
                byte b2 = input.ReadByte();

                bool fin = (b1 & 0x80) != 0;
                byte opcode = (byte)(b1 & 0x0F);
                bool masked = (b2 & 0x80) != 0;
                int payloadLen = b2 & 0x7F;

                if (!masked)
                {
                    context.CloseAsync();
                    return;
                }

                if (payloadLen == 126)
                {
                    if (input.ReadableBytes < 2)
                    {
                        input.ResetReaderIndex();
                        return;
                    }
                    payloadLen = input.ReadUnsignedShort();
                }
                else if (payloadLen == 127)
                {
                    if (input.ReadableBytes < 8)
                    {
                        input.ResetReaderIndex();
                        return;
                    }
                    payloadLen = (int)input.ReadLong();
                }

                if (input.ReadableBytes < 4 + payloadLen)
                {
                    input.ResetReaderIndex();
                    return;
                }

                byte[] mask = new byte[4];
                input.ReadBytes(mask);

                byte[] payload = new byte[payloadLen];
                input.ReadBytes(payload);
                for (int i = 0; i < payload.Length; i++)
                    payload[i] ^= mask[i % 4];

                if (payload.Length < 8)
                {
                    context.CloseAsync();
                    return;
                }

                using (var buffer = new MemoryStream(payload))
                using (var reader = new BinaryReader(buffer))
                {
                    byte[] bodyLenBytes = reader.ReadBytes(4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bodyLenBytes);
                    int bodyLength = BitConverter.ToInt32(bodyLenBytes, 0);

                    byte[] protoIDBytes = reader.ReadBytes(4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(protoIDBytes);
                    int protoID = BitConverter.ToInt32(protoIDBytes, 0);

                    if (bodyLength < 0 || bodyLength > (1024 * 8) || payload.Length < 8 + bodyLength)
                    {
                        context.CloseAsync();
                        return;
                    }

                    byte[] bodyData = reader.ReadBytes(bodyLength);

                    var netPackage = new NetPackage()
                    {
                        protoID = protoID,
                        bodyData = bodyData
                    };

                    output.Add(netPackage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket 解码异常: {ex.Message}");
            }
        }
    }
}

