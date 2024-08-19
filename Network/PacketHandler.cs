using NetWorkLibrary.Utility;
using System;
using System.Reflection;

namespace NetWorkLibrary.Network
{
    public abstract class PacketHandler<TBasePacket> where TBasePacket : IPacket
    {
        private readonly string methodName = "CreateDelegate";

        protected PacketHandler()
        {
            MethodInfo createMethod = typeof(PacketHandler<TBasePacket>).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var methodInfo in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var handler = methodInfo.GetCustomAttribute<PacketHandlerAttribute>(false);
                if (handler != null)
                {
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length < 2) { continue; }

                    Type T1 = parameters[0].ParameterType;
                    Type T2 = parameters[1].ParameterType;

                    if (T1.IsSubclassOf(typeof(BaseSocket<TBasePacket>)) && T2.IsSubclassOf(typeof(TBasePacket)))
                    {
                        PacketHandlerAction action = (PacketHandlerAction)createMethod.MakeGenericMethod(T1, T2).Invoke(null, new object[] { this, methodInfo });
                        BaseSocket<TBasePacket>.RegisterPacketHandler(handler.Opcode, action);
                    }
                }
            }
        }

        private static PacketHandlerAction CreateDelegate<T1, T2>(object sender, MethodInfo method) where T1 : ISocket where T2 : IPacket
        {
            var d = method.CreateDelegate(typeof(Action<T1, T2>), sender);
            PacketHandlerAction p = delegate (ISocket socket, ByteBuffer byteBuffer)
            {
                T2 packet = Activator.CreateInstance<T2>();
                packet.Read(byteBuffer);
                d.DynamicInvoke((T1)socket, packet);
            };
            return p;
        }
    }
}
