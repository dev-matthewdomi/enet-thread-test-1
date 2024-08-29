using System;

namespace GSUnity.Netcode.Serializers
{
    [Flags]
    public enum NetcodeSerializerType
    {
        Serializer     = 1 << 0,
        Deserializer   = 1 << 1
    }
}