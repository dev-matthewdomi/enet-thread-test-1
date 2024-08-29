using GSUnity.Netcode.Packets.Inputs;
using Unity.Collections;

namespace GSUnity.Netcode.Serializers.History
{
    public struct NetworkInputHistory
    {
        public NativeHashSet<PlayerMovementNetworkInputData> PlayerMovement;
    }
}