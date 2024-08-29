using ENet;
using GSUnity.Netcode;
using Unity.Entities;

namespace GSUnity.Server.Ecs.Components.Netcode
{
    public struct NetworkRequestListen : IComponentData
    {
        public Address Address;
        public int PeerLimit;
    }
}