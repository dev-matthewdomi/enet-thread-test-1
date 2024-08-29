using System;

namespace GSUnity.Ecs.Components.Network
{
    public struct NetworkEntityId : IEquatable<NetworkEntityId>
    {
        public uint Value;

        public bool Equals(NetworkEntityId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkEntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }
    }
}