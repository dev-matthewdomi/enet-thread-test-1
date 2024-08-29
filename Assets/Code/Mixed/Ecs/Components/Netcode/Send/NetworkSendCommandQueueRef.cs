using GSUnity.Netcode.Serializers;
using Unity.Entities;

namespace GSUnity.Ecs.Components.Netcode
{
    public class NetworkSendCommandQueueRef : IComponentData
    {
        public NetworkCommandQueue Value;
    }
}