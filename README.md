## About

C#�첽�����ܣ�������mysql���ݿ�֧�֡�MPPCѹ���㷨�ļ��ܽ��ܣ�RC4�Եȼ��ܡ�

## How to use

#��������Ҫһ��IPacket����
``` 
public class WorldPacket : IPacket
{
    //������
    public virtual uint PacketOpcode { get; }

    protected ByteBuffer PacketBuffer => byteBuffer;
    private ByteBuffer byteBuffer;

    public WorldPacket()
    {
        byteBuffer = new ByteBuffer();
    }

    //��ȡ�����ķ���
    public virtual uint GetLength(ByteBuffer buffer)
    {
        return buffer.ReadUint32();
    }

    //��ȡ������ķ���
    public virtual uint GetOpcode(ByteBuffer buffer)
    {
        return buffer.ReadUint32();
    }

    //����ķ���
    public virtual void Read(ByteBuffer buffer)
    {

    }

    //����ķ���
    public virtual void Write(ByteBuffer buffer)
    {
        buffer.WriteUint32(PacketOpcode);
        buffer.WriteUint32((uint)byteBuffer.Length);
        buffer.Write(byteBuffer.Data);
    }
}

```

#һ��BaseSocket<IPacket>������

```
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
```

#һ��������PacketHandler<IPacket>��������Ϊ���а�������ע��ĸ���

```
public class BaseHandler : PacketHandler<WorldPacket>
{
}
```

#һ������ʾ��:

```
public class WorldSocketManager : SocketManager<WorldSocket>
{
    public WorldSocketManager() : base()
    {
        LogManager.Instance.AddLogger(new ConsoleLog());
        LogManager.Instance.AddLogger(new FileLog("WorldServer.log"));
    }

    protected override void SocketAdded(WorldSocket sock)
    {
        base.SocketAdded(sock);
        LogManager.Instance.Log(LogType.Message, $"[{sock.LinkIP}] connected.");
    }

    protected override void SocketRemoved(WorldSocket sock)
    {
        base.SocketRemoved(sock);
        LogManager.Instance.Log(LogType.Message, $"[{sock.LinkIP}] shut down.");
    }

    public static void ServerLoop()
    {
        WorldSocketManager socketManager = new WorldSocketManager();
        socketManager.Open("localhost", 20001);
        Console.Write("���������(q-�˳�):");
        Console.WriteLine();

        while (socketManager.IsRunning())
        {
            var code = Console.ReadLine().Trim();
            switch (code)
            {
                case "q":
                    socketManager.Close();
                    Task.Delay(5000).Wait();
                    return;
                default:
                    break;
            }
        }
    }
}
```

#һ����������ʾ��

```
[PacketHandler((uint)WorldOpCode.Ping)]
private void OnPing(AuthSocket socket, PingPacket packet)
{

}
```