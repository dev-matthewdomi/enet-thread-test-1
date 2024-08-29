using GSUnity.Ecs.Components.Network;
using GSUnity.Netcode.Packets.Snapshots;
using Unity.Collections;

namespace GSUnity.Netcode.Serializers.History
{
    public struct NetworkSnapshotHistory
    {
        public NativeHashMap<NetworkEntityId, PlayerMovementNetworkSnapshotData> PlayerMovement;
    }
}