using System;

namespace NetWorkLibrary.Network
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PacketHandlerAttribute : Attribute
    {
        public uint Opcode { get; private set; }

        public PacketHandlerAttribute(uint opcode)
        {
            Opcode = opcode;
        }
    }
}
