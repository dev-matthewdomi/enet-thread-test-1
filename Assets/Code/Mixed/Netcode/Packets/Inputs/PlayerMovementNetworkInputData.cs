using System;
using Unity.Mathematics;

namespace GSUnity.Netcode.Packets.Inputs
{
    public struct PlayerMovementNetworkInputData : IEquatable<PlayerMovementNetworkInputData>
    {
        public int2 TargetPositionQuantized;

        public bool Equals(PlayerMovementNetworkInputData other)
        {
            return TargetPositionQuantized.Equals(other.TargetPositionQuantized);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerMovementNetworkInputData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TargetPositionQuantized.GetHashCode();
        }
    }
}