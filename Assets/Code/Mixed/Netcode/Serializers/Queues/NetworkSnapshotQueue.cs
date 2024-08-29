using GSUnity.External;
using GSUnity.Netcode.Packets.Snapshots;

namespace GSUnity.Netcode.Serializers
{
    public class NetworkSnapshotQueue
    {
        public RingBuffer<PlayerMovementNetworkSnapshotData> PlayerMovement;
    }
}