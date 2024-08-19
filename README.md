## About

C#异步网络框架，加入了mysql数据库支持、MPPC压缩算法的加密解密，RC4对等加密。

## How to use

#首先你需要一个IPacket子类
``` 
public class WorldPacket : IPacket
{
    //操作码
    public virtual uint PacketOpcode { get; }

    protected ByteBuffer PacketBuffer => byteBuffer;
    private ByteBuffer byteBuffer;

    public WorldPacket()
    {
        byteBuffer = new ByteBuffer();
    }

    //获取包长的方法
    public virtual uint GetLength(ByteBuffer buffer)
    {
        return buffer.ReadUint32();
    }

    //获取操作码的方法
    public virtual uint GetOpcode(ByteBuffer buffer)
    {
        return buffer.ReadUint32();
    }

    //解包的方法
    public virtual void Read(ByteBuffer buffer)
    {

    }

    //封包的方法
    public virtual void Write(ByteBuffer buffer)
    {
        buffer.WriteUint32(PacketOpcode);
        buffer.WriteUint32((uint)byteBuffer.Length);
        buffer.Write(byteBuffer.Data);
    }
}

```

#一个BaseSocket<IPacket>的子类

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

#一个包处理PacketHandler<IPacket>的子类作为所有包处理函数注册的父类

```
public class BaseHandler : PacketHandler<WorldPacket>
{
}
```

#一个启动示例:

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
        Console.Write("请输入操作(q-退出):");
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

#一个包处理函数示例

```
[PacketHandler((uint)WorldOpCode.Ping)]
private void OnPing(AuthSocket socket, PingPacket packet)
{

}
```