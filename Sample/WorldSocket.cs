using NetWorkLibrary.Network;
using NetWorkLibrary.Utility;
using System.Net.Sockets;

namespace NetWorkLibrary.Sample
{
    public class WorldSocket : BaseSocket<WorldPacket>
    {
        public WorldSocket(Socket socket) : base(socket)
        {
        }

        public override void Open()
        {

        }

        public void ProcessWrite(WorldPacket packet)
        {
            if (packet != null)
            {
                ByteBuffer byteBuffer = new ByteBuffer();
                packet.Write(byteBuffer);
                ProcessWrite(byteBuffer.Data);
            }
        }
    }
}
