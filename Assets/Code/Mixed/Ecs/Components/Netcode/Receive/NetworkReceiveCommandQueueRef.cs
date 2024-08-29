using GSUnity.Netcode.Serializers;
using Unity.Entities;

namespace GSUnity.Ecs.Components.Netcode
{
    public class NetworkReceiveCommandQueueRef : IComponentData
    {
        public NetworkCommandQueue Value;
    }
}