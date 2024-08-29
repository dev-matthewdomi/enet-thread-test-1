using GSUnity.External;
using GSUnity.Netcode.Packets.Inputs;

namespace GSUnity.Netcode.Serializers
{
    public struct NetworkInputQueue
    {
        public readonly RingBuffer<PlayerMovementNetworkInputData> PlayerMovement;
    }
}